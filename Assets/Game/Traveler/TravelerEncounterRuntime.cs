using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Diagnostics;
using Game.Enemy;
using Game.Pickup;
using Game.Pooling;
using Game.Run;
using UnityEngine;
namespace Game.Traveler
{
    [DefaultExecutionOrder(-50)]
    public sealed class TravelerEncounterRuntime : MonoBehaviour, ITravelerRuntime
    {
        private readonly List<TravelerLife> _lives = new List<TravelerLife>();
        private readonly List<EnemyRuntime> _targets = new List<EnemyRuntime>();
        private readonly HashSet<EnemyRuntime> _affected = new HashSet<EnemyRuntime>();
        private readonly List<EnemyRuntime> _scratch = new List<EnemyRuntime>();
        private readonly List<EnemyRuntime> _nearest = new List<EnemyRuntime>();
        private readonly List<TravelerPulseEffect> _pulses = new List<TravelerPulseEffect>();
        /// <summary>Visible length of one support/teleport pulse and the teleport flash size in body sizes (presentation timing, DECISION-0120).</summary>
        private const float PulseSeconds = .5f, HealPulseSeconds = .8f, TeleportFlashSizes = 3f;
        private GameObjectPool<TravelerPulseEffect> _effects;
        private GameObjectPool<EnemyRuntime> _pool;
        private GameObjectPool<EnemyProjectileRuntime> _projectiles;
        private IReadOnlyDictionary<ContentId, TravelerDefinition> _definitions;
        private TravelerScheduleDefinition _schedule;
        private RunController _run;
        private RunModel _model;
        private Transform _player;
        private Camera _camera;
        private WorldPickupRuntime _pickups;
        private PickupDefinition _book;
        private IEnemyLifecycleSink _xp;
        private TravelerPlacement _placement;
        private System.Random _random;
        private Game.Presentation.EnemyDeathPresentationProfile _deathPresentation;
        private Game.Presentation.GroundShadowPresentationProfile _groundShadowPresentation;
        private ContentRegistry _contentRegistry;
        private int _next, _sequence, _devIndex;
        public bool IsInitialized => _model != null;
        /// <summary>Seed actually driving the schedule draw and placement: a per-run seed from the
        /// composition root (DECISION-0057) or the schedule reference seed.</summary>
        public int Seed { get; private set; }
        public IReadOnlyList<TravelerScheduleEntry> Schedule { get; private set; } = Array.Empty<TravelerScheduleEntry>();
        public IReadOnlyList<TravelerSnapshot> Snapshot => _lives.Where(life => life.Actor != null && life.Actor.IsAlive).Select(life => new TravelerSnapshot(life, _model.RunId)).ToList().AsReadOnly();
        public string DevelopmentObservation => string.Join("\n", _lives.Select(life => $"{life.Definition.Id} · {life.Definition.Role} · {life.Actor.MovementPhase}/{life.Actor.AttackPhase} · spawn {life.SpawnTime:0.0}s / until {life.Deadline:0.0}s · ×{life.Scale:0.00}"));
        public event Action<TravelerEvent> LifeEvent;
        public event Action<Game.Combat.CombatResult> CombatResolved;
        /// <summary>Forwarded from every living Traveler body: a wind-up, volley or dash started (audio cues).</summary>
        public event Action<EnemyActionKind, Vector2> ActionStarted;
        public void Initialize(TravelerScheduleDefinition schedule, FixtureTravelerCatalog catalog, RunController run,
            Transform player, Camera camera, TravelerPlacement placement, WorldPickupRuntime pickups, PickupDefinition book,
            IEnemyLifecycleSink xp, Game.Presentation.EnemyDeathPresentationProfile deathPresentation = null,
            Game.Presentation.GroundShadowPresentationProfile groundShadowPresentation = null,
            Game.Content.ContentRegistry contentRegistry = null, int? seed = null)
        {
            if (schedule == null || catalog == null || run?.Model == null || player == null || camera == null || !camera.orthographic || placement == null || pickups == null || book?.Kind != PickupRewardKind.Book)
                throw new ArgumentException("Traveler dependencies required.");
            foreach (var id in schedule.TravelerIds) if (!catalog.Definitions.ContainsKey(id)) throw new ArgumentException("Missing Traveler definition.");
            var resolvedSeed = seed ?? schedule.Seed;
            var planned = schedule.Draw(run.Model.Duration, new System.Random(resolvedSeed), id => catalog.Definitions[id].Role);
            Shutdown();
            Seed = resolvedSeed;
            _schedule = schedule; _definitions = catalog.Definitions; _run = run; _model = run.Model;
            _player = player; _camera = camera; _placement = placement; _pickups = pickups; _book = book; _xp = xp;
            _deathPresentation = deathPresentation;
            _groundShadowPresentation = groundShadowPresentation;
            _contentRegistry = contentRegistry;
            Schedule = planned; _random = new System.Random(unchecked(resolvedSeed ^ 0x54726176));
            _pool ??= new GameObjectPool<EnemyRuntime>(EnemyFactory.CreateInstance, transform);
            _projectiles ??= new GameObjectPool<EnemyProjectileRuntime>(EnemyProjectileFactory.CreateInstance, transform);
            _effects ??= new GameObjectPool<TravelerPulseEffect>(TravelerPulseEffect.CreateInstance, transform);
            _model.StateChanged += HandleState;
        }
        private void Update() => Tick();
        public void Tick()
        {
            if (_model == null || _model.State != RunState.Running) return;
            using var guard = PerfGuard.Measure("Traveler.Tick", 5f);
            foreach (var life in _lives.ToArray())
            {
                if (_model.Elapsed >= life.Deadline) { life.Actor.Despawn(EnemyLifeReason.Escaped); continue; }
                var bounded = _placement.ClampToBounds(life.Actor.Position);
                if ((bounded - life.Actor.Position).sqrMagnitude > .000001f)
                    life.Actor.GetComponent<Rigidbody2D>().position = bounded;
            }
            while (_next < Schedule.Count && _model.State == RunState.Running && Schedule[_next].Time <= _model.Elapsed)
            {
                var entry = Schedule[_next++];
                // A large time step must not resurrect an encounter whose entire window has already elapsed.
                if (_model.Elapsed < entry.Time + _definitions[entry.Id].PresenceSeconds)
                    Spawn(entry.Id, entry.Time, entry.Scale, entry.HealthScale);
            }
            ApplyTeleports();
            UpdateSupport();
            TickEffects(Time.deltaTime);
        }
        public EnemyRuntime Spawn(ContentId id, float spawnTime, float scale)
        {
            return Spawn(id, spawnTime, scale, scale);
        }
        public EnemyRuntime Spawn(ContentId id, float spawnTime, float scale, float healthScale)
        {
            if (_model == null || _model.State != RunState.Running) return null;
            var definition = _definitions[id];
            if (!_placement.TrySpawn(_player.position, _camera.orthographicSize * 2 * _schedule.SpawnScreenHeights,
                _schedule.PlacementAttempts, _random, out var position)) throw new InvalidOperationException("Traveler spawn circle has no valid point inside the field and outside obstacles; field geometry/config invalid.");
            // Art comes from the unscaled body: Scale() rebuilds stats only (DECISION-0057).
            var body = EnemyBodyVisual.Resolve(definition.Body, _contentRegistry);
            var actor = EnemyFactory.Spawn(definition.Scale(healthScale, scale), position, _player, _run, transform,
                visual: body.Sprite, pool: _pool, projectilePool: _projectiles, category: EnemyCategory.Traveler,
                motionProfile: body.Motion, contact: body.Contact, deathPresentation: _deathPresentation,
                groundShadowPresentation: _groundShadowPresentation, contentRegistry: _contentRegistry);
            var life = new TravelerLife(actor, definition, spawnTime, scale, _sequence++);
            // The role color only tints placeholder squares; approved art keeps its own colors.
            if (body.Sprite == null) actor.GetComponent<SpriteRenderer>().color = definition.Color;
            _lives.Add(life);
            if (definition.Role != TravelerRole.Offensive)
                life.Driver = new TravelerMovementDriver(definition, _placement, _model.RunId, _random.Next());
            if (definition.Support == TravelerSupportKind.Aura)
            { life.AuraEffect = _effects.Rent(); life.AuraEffect.ShowSteady(actor.transform, definition.SupportRadius * 2f, definition.EffectColor, definition.SupportVerticalScale, definition.EffectShape); }
            actor.ConfigureEncounter(life.Driver, () => DamageAllowed(life));
            actor.LifeEvent += OnActorEvent;
            actor.Despawned += OnDespawn;
            actor.CombatResolved += ForwardCombat;
            actor.ActionStarted += ForwardAction;
            LifeEvent?.Invoke(new TravelerEvent(new TravelerSnapshot(life, _model.RunId), "Spawned"));
            if (_model == null || _model.State != RunState.Running) actor.Despawn();
            return actor;
        }
        private bool DamageAllowed(TravelerLife life)
        {
            if (_model == null || _model.State != RunState.Running) return false;
            if (_model.Elapsed < life.Deadline) return true;
            life.Actor.Despawn(EnemyLifeReason.Escaped); return false;
        }
        private void OnActorEvent(EnemyLifeEvent snapshot)
        {
            var life = _lives.FirstOrDefault(item => item.Actor.LifeId == snapshot.LifeId);
            if (life == null || snapshot.Kind != EnemyLifeEventKind.Died) return;
            RemoveSupport(life.Actor.LifeId); ReleaseAura(life);
            if (_model.State != RunState.Running || _model.Elapsed >= life.Deadline) return;
            _xp?.OnEnemyLifeEvent(snapshot);
            _pickups.Spawn(_book, snapshot.Position, snapshot.LifeId, snapshot.ContentId);
            LifeEvent?.Invoke(new TravelerEvent(new TravelerSnapshot(life, _model.RunId), "Killed"));
        }
        private void OnDespawn(EnemyRuntime actor)
        {
            var life = _lives.FirstOrDefault(item => item.Actor == actor);
            if (life == null) return;
            _lives.Remove(life); RemoveSupport(actor.LifeId); ReleaseAura(life);
            actor.LifeEvent -= OnActorEvent; actor.Despawned -= OnDespawn; actor.CombatResolved -= ForwardCombat; actor.ActionStarted -= ForwardAction;
            if (actor.LastLifeEvent.Reason != EnemyLifeReason.Killed)
                LifeEvent?.Invoke(new TravelerEvent(new TravelerSnapshot(life, _model.RunId), actor.LastLifeEvent.Reason == EnemyLifeReason.Escaped ? "Escaped" : "Cancelled"));
        }
        private void UpdateSupport()
        {
            if (_model == null) return;
            var supporting = false;
            foreach (var life in _lives) if (life.Definition.Support != TravelerSupportKind.None) { supporting = true; break; }
            if (!supporting && _affected.Count == 0) return;
            using var guard = PerfGuard.Measure("Traveler.Support", 3f);
            // No LINQ or per-call lists: one pass over the alive registry, then one pass per protector (DECISION-0120).
            EnemyRegistry.CopyAliveTo(_targets);
            var kept = 0;
            for (var i = 0; i < _targets.Count; i++)
            {
                var enemy = _targets[i];
                if (enemy.Category == EnemyCategory.Ordinary && enemy.Identity.RunId == _model.RunId) _targets[kept++] = enemy;
            }
            _targets.RemoveRange(kept, _targets.Count - kept);
            _scratch.Clear(); _scratch.AddRange(_affected);
            foreach (var previous in _scratch) if (previous == null || !previous.IsAlive) _affected.Remove(previous);
            for (var index = 0; index < _lives.Count; index++)
            {
                var life = _lives[index];
                var d = life.Definition;
                if (d.Support == TravelerSupportKind.None) continue;
                var source = life.Actor.LifeId;
                var origin = life.Actor.Position;
                var radiusSquared = d.SupportRadius * d.SupportRadius;
                var inverseScale = 1f / d.SupportVerticalScale;
                switch (d.Support)
                {
                    case TravelerSupportKind.Aura:
                        _scratch.Clear(); _scratch.AddRange(_affected);
                        foreach (var previous in _scratch)
                            if (Zone(previous.Position - origin, inverseScale) > radiusSquared) previous.Protection.RemoveAura(source);
                        foreach (var enemy in _targets)
                            if (Zone(enemy.Position - origin, inverseScale) <= radiusSquared)
                            { enemy.Protection.SetAura(source, d.Reduction, d.Resistance, life.Deadline); _affected.Add(enemy); }
                        break;
                    case TravelerSupportKind.Shield:
                        if (_model.Elapsed < life.NextSupportTime) break;
                        life.NextSupportTime = _model.Elapsed + d.SupportCooldown;
                        SelectNearest(origin, radiusSquared, d.SupportTargets, inverseScale);
                        foreach (var enemy in _nearest)
                        { enemy.Protection.CastShield(source, d.ShieldHp, Mathf.Min(d.ShieldSeconds, life.Deadline - _model.Elapsed), _model.Elapsed); _affected.Add(enemy); }
                        break;
                    case TravelerSupportKind.SpeedBurst:
                        if (_model.Elapsed < life.NextSupportTime) break;
                        CastSpeedBurst(life, source, origin, radiusSquared, inverseScale);
                        break;
                    case TravelerSupportKind.Heal:
                        if (_model.Elapsed < life.NextSupportTime) break;
                        life.NextSupportTime = _model.Elapsed + d.SupportCooldown;
                        foreach (var enemy in _targets)
                            if (Zone(enemy.Position - origin, inverseScale) <= radiusSquared) enemy.Health.Heal(d.HealAmount);
                        PlayPulse(origin, d.SupportRadius * 2f, d.EffectColor, d.SupportVerticalScale, d.EffectShape);
                        break;
                }
            }
            foreach (var enemy in _affected) enemy.Protection.Tick(_model.Elapsed);
        }
        // Support zones are flattened ground ellipses like the strike areas (DECISION-0058: the 3/4 camera); the test costs one extra multiply.
        private static float Zone(Vector2 offset, float inverseScale) => offset.x * offset.x + offset.y * offset.y * inverseScale * inverseScale;
        // DECISION-0120: a random ordinary enemy near the Traveler is picked (reservoir sampling, no list) and everything in the small radius around it speeds up.
        private void CastSpeedBurst(TravelerLife life, Guid source, Vector2 origin, float radiusSquared, float inverseScale)
        {
            var d = life.Definition;
            EnemyRuntime picked = null; var candidates = 0;
            foreach (var enemy in _targets)
                if (Zone(enemy.Position - origin, inverseScale) <= radiusSquared && _random.Next(++candidates) == 0) picked = enemy;
            if (picked == null) { life.NextSupportTime = _model.Elapsed + 1f; return; }
            life.NextSupportTime = _model.Elapsed + d.SupportCooldown;
            var center = picked.Position;
            var burstSquared = d.EffectRadius * d.EffectRadius;
            foreach (var enemy in _targets)
                if (Zone(enemy.Position - center, inverseScale) <= burstSquared)
                    enemy.Protection.SetSpeedBoost(source, d.SpeedBonus, _model.Elapsed + d.EffectSeconds, _model.Elapsed);
            PlayPulse(center, d.EffectRadius * 2f, d.EffectColor, d.SupportVerticalScale, d.EffectShape);
        }
        // The `count` nearest targets inside the radius, nearest first, ties by LifeId; insertion into a list bounded by `count`.
        private void SelectNearest(Vector2 origin, float radiusSquared, int count, float inverseScale)
        {
            _nearest.Clear();
            foreach (var enemy in _targets)
            {
                var distance = Zone(enemy.Position - origin, inverseScale);
                if (distance > radiusSquared) continue;
                var at = _nearest.Count;
                while (at > 0)
                {
                    var other = _nearest[at - 1];
                    var otherDistance = Zone(other.Position - origin, inverseScale);
                    if (otherDistance < distance || (otherDistance == distance && other.LifeId.CompareTo(enemy.LifeId) < 0)) break;
                    at--;
                }
                if (at >= count) continue;
                _nearest.Insert(at, enemy);
                if (_nearest.Count > count) _nearest.RemoveAt(count);
            }
        }
        private void PlayPulse(Vector2 position, float diameter, Color color, float verticalScale = 1f, TravelerEffectShape shape = TravelerEffectShape.Ring)
        {
            var effect = _effects.Rent();
            effect.PlayPulse(position, diameter, color,
                shape == TravelerEffectShape.Ripples ? HealPulseSeconds : PulseSeconds, verticalScale, shape);
            _pulses.Add(effect);
        }
        private void TickEffects(float deltaTime)
        {
            for (var i = _pulses.Count - 1; i >= 0; i--)
            {
                var pulse = _pulses[i];
                pulse.Tick(deltaTime);
                if (!pulse.IsFinished) continue;
                _pulses.RemoveAt(i); _effects.Return(pulse);
            }
            foreach (var life in _lives) life.AuraEffect?.Tick(deltaTime);
        }
        private void ReleaseAura(TravelerLife life)
        { if (life.AuraEffect == null) return; _effects.Return(life.AuraEffect); life.AuraEffect = null; }
        // Teleport of the orbiting mage: flash where it leaves and where it lands (DECISION-0120).
        private void ApplyTeleports()
        {
            foreach (var life in _lives)
            {
                if (life.Driver == null || life.Actor == null || !life.Driver.TryTakeTeleport(out var target)) continue;
                var flash = life.Definition.Body.CollisionSize * TeleportFlashSizes;
                PlayPulse(life.Actor.Position, flash, life.Definition.EffectColor);
                life.Actor.transform.position = target;
                life.Actor.GetComponent<Rigidbody2D>().position = target;
                PlayPulse(target, flash, life.Definition.EffectColor);
            }
        }
        private void RemoveSupport(Guid source)
        { foreach (var enemy in _affected) if (enemy != null) enemy.Protection.RemoveSource(source); }
        private void ForwardCombat(Game.Combat.CombatResult result) => CombatResolved?.Invoke(result);
        private void ForwardAction(EnemyActionKind kind, Vector2 position) => ActionStarted?.Invoke(kind, position);
        public IReadOnlyList<TravelerChoice> DevelopmentChoices => _schedule == null || _definitions == null
            ? Array.Empty<TravelerChoice>()
            : _schedule.TravelerIds.Select(id => new TravelerChoice(id.ToString(), _definitions[id].Name, _definitions[id].Role.ToString())).ToList().AsReadOnly();
        public void SpawnDevelopmentTraveler(string id)
        {
            if ((!Application.isEditor && !Debug.isDebugBuild) || _model == null || _schedule == null || string.IsNullOrWhiteSpace(id) || _lives.Count >= DevelopmentMaxAlive) return;
            var key = new ContentId(id);
            if (!_definitions.ContainsKey(key)) return;
            Spawn(key, _model.Elapsed, _schedule.Scale(_model.Elapsed, _model.Duration), _schedule.HealthScale(_model.Elapsed, _model.Duration));
        }
        private const int DevelopmentMaxAlive = 10;
        public void SpawnDevelopmentTraveler()
        {
            if ((!Application.isEditor && !Debug.isDebugBuild) || _model == null || _lives.Count >= 3) return;
            var id = _schedule.TravelerIds[_devIndex++ % _schedule.TravelerIds.Count];
            Spawn(id, _model.Elapsed, _schedule.Scale(_model.Elapsed, _model.Duration), _schedule.HealthScale(_model.Elapsed, _model.Duration));
        }
        private void HandleState(RunState state)
        {
            if (state == RunState.Running || state == RunState.Paused)
            { foreach (var life in _lives) { var body = life.Actor.GetComponent<Rigidbody2D>(); body.linearVelocity = Vector2.zero; body.simulated = state == RunState.Running; } }
            else Clear();
        }
        private void Clear()
        { foreach (var life in _lives.ToArray()) if (life.Actor != null) life.Actor.Despawn(); _lives.Clear(); _affected.Clear();
            foreach (var pulse in _pulses) _effects.Return(pulse);
            _pulses.Clear(); }
        public void Shutdown()
        {
            if (_model != null) _model.StateChanged -= HandleState;
            Clear();
            foreach (var projectile in GetComponentsInChildren<EnemyProjectileRuntime>()) projectile.Shutdown();
            _model = null; _run = null; _player = null; _pickups = null; _book = null; _xp = null; _schedule = null; _definitions = null;
            _deathPresentation = null;
            _contentRegistry = null;
            Schedule = Array.Empty<TravelerScheduleEntry>(); _next = _sequence = _devIndex = 0;
        }
        private void OnDestroy() => Shutdown();
    }
}
