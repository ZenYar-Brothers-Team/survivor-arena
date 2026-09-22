using System;
using System.Collections.Generic;
using System.Linq;
using Game.Combat;
using Game.Diagnostics;
using Game.Pooling;
using Game.Run;
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
        private IReadOnlyDictionary<WaveHookKind, BossEncounterDefinition> _definitions;
        private WaveDirector _director;
        private RunController _run;
        private RunModel _model;
        private Transform _target;
        private IEnemyLifecycleSink _sink;
        private GameObjectPool<EnemyRuntime> _pool;
        private GameObjectPool<EnemyProjectileRuntime> _projectiles;
        public EnemyRuntime FinalBoss => _alive.TryGetValue(WaveHookKind.FinalBoss, out var enemy) && enemy.IsAlive ? enemy : null;
        public BossEncounterDefinition FinalDefinition => _definitions != null && _definitions.TryGetValue(WaveHookKind.FinalBoss, out var definition) ? definition : null;
        public event Action<EnemyLifeEvent> LifeEvent;
        public event Action<BossPhaseEvent> PhaseChanged;
        public event Action<CombatResult> CombatResolved;
        public event Action Changed;

        public string DevelopmentObservation
        {
            get
            {
                using var guard = PerfGuard.Measure("BossEncounter.Observation", 2f);
                return string.Join("\n", _alive.Select(pair =>
                    $"{pair.Key}: {pair.Value.ContentId} · life {pair.Value.LifeId:N} · phase {pair.Value.BossCombat.Phase.Id} · attack {pair.Value.BossCombat.AttackDefinition.Id} · {pair.Value.AttackPhase} {pair.Value.AttackPhaseRemaining:0.##} s"));
            }
        }

        public void Initialize(WaveDirector director, RunController run, Transform target,
            IReadOnlyList<BossEncounterDefinition> definitions, IEnemyLifecycleSink sink = null)
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
            _sink = sink;
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
            var enemy = EnemyFactory.Spawn(definition.Body,
                (Vector2)_target.position + new Vector2(definition.SpawnOffsetX, definition.SpawnOffsetY),
                _target, _run, transform, pool: _pool, projectilePool: _projectiles,
                lifecycleSink: this, category: EnemyCategory.Boss);
            // An observer may end the run during Spawned; don't leak that new life after terminal cleanup.
            if (_model == null || _model.State != RunState.Running) { enemy.Despawn(); return; }
            enemy.ConfigureBoss(definition);
            _alive.Add(hook.Kind, enemy);
            enemy.Despawned += HandleDespawn;
            enemy.CombatResolved += ForwardCombat;
            enemy.Health.HealthChanged += HandleHealthChanged;
            Action<int, int> handler = (previous, current) => PhaseChanged?.Invoke(new BossPhaseEvent(
                enemy.Identity, definition.Phases[previous].Id, definition.Phases[current].Id, enemy.BossCombat.AttackDefinition.Id));
            _phaseHandlers.Add(enemy, handler);
            enemy.BossCombat.PhaseChanged += handler;
            Changed?.Invoke();
        }

        private void ForwardCombat(CombatResult result) => CombatResolved?.Invoke(result);
        private void HandleHealthChanged(float previous, float current) => Changed?.Invoke();

        private void HandleDespawn(EnemyRuntime enemy)
        {
            enemy.Despawned -= HandleDespawn;
            enemy.CombatResolved -= ForwardCombat;
            enemy.Health.HealthChanged -= HandleHealthChanged;
            if (_phaseHandlers.TryGetValue(enemy, out var handler))
            {
                enemy.BossCombat.PhaseChanged -= handler;
                _phaseHandlers.Remove(enemy);
            }
            foreach (var pair in _alive)
                if (pair.Value == enemy) { _alive.Remove(pair.Key); break; }
            Changed?.Invoke();
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
            _sink = null;
            _definitions = null;
            _consumed.Clear();
            LifeEvent = null;
            PhaseChanged = null;
            CombatResolved = null;
            Changed = null;
        }

        private void OnDestroy() => Shutdown();
    }
}
