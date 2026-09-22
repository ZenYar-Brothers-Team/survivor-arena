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
        private int _next, _sequence, _devIndex;
        public bool IsInitialized => _model != null;
        public IReadOnlyList<TravelerScheduleEntry> Schedule { get; private set; } = Array.Empty<TravelerScheduleEntry>();
        public IReadOnlyList<TravelerSnapshot> Snapshot => _lives.Where(life => life.Actor != null && life.Actor.IsAlive).Select(life => new TravelerSnapshot(life, _model.RunId)).ToList().AsReadOnly();
        public string DevelopmentObservation => string.Join("\n", _lives.Select(life => $"{life.Definition.Id} · {life.Definition.Role} · {life.Actor.MovementPhase}/{life.Actor.AttackPhase} · spawn {life.SpawnTime:0.0}s / until {life.Deadline:0.0}s · ×{life.Scale:0.00}"));
        public event Action<TravelerEvent> LifeEvent;
        public event Action<Game.Combat.CombatResult> CombatResolved;
        public void Initialize(TravelerScheduleDefinition schedule, FixtureTravelerCatalog catalog, RunController run,
            Transform player, Camera camera, TravelerPlacement placement, WorldPickupRuntime pickups, PickupDefinition book,
            IEnemyLifecycleSink xp, Game.Presentation.EnemyDeathPresentationProfile deathPresentation = null)
        {
            if (schedule == null || catalog == null || run?.Model == null || player == null || camera == null || !camera.orthographic || placement == null || pickups == null || book?.Kind != PickupRewardKind.Book)
                throw new ArgumentException("Traveler dependencies required.");
            foreach (var id in schedule.TravelerIds) if (!catalog.Definitions.ContainsKey(id)) throw new ArgumentException("Missing Traveler definition.");
            var planned = schedule.Draw(run.Model.Duration, new System.Random(schedule.Seed));
            Shutdown();
            _schedule = schedule; _definitions = catalog.Definitions; _run = run; _model = run.Model;
            _player = player; _camera = camera; _placement = placement; _pickups = pickups; _book = book; _xp = xp;
            _deathPresentation = deathPresentation;
            Schedule = planned; _random = new System.Random(unchecked(schedule.Seed ^ 0x54726176));
            _pool ??= new GameObjectPool<EnemyRuntime>(EnemyFactory.CreateInstance, transform);
            _projectiles ??= new GameObjectPool<EnemyProjectileRuntime>(EnemyProjectileFactory.CreateInstance, transform);
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
                var projected = _placement.Project(life.Actor.Position);
                if ((projected - life.Actor.Position).sqrMagnitude > .000001f)
                    life.Actor.GetComponent<Rigidbody2D>().position = projected;
            }
            while (_next < Schedule.Count && _model.State == RunState.Running && Schedule[_next].Time <= _model.Elapsed)
            {
                var entry = Schedule[_next++];
                // A large time step must not resurrect an encounter whose entire window has already elapsed.
                if (_model.Elapsed < entry.Time + _definitions[entry.Id].PresenceSeconds)
                    Spawn(entry.Id, entry.Time, entry.Scale);
            }
            UpdateSupport();
        }
        public EnemyRuntime Spawn(ContentId id, float spawnTime, float scale)
        {
            if (_model == null || _model.State != RunState.Running) return null;
            var definition = _definitions[id];
            if (!_placement.TrySpawn(_player.position, _camera.orthographicSize * 2 * _schedule.SpawnScreenHeights,
                _schedule.PlacementAttempts, _random, out var position)) throw new InvalidOperationException("Traveler spawn circle has no sampled reachable point; field geometry/config invalid.");
            var actor = EnemyFactory.Spawn(definition.Scale(scale), position, _player, _run, transform,
                pool: _pool, projectilePool: _projectiles, category: EnemyCategory.Traveler,
                deathPresentation: _deathPresentation);
            var life = new TravelerLife(actor, definition, spawnTime, scale, _sequence++);
            actor.GetComponent<SpriteRenderer>().color = definition.Color;
            _lives.Add(life);
            actor.ConfigureEncounter(definition.Role == TravelerRole.Offensive ? null :
                new TravelerMovementDriver(definition, _placement, _model.RunId, _random.Next()), () => DamageAllowed(life));
            actor.LifeEvent += OnActorEvent;
            actor.Despawned += OnDespawn;
            actor.CombatResolved += ForwardCombat;
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
            RemoveSupport(life.Actor.LifeId);
            if (_model.State != RunState.Running || _model.Elapsed >= life.Deadline) return;
            _xp?.OnEnemyLifeEvent(snapshot);
            _pickups.Spawn(_book, snapshot.Position, snapshot.LifeId, snapshot.ContentId);
            LifeEvent?.Invoke(new TravelerEvent(new TravelerSnapshot(life, _model.RunId), "Killed"));
        }
        private void OnDespawn(EnemyRuntime actor)
        {
            var life = _lives.FirstOrDefault(item => item.Actor == actor);
            if (life == null) return;
            _lives.Remove(life); RemoveSupport(actor.LifeId);
            actor.LifeEvent -= OnActorEvent; actor.Despawned -= OnDespawn; actor.CombatResolved -= ForwardCombat;
            if (actor.LastLifeEvent.Reason != EnemyLifeReason.Killed)
                LifeEvent?.Invoke(new TravelerEvent(new TravelerSnapshot(life, _model.RunId), actor.LastLifeEvent.Reason == EnemyLifeReason.Escaped ? "Escaped" : "Cancelled"));
        }
        private void UpdateSupport()
        {
            if (_model == null) return;
            EnemyRegistry.CopyAliveTo(_targets);
            _targets.RemoveAll(enemy => enemy.Category != EnemyCategory.Ordinary || enemy.Identity.RunId != _model.RunId);
            foreach (var life in _lives.ToArray())
            {
                var d = life.Definition;
                if (d.Support == TravelerSupportKind.None) continue;
                var source = life.Actor.LifeId;
                foreach (var previous in _affected.ToArray())
                    if (previous == null || !previous.IsAlive) _affected.Remove(previous);
                    else previous.Protection.RemoveAura(source);
                var nearby = _targets.Where(enemy => (enemy.Position - life.Actor.Position).sqrMagnitude <= d.SupportRadius * d.SupportRadius)
                    .OrderBy(enemy => (enemy.Position - life.Actor.Position).sqrMagnitude).ThenBy(enemy => enemy.LifeId).ToList();
                if (d.Support == TravelerSupportKind.Aura)
                    foreach (var enemy in nearby) { enemy.Protection.SetAura(source, d.Reduction, d.Resistance, life.Deadline); _affected.Add(enemy); }
                else if (_model.Elapsed >= life.NextSupportTime)
                {
                    life.NextSupportTime = _model.Elapsed + d.SupportCooldown;
                    foreach (var enemy in nearby.Take(d.SupportTargets))
                    { enemy.Protection.CastShield(source, d.ShieldHp, Mathf.Min(d.ShieldSeconds, life.Deadline - _model.Elapsed), _model.Elapsed); _affected.Add(enemy); }
                }
            }
            foreach (var enemy in _affected) if (enemy != null && enemy.IsAlive) enemy.Protection.Tick(_model.Elapsed);
        }
        private void RemoveSupport(Guid source)
        { foreach (var enemy in _affected) if (enemy != null) enemy.Protection.RemoveSource(source); }
        private void ForwardCombat(Game.Combat.CombatResult result) => CombatResolved?.Invoke(result);
        public void SpawnDevelopmentTraveler()
        {
            if ((!Application.isEditor && !Debug.isDebugBuild) || _model == null || _lives.Count >= 3) return;
            var id = _schedule.TravelerIds[_devIndex++ % _schedule.TravelerIds.Count];
            Spawn(id, _model.Elapsed, _schedule.Scale(_model.Elapsed, _model.Duration));
        }
        private void HandleState(RunState state)
        {
            if (state == RunState.Running || state == RunState.Paused)
            { foreach (var life in _lives) { var body = life.Actor.GetComponent<Rigidbody2D>(); body.linearVelocity = Vector2.zero; body.simulated = state == RunState.Running; } }
            else Clear();
        }
        private void Clear()
        { foreach (var life in _lives.ToArray()) if (life.Actor != null) life.Actor.Despawn(); _lives.Clear(); _affected.Clear(); }
        public void Shutdown()
        {
            if (_model != null) _model.StateChanged -= HandleState;
            Clear();
            foreach (var projectile in GetComponentsInChildren<EnemyProjectileRuntime>()) projectile.Shutdown();
            _model = null; _run = null; _player = null; _pickups = null; _book = null; _xp = null; _schedule = null; _definitions = null;
            _deathPresentation = null;
            Schedule = Array.Empty<TravelerScheduleEntry>(); _next = _sequence = _devIndex = 0;
        }
        private void OnDestroy() => Shutdown();
    }
}
