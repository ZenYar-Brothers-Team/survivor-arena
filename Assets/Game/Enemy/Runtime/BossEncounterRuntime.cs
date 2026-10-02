using System;
using System.Collections.Generic;
using System.Linq;
using Game.Character;
using Game.Combat;
using Game.Content;
using Game.Diagnostics;
using Game.Pooling;
using Game.Run;
using Game.Presentation;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>IP-15 owns uncapped encounter lives. WaveDirector owns hook time; RunModel alone owns victory.</summary>
    [DisallowMultipleComponent]
    public sealed class BossEncounterRuntime : MonoBehaviour, IBossEncounterRuntime, IEnemyLifecycleSink
    {
        private readonly Dictionary<WaveHookKind, EnemyRuntime> _alive = new Dictionary<WaveHookKind, EnemyRuntime>();
        private readonly HashSet<WaveHookKind> _consumed = new HashSet<WaveHookKind>();
        private readonly Dictionary<EnemyRuntime, Action<int, int>> _phaseHandlers = new Dictionary<EnemyRuntime, Action<int, int>>();
        private readonly Dictionary<EnemyRuntime, BossTeleportController> _teleports = new Dictionary<EnemyRuntime, BossTeleportController>();
        private readonly Dictionary<EnemyRuntime, BossTeleportPresentation> _teleportViews = new Dictionary<EnemyRuntime, BossTeleportPresentation>();
        private readonly List<BossTeleportPresentation> _teleportViewPool = new List<BossTeleportPresentation>();
        private readonly List<KeyValuePair<EnemyRuntime, BossTeleportController>> _teleportTicks = new List<KeyValuePair<EnemyRuntime, BossTeleportController>>();
        // DECISION-0066: per boss life zones/beams/summon markers, their views, and the ordinary enemies it summoned.
        private readonly Dictionary<EnemyRuntime, BossHazardField> _hazards = new Dictionary<EnemyRuntime, BossHazardField>();
        private readonly Dictionary<EnemyRuntime, BossHazardPresentation> _hazardViews = new Dictionary<EnemyRuntime, BossHazardPresentation>();
        private readonly List<BossHazardPresentation> _hazardViewPool = new List<BossHazardPresentation>();
        private readonly List<KeyValuePair<EnemyRuntime, BossHazardField>> _hazardTicks = new List<KeyValuePair<EnemyRuntime, BossHazardField>>();
        private readonly Dictionary<EnemyRuntime, EnemyRuntime> _summonOwners = new Dictionary<EnemyRuntime, EnemyRuntime>();
        private readonly Dictionary<EnemyRuntime, BossSummonProfile> _summonProfiles = new Dictionary<EnemyRuntime, BossSummonProfile>();
        /// <summary>Fallback view radius when no camera is available (tests); covers the 200×200 arena reference screen.</summary>
        private const float FallbackViewRadius = 12f;
        private IReadOnlyDictionary<WaveHookKind, BossEncounterDefinition> _definitions;
        private WaveDirector _director;
        private RunController _run;
        private RunModel _model;
        private Transform _target;
        private PlayerCharacterRuntime _player;
        private IEnemyLifecycleSink _sink;
        private IEnemyLifecycleSink _summonSink;
        private GameObjectPool<EnemyRuntime> _pool;
        private GameObjectPool<EnemyProjectileRuntime> _projectiles;
        private EnemyDeathPresentationProfile _deathPresentation;
        private GroundShadowPresentationProfile _groundShadowPresentation;
        private ContentRegistry _contentRegistry;
        public EnemyRuntime FinalBoss => _alive.TryGetValue(WaveHookKind.FinalBoss, out var enemy) && enemy.IsAlive ? enemy : null;
        public BossEncounterDefinition FinalDefinition => _definitions != null && _definitions.TryGetValue(WaveHookKind.FinalBoss, out var definition) ? definition : null;
        public EnemyRuntime MidBoss => _alive.TryGetValue(WaveHookKind.MidBoss, out var enemy) && enemy.IsAlive ? enemy : null;
        public BossEncounterDefinition MidDefinition => _definitions != null && _definitions.TryGetValue(WaveHookKind.MidBoss, out var definition) ? definition : null;
        public event Action<EnemyLifeEvent> LifeEvent;
        public event Action<BossPhaseEvent> PhaseChanged;
        public event Action<CombatResult> CombatResolved;
        /// <summary>Forwarded from every living boss, plus teleport-slam moments (audio cues).</summary>
        public event Action<EnemyActionKind, Vector2> ActionStarted;
        public event Action Changed;
        /// <summary>Living ordinary enemies summoned by bosses (DECISION-0066, F3); they outlive their boss.</summary>
        public int SummonedAlive => _summonOwners.Count;
        public BossHazardField HazardsOf(EnemyRuntime boss) => boss != null && _hazards.TryGetValue(boss, out var field) ? field : null;
        /// <summary>Copies currently visible boss hazard shapes for diagnostics and autonomous observation.</summary>
        public void CopyHazardVisualsTo(List<BossHazardVisual> destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            using var guard = Game.Diagnostics.PerfGuard.Measure("BossEncounterRuntime.CopyHazardVisualsTo", 2f);
            destination.Clear();
            foreach (var field in _hazards.Values) destination.AddRange(field.Visuals);
        }

        public string DevelopmentObservation
        {
            get
            {
                using var guard = PerfGuard.Measure("BossEncounter.Observation", 2f);
                return string.Join("\n", _alive.Select(pair =>
                    $"{pair.Key}: {pair.Value.ContentId} · life {pair.Value.LifeId:N} · phase {pair.Value.BossCombat.Phase.Id} · attack {pair.Value.BossCombat.AttackDefinition?.Id.ToString() ?? "none"} · {pair.Value.AttackPhase} {pair.Value.AttackPhaseRemaining:0.##} s" +
                    (_teleports.TryGetValue(pair.Value, out var teleport) ? $" · teleport {teleport.Phase} far {teleport.FarElapsed:0.0} s" : "") +
                    (_hazards.TryGetValue(pair.Value, out var hazards) ? $" · zones {hazards.ActiveZones} beams {hazards.ActiveBeams}" : "") +
                    $" · summoned {_summonOwners.Count}"));
            }
        }

        public void Initialize(WaveDirector director, RunController run, Transform target,
            IReadOnlyList<BossEncounterDefinition> definitions, IEnemyLifecycleSink sink = null,
            EnemyDeathPresentationProfile deathPresentation = null,
            GroundShadowPresentationProfile groundShadowPresentation = null,
            ContentRegistry contentRegistry = null, IEnemyLifecycleSink summonSink = null)
        {
            if (_director != null) throw new InvalidOperationException("Boss owner is already initialized.");
            if (director == null || run == null || run.Model == null || target == null || definitions == null)
                throw new ArgumentException("Boss owner requires director, run, target and definitions.");
            var byHook = definitions.ToDictionary(d => d.Hook);
            if (!byHook.ContainsKey(WaveHookKind.FinalBoss) || !director.Timeline.Hooks.Any(h => h.Kind == WaveHookKind.FinalBoss))
                throw new ArgumentException("Final boss definition and hook are required.");
            foreach (var hook in director.Timeline.Hooks)
                if (!byHook.ContainsKey(hook.Kind)) throw new ArgumentException($"Missing encounter for {hook.Kind}.");
            _definitions = byHook;
            _run = run;
            _model = run.Model;
            _target = target;
            _player = target.GetComponent<PlayerCharacterRuntime>();
            _sink = sink;
            // Summons are ordinary enemies: the ordinary reward sink (XP and pickups) when the composition provides it.
            _summonSink = summonSink ?? sink;
            _deathPresentation = deathPresentation;
            _groundShadowPresentation = groundShadowPresentation;
            _contentRegistry = contentRegistry;
            _pool ??= new GameObjectPool<EnemyRuntime>(EnemyFactory.CreateInstance, transform);
            _projectiles ??= new GameObjectPool<EnemyProjectileRuntime>(EnemyProjectileFactory.CreateInstance, transform);
            _consumed.Clear();
            _director = director;
            _director.HookTriggered += HandleHook;
            _model.StateChanged += HandleState;
        }

        private void HandleHook(WaveHookDefinition hook)
        {
            if (_model.State != RunState.Running || !_consumed.Add(hook.Kind)) return;
            using var guard = PerfGuard.Measure("BossEncounter.Spawn", 2f);
            var definition = _definitions[hook.Kind];
            var body = EnemyBodyVisual.Resolve(definition.Body, _contentRegistry);
            var enemy = EnemyFactory.Spawn(definition.Body,
                (Vector2)_target.position + new Vector2(definition.SpawnOffsetX, definition.SpawnOffsetY),
                _target, _run, transform, visual: body.Sprite, pool: _pool, projectilePool: _projectiles,
                lifecycleSink: this, category: EnemyCategory.Boss, motionProfile: body.Motion, contact: body.Contact,
                deathPresentation: _deathPresentation, groundShadowPresentation: _groundShadowPresentation,
                contentRegistry: _contentRegistry);
            // An observer may end the run during Spawned; don't leak that new life after terminal cleanup.
            if (_model == null || _model.State != RunState.Running) { enemy.Despawn(); return; }
            enemy.ConfigureBoss(definition);
            if (definition.Teleport != null) _teleports.Add(enemy, new BossTeleportController(definition.Teleport, new System.Random(enemy.LifeId.GetHashCode())));
            _hazards.Add(enemy, new BossHazardField(new System.Random(enemy.LifeId.GetHashCode() ^ 0x5bd1e995)));
            enemy.SpecialRequested += HandleSpecial;
            enemy.Died += HandleBossDied;
            _alive.Add(hook.Kind, enemy);
            enemy.Despawned += HandleDespawn;
            enemy.CombatResolved += ForwardCombat;
            enemy.ActionStarted += ForwardAction;
            enemy.Health.HealthChanged += HandleHealthChanged;
            Action<int, int> handler = (previous, current) => PhaseChanged?.Invoke(new BossPhaseEvent(
                enemy.Identity, definition.Phases[previous].Id, definition.Phases[current].Id, enemy.BossCombat.AttackDefinition?.Id ?? default));
            _phaseHandlers.Add(enemy, handler);
            enemy.BossCombat.PhaseChanged += handler;
            Changed?.Invoke();
        }

        private void ForwardCombat(CombatResult result) => CombatResolved?.Invoke(result);
        private void ForwardAction(EnemyActionKind kind, Vector2 position) => ActionStarted?.Invoke(kind, position);
        private void HandleHealthChanged(float previous, float current) => Changed?.Invoke();

        private void HandleSpecial(EnemyRuntime boss, BossSpecialRequest request)
        {
            if (!_hazards.TryGetValue(boss, out var field) || _target == null) return;
            var alive = 0;
            if (request.Kind == BossSpecialKind.Summon)
                foreach (var pair in _summonOwners)
                    if (pair.Value == boss && _summonProfiles[pair.Key] == request.Summon) alive++;
            field.Start(request, boss.Position, _target.position, alive);
        }

        // Death removes the boss's pending zones, beams and markers at once (IP-21 bosses-v1 acceptance).
        private void HandleBossDied(EnemyRuntime boss)
        {
            if (_hazards.TryGetValue(boss, out var field)) field.Clear();
            if (_hazardViews.TryGetValue(boss, out var view)) view.ResetPresentation();
        }

        private void HandleDespawn(EnemyRuntime enemy)
        {
            enemy.Despawned -= HandleDespawn;
            enemy.SpecialRequested -= HandleSpecial;
            enemy.Died -= HandleBossDied;
            _hazards.Remove(enemy);
            ReleaseHazardView(enemy);
            enemy.CombatResolved -= ForwardCombat;
            enemy.ActionStarted -= ForwardAction;
            enemy.Health.HealthChanged -= HandleHealthChanged;
            if (_phaseHandlers.TryGetValue(enemy, out var handler))
            {
                enemy.BossCombat.PhaseChanged -= handler;
                _phaseHandlers.Remove(enemy);
            }
            _teleports.Remove(enemy);
            ReleaseTeleportView(enemy);
            foreach (var pair in _alive)
                if (pair.Value == enemy) { _alive.Remove(pair.Key); break; }
            Changed?.Invoke();
        }

        private void FixedUpdate()
        {
            if (_model == null) return;
            TickHazards();
            if (_model == null || _teleports.Count == 0) return;
            using var guard = PerfGuard.Measure("BossEncounter.Teleport", 1f);
            var running = _model.State == RunState.Running;
            // A slam can end the run (player death) and clear the lives iterated here.
            _teleportTicks.Clear();
            _teleportTicks.AddRange(_teleports);
            foreach (var pair in _teleportTicks)
            {
                var enemy = pair.Key;
                if (_model == null || enemy == null || !_teleports.ContainsKey(enemy)) continue;
                var controller = pair.Value;
                if (!enemy.IsAlive)
                {
                    // A dying boss never lands: drop its pending marker instead of freezing it on the ground.
                    if (controller.Phase == BossTeleportPhase.Telegraphing && _teleportViews.TryGetValue(enemy, out var dyingView))
                        dyingView.ResetPresentation();
                    continue;
                }
                var signal = controller.Tick(Time.fixedDeltaTime, running, enemy.Position, _target.position);
                if (signal == BossTeleportSignal.TelegraphStarted)
                {
                    ActionStarted?.Invoke(EnemyActionKind.TeleportWindup, enemy.Position);
                    TeleportView(enemy).ShowTelegraph(controller.Profile, controller.Landing);
                }
                else if (signal == BossTeleportSignal.Impact)
                {
                    ActionStarted?.Invoke(EnemyActionKind.TeleportSlam, controller.Landing);
                    Slam(enemy, controller);
                }
            }
        }

        private void Update()
        {
            if (_model == null) return;
            if (_hazards.Count > 0) RenderHazards();
            if (_teleportViews.Count == 0) return;
            var running = _model.State == RunState.Running;
            foreach (var view in _teleportViews.Values) view.Tick(Time.deltaTime, running);
        }

        private void TickHazards()
        {
            if (_hazards.Count == 0) return;
            using var guard = PerfGuard.Measure("BossEncounter.Hazards", 1f);
            var running = _model.State == RunState.Running;
            // A hit can end the run (player death) and clear the lives iterated here.
            _hazardTicks.Clear();
            _hazardTicks.AddRange(_hazards);
            foreach (var pair in _hazardTicks)
            {
                var boss = pair.Key;
                if (_model == null || boss == null || !_hazards.ContainsKey(boss) || !boss.IsAlive) continue;
                var field = pair.Value;
                field.Tick(Time.fixedDeltaTime, running, boss.Position, _target.position);
                foreach (var hit in field.Hits)
                {
                    if (_model == null || _model.State != RunState.Running) break;
                    if (_player == null || _player.Health == null || _player.Health.IsDead) break;
                    _player.ApplyDamage(new CombatDamageRequest(
                        new CombatSource(boss.Identity, hit.SourceId, CombatSourceOrigin.EnemyProjectile),
                        hit.Damage, hit.Controls, hit.Direction.x, hit.Direction.y));
                }
                foreach (var spawn in field.Spawns)
                {
                    if (_model == null || _model.State != RunState.Running) break;
                    Summon(boss, spawn);
                }
            }
        }

        private void Summon(EnemyRuntime boss, BossSummonSpawn spawn)
        {
            using var guard = PerfGuard.Measure("BossEncounter.Summon", 2f);
            var definition = spawn.Profile.Enemy;
            var body = EnemyBodyVisual.Resolve(definition, _contentRegistry);
            // Summons are ordinary enemies outside the regular cap: XP through the same sink, cleaned with the encounter.
            var summoned = EnemyFactory.Spawn(definition, spawn.Position, _target, _run, transform, visual: body.Sprite, pool: _pool,
                projectilePool: _projectiles, lifecycleSink: _summonSink, category: EnemyCategory.Ordinary, motionProfile: body.Motion,
                contact: body.Contact, deathPresentation: _deathPresentation, groundShadowPresentation: _groundShadowPresentation,
                contentRegistry: _contentRegistry);
            if (_model == null || _model.State != RunState.Running) { summoned.Despawn(); return; }
            _summonOwners.Add(summoned, boss);
            _summonProfiles.Add(summoned, spawn.Profile);
            summoned.Despawned += HandleSummonDespawn;
            summoned.Died += HandleSummonDied;
        }

        private void HandleSummonDied(EnemyRuntime summoned) => ForgetSummon(summoned);
        private void HandleSummonDespawn(EnemyRuntime summoned) => ForgetSummon(summoned);

        private void ForgetSummon(EnemyRuntime summoned)
        {
            summoned.Despawned -= HandleSummonDespawn;
            summoned.Died -= HandleSummonDied;
            _summonOwners.Remove(summoned);
            _summonProfiles.Remove(summoned);
        }

        private void RenderHazards()
        {
            var camera = Camera.main;
            var center = camera != null ? (Vector2)camera.transform.position : (Vector2)_target.position;
            var radius = camera != null && camera.orthographic
                ? new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize).magnitude + 1f
                : FallbackViewRadius;
            foreach (var pair in _hazards)
            {
                if (pair.Value.Visuals.Count == 0 && !_hazardViews.ContainsKey(pair.Key)) continue;
                HazardView(pair.Key).Render(pair.Value.Visuals, center, radius);
            }
        }

        private BossHazardPresentation HazardView(EnemyRuntime enemy)
        {
            if (_hazardViews.TryGetValue(enemy, out var view)) return view;
            if (_hazardViewPool.Count > 0)
            {
                view = _hazardViewPool[_hazardViewPool.Count - 1];
                _hazardViewPool.RemoveAt(_hazardViewPool.Count - 1);
            }
            else
            {
                var viewObject = new GameObject("BossHazardEffect");
                viewObject.transform.SetParent(transform, false);
                view = viewObject.AddComponent<BossHazardPresentation>();
            }
            _hazardViews.Add(enemy, view);
            return view;
        }

        private void ReleaseHazardView(EnemyRuntime enemy)
        {
            if (!_hazardViews.TryGetValue(enemy, out var view)) return;
            _hazardViews.Remove(enemy);
            if (view == null) return;
            view.ResetPresentation();
            _hazardViewPool.Add(view);
        }

        /// <summary>BOSS-001 teleport-slam (DECISION-0059): land next to the player and hit them if they are still in the marked area.</summary>
        private void Slam(EnemyRuntime enemy, BossTeleportController controller)
        {
            var profile = controller.Profile;
            var landing = controller.Landing;
            var body = enemy.GetComponent<Rigidbody2D>();
            body.position = landing;
            body.linearVelocity = Vector2.zero;
            enemy.transform.position = landing;
            TeleportView(enemy).PlayImpact(profile, landing);
            if (_player == null || _player.Health == null || _player.Health.IsDead || profile.ImpactDamage <= 0f) return;
            var playerPosition = (Vector2)_target.position;
            if (!controller.Hits(playerPosition)) return;
            var direction = playerPosition - landing;
            _player.ApplyDamage(new CombatDamageRequest(
                new CombatSource(enemy.Identity, enemy.ContentId, CombatSourceOrigin.EnemyContact),
                profile.ImpactDamage, profile.ImpactControls, direction.x, direction.y));
        }

        private BossTeleportPresentation TeleportView(EnemyRuntime enemy)
        {
            if (_teleportViews.TryGetValue(enemy, out var view)) return view;
            if (_teleportViewPool.Count > 0)
            {
                view = _teleportViewPool[_teleportViewPool.Count - 1];
                _teleportViewPool.RemoveAt(_teleportViewPool.Count - 1);
            }
            else
            {
                var viewObject = new GameObject("BossTeleportEffect");
                viewObject.transform.SetParent(transform, false);
                view = viewObject.AddComponent<BossTeleportPresentation>();
            }
            _teleportViews.Add(enemy, view);
            return view;
        }

        private void ReleaseTeleportView(EnemyRuntime enemy)
        {
            if (!_teleportViews.TryGetValue(enemy, out var view)) return;
            _teleportViews.Remove(enemy);
            if (view == null) return;
            view.ResetPresentation();
            _teleportViewPool.Add(view);
        }

        public void OnEnemyLifeEvent(EnemyLifeEvent snapshot)
        {
            _sink?.OnEnemyLifeEvent(snapshot);
            LifeEvent?.Invoke(snapshot);
        }

        private void HandleState(RunState state)
        {
            if (_model == null || _model.State != state) return;
            if (state == RunState.Running || state == RunState.Paused)
            {
                using var guard = PerfGuard.Measure("BossEncounter.Pause", 2f);
                foreach (var enemy in _alive.Values)
                {
                    var body = enemy.GetComponent<Rigidbody2D>();
                    body.linearVelocity = Vector2.zero;
                    body.simulated = state == RunState.Running;
                }
                return;
            }
            ClearLives();
        }

        private void ClearLives()
        {
            using var guard = PerfGuard.Measure("BossEncounter.Cleanup", 2f);
            foreach (var enemy in _alive.Values.ToArray())
            {
                HandleDespawn(enemy);
                if (enemy != null) enemy.Despawn();
            }
            foreach (var summoned in _summonOwners.Keys.ToArray())
            {
                ForgetSummon(summoned);
                if (summoned != null) summoned.Despawn();
            }
        }

        public void Shutdown()
        {
            if (_director == null) return;
            using var guard = PerfGuard.Measure("BossEncounter.Shutdown", 2f);
            _director.HookTriggered -= HandleHook;
            _model.StateChanged -= HandleState;
            ClearLives();
            // Projectiles normally return via terminal state; explicit shutdown also clears a still-running encounter.
            foreach (var projectile in GetComponentsInChildren<EnemyProjectileRuntime>()) projectile.Shutdown();
            _director = null;
            _model = null;
            _run = null;
            _target = null;
            _player = null;
            _sink = null;
            _summonSink = null;
            _definitions = null;
            _deathPresentation = null;
            _contentRegistry = null;
            _consumed.Clear();
            LifeEvent = null;
            PhaseChanged = null;
            CombatResolved = null;
            ActionStarted = null;
            Changed = null;
        }

        private void OnDestroy() => Shutdown();
    }
}
