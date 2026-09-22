using System.Collections.Generic;
using Game.Content;
using Game.Diagnostics;
using Game.Pooling;
using Game.Run;
using Game.Presentation;
using UnityEngine;

namespace Game.Enemy
{
    // Executes the WaveDirector's spawn decisions: owns the enemy/projectile pools and
    // the alive list, while the director owns timing, composition and phase state.
    [DisallowMultipleComponent]
    public sealed class ContinuousFixtureEnemySpawner : MonoBehaviour, IEnemyLifecycleSink, IRunOutcomeContributor
    {
        // Spawning instantiates/rents and initializes several enemies in one tick at
        // high phase rates; warn if that starts costing real time (DECISION-0008).
        private const float TickWarningMilliseconds = 2f;

        [SerializeField]
        private RunController runController;

        [SerializeField]
        private Transform target;

        private readonly List<EnemyRuntime> _aliveEnemies = new List<EnemyRuntime>();
        private WaveDirector _director;
        private IReadOnlyDictionary<ContentId, Sprite> _visuals;
        private IReadOnlyDictionary<ContentId, SpriteMotionProfile> _motions;
        private IReadOnlyDictionary<ContentId, SpriteContactProfile> _contacts;
        private EnemyDeathPresentationProfile _deathPresentation;
        private GameObjectPool<EnemyRuntime> _pool;
        private GameObjectPool<EnemyProjectileRuntime> _projectilePool;
        private bool _initialized;
        private IEnemyLifecycleSink _lifecycleSink;
        private RunModel _outcomeOwner;
        private int _kills;

        public string Key => "ordinary-enemy-kills";
        /// <summary>Feature-owned facts; optional observers do not participate in spawn/reward decisions.</summary>
        public event System.Action<EnemyLifeEvent> LifeEvent;
        public event System.Action<Game.Combat.CombatResult> CombatResolved;
        public event System.Action<WaveSpawnOutcome> SpawnResolved;
        public WaveSpawnOutcome LastSpawnOutcome { get; private set; }
        public EnemyLifeEvent LastLifeEvent { get; private set; }

        public int AliveCount => _aliveEnemies.Count;
        public WaveDirector Director => _director;
        public string DevelopmentObservation
        {
            get
            {
                if (_aliveEnemies.Count > 0 && _aliveEnemies[0] != null)
                {
                    var enemy = _aliveEnemies[0];
                    return $"{enemy.Definition.Id} · life {enemy.LifeId:N} · {enemy.MovementPhase} · {enemy.AttackPattern?.ToString() ?? "Melee"} · {enemy.AttackPhase?.ToString() ?? "No attack"} {enemy.AttackPhaseRemaining:0.##} s · burst left {enemy.BurstShotsRemaining} · shot {enemy.LastProjectileSource.ContentId?.ToString() ?? "none"}/{enemy.LastProjectileSource.Owner.LifeId:N} · slow sources {enemy.Controls.SlowSourceCount} · movement x{enemy.Controls.MovementMultiplier:0.##} · knockback {enemy.Controls.KnockbackRemaining:0.##} s";
                }
                if (LastLifeEvent != null)
                    return $"{LastLifeEvent.ContentId} · life {LastLifeEvent.LifeId:N} · {LastLifeEvent.Reason}";
                return _initialized ? "No live enemies" : "Enemy fixtures unavailable";
            }
        }

        private void Start()
        {
            if (_initialized)
                return;
            Debug.LogError("Enemy spawner must be initialized by the gameplay composition root.", this);
            enabled = false;
        }

        public void Initialize(WaveDirector director, IReadOnlyDictionary<ContentId, Sprite> visuals = null,
            IEnemyLifecycleSink lifecycleSink = null,
            IReadOnlyDictionary<ContentId, SpriteMotionProfile> motions = null,
            IReadOnlyDictionary<ContentId, SpriteContactProfile> contacts = null,
            EnemyDeathPresentationProfile deathPresentation = null)
        {
            if (_initialized)
                throw new System.InvalidOperationException("Enemy spawner is already initialized.");

            _director = director ?? throw new System.ArgumentNullException(nameof(director));
            _lifecycleSink = lifecycleSink;
            _kills = 0;
            LastLifeEvent = null;
            LastSpawnOutcome = default;
            _visuals = visuals;
            _motions = motions;
            _contacts = contacts;
            _deathPresentation = deathPresentation;
            _pool ??= new GameObjectPool<EnemyRuntime>(EnemyFactory.CreateInstance, transform);
            _projectilePool ??= new GameObjectPool<EnemyProjectileRuntime>(EnemyProjectileFactory.CreateInstance, transform);
            _outcomeOwner = runController != null ? runController.Model : null;
            _outcomeOwner?.RegisterOutcomeContributor(this);
            _initialized = true;
        }

