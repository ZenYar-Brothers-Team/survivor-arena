using System.Collections.Generic;
using Game.Content;
using Game.Pooling;
using Game.Run;
using UnityEngine;

namespace Game.Enemy
{
    // Executes the WaveDirector's spawn decisions: owns the enemy/projectile pools and
    // the alive list, while the director owns timing, composition and phase state.
    [DisallowMultipleComponent]
    public sealed class ContinuousFixtureEnemySpawner : MonoBehaviour
    {
        [SerializeField]
        private RunController runController;

        [SerializeField]
        private Transform target;

        private readonly List<EnemyRuntime> _aliveEnemies = new List<EnemyRuntime>();
        private WaveDirector _director;
        private IReadOnlyDictionary<ContentId, Sprite> _visuals;
        private GameObjectPool<EnemyRuntime> _pool;
        private GameObjectPool<EnemyProjectileRuntime> _projectilePool;
        private bool _initialized;

        public int AliveCount => _aliveEnemies.Count;
        public WaveDirector Director => _director;
        public string DevelopmentObservation
        {
            get
            {
                if (_aliveEnemies.Count > 0 && _aliveEnemies[0] != null)
                {
                    var enemy = _aliveEnemies[0];
                    return $"{enemy.Definition.Id} · {enemy.MovementPhase} · {enemy.AttackPattern?.ToString() ?? "Melee"}";
                }
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

        public void Initialize(WaveDirector director, IReadOnlyDictionary<ContentId, Sprite> visuals = null)
        {
            if (_initialized)
                throw new System.InvalidOperationException("Enemy spawner is already initialized.");

            _director = director ?? throw new System.ArgumentNullException(nameof(director));
            _visuals = visuals;
            _pool ??= new GameObjectPool<EnemyRuntime>(EnemyFactory.CreateInstance, transform);
            _projectilePool ??= new GameObjectPool<EnemyProjectileRuntime>(EnemyProjectileFactory.CreateInstance, transform);
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

            var spawnCount = _director.Advance(elapsedSeconds, deltaTime, isRunning, _aliveEnemies.Count);
            for (var i = 0; i < spawnCount; i++)
                SpawnEnemy();
            return spawnCount;
        }

        private void SpawnEnemy()
        {
            if (target == null || runController == null)
                return;

            var direction = Random.insideUnitCircle;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                direction = Vector2.right;
            direction.Normalize();

            var definition = _director.SelectEnemy();
            var visual = _visuals != null && _visuals.TryGetValue(definition.Id, out var sprite) ? sprite : null;
            var spawnPosition = (Vector2)target.position + direction * _director.Timeline.SpawnRadius;
            var enemy = EnemyFactory.Spawn(
                definition,
                spawnPosition,
                target,
                runController,
                transform,
                visual,
                _pool,
                _projectilePool);
            enemy.Despawned += HandleEnemyDespawned;
            _aliveEnemies.Add(enemy);
        }

        private void HandleEnemyDespawned(EnemyRuntime enemy)
        {
            enemy.Despawned -= HandleEnemyDespawned;
            _aliveEnemies.Remove(enemy);
        }

        public void Shutdown()
        {
            if (!_initialized)
                return;

            while (_aliveEnemies.Count > 0)
            {
                var lastIndex = _aliveEnemies.Count - 1;
                var enemy = _aliveEnemies[lastIndex];
                _aliveEnemies.RemoveAt(lastIndex);
                if (enemy == null)
                    continue;

                enemy.Despawned -= HandleEnemyDespawned;
                enemy.Despawn();
            }
            _director = null;
            _visuals = null;
            _initialized = false;
        }

        private void OnDestroy()
        {
            Shutdown();
        }
    }
}