        private void Update()
        {
            var hasRun = runController != null && runController.Model != null;
            var isRunning = hasRun && runController.Model.State == RunState.Running;
            Tick(hasRun ? runController.Model.Elapsed : 0f, Time.deltaTime, isRunning);
        }

        // Spawns whatever the director says is due; returns how many enemies were created.
        public int Tick(float elapsedSeconds, float deltaTime, bool isRunning)
        {
            if (!_initialized)
                return 0;

            using var guard = PerfGuard.Measure("ContinuousFixtureEnemySpawner.Tick", TickWarningMilliseconds);
            var wasRunning = runController != null && runController.Model != null && runController.Model.State == RunState.Running;
            var spawnCount = _director.Advance(elapsedSeconds, deltaTime, isRunning, _aliveEnemies.Count);
            var actual = 0;
            for (var i = 0; i < spawnCount; i++)
            {
                if (wasRunning && runController.Model.State != RunState.Running) break;
                if (SpawnEnemy()) actual++;
            }
            var decision = _director.LastDecision;
            if (decision.Requested > 0 || decision.Expired > 0)
            {
                LastSpawnOutcome = new WaveSpawnOutcome(_director.CurrentPhase.Id, _director.Elapsed,
                    _director.CurrentPhase.SpawnMode, decision, actual, AliveCount, _director.CurrentPhase.MaxAliveEnemies);
                SpawnResolved?.Invoke(LastSpawnOutcome);
            }
            return actual;
        }

        private bool SpawnEnemy()
        {
            if (target == null || runController == null)
                return false;

            var angle = _director.SelectSpawnAngle();
            var direction = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle));

            var definition = _director.SelectEnemy();
            var visual = _visuals != null && _visuals.TryGetValue(definition.Id, out var sprite) ? sprite : null;
            var motion = _motions != null && _motions.TryGetValue(definition.Id, out var profile) ? profile : null;
            var contact = _contacts != null && _contacts.TryGetValue(definition.Id, out var fitted) ? fitted : null;
            var spawnPosition = (Vector2)target.position + direction * _director.Timeline.SpawnRadius;
            var enemy = EnemyFactory.Spawn(
                definition,
                spawnPosition,
                target,
                runController,
                transform,
                visual,
                _pool,
                _projectilePool,
                this,
                motionProfile: motion,
                contact: contact,
                deathPresentation: _deathPresentation);
            enemy.Despawned += HandleEnemyDespawned;
            enemy.CombatResolved += ForwardCombat;
            _aliveEnemies.Add(enemy);
            return true;
        }

        private void ForwardCombat(Game.Combat.CombatResult result) => CombatResolved?.Invoke(result);

        private void HandleEnemyDespawned(EnemyRuntime enemy)
        {
            enemy.Despawned -= HandleEnemyDespawned;
            enemy.CombatResolved -= ForwardCombat;
            _aliveEnemies.Remove(enemy);
        }

        public void OnEnemyLifeEvent(EnemyLifeEvent snapshot)
        {
            LastLifeEvent = snapshot;
            if (snapshot.Kind == EnemyLifeEventKind.Died && snapshot.Category == EnemyCategory.Ordinary) _kills++;
            _lifecycleSink?.OnEnemyLifeEvent(snapshot);
            LifeEvent?.Invoke(snapshot);
        }

        public RunOutcomeContribution Capture() => new RunOutcomeContribution(kills: _kills);

        public void Shutdown()
        {
            if (!_initialized)
                return;

            using var guard = PerfGuard.Measure("ContinuousFixtureEnemySpawner.Shutdown", TickWarningMilliseconds);

            while (_aliveEnemies.Count > 0)
            {
                var lastIndex = _aliveEnemies.Count - 1;
                var enemy = _aliveEnemies[lastIndex];
                _aliveEnemies.RemoveAt(lastIndex);
                if (enemy == null)
                    continue;

                enemy.Despawned -= HandleEnemyDespawned;
                enemy.CombatResolved -= ForwardCombat;
                enemy.Despawn();
            }
            _director = null;
            _visuals = null;
            _motions = null;
            _contacts = null;
            _deathPresentation = null;
            _outcomeOwner?.UnregisterOutcomeContributor(this);
            _outcomeOwner = null;
            _lifecycleSink = null;
            LifeEvent = null;
            CombatResolved = null;
            SpawnResolved = null;
            LastSpawnOutcome = default;
            _initialized = false;
        }

        private void OnDestroy()
        {
            Shutdown();
        }
    }
}
