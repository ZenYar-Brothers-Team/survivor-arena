using System.Collections.Generic;
using Game.Content;
using Game.Pooling;
using Game.Run;
using UnityEngine;

namespace Game.Enemy
{
    [DisallowMultipleComponent]
    public sealed class ContinuousFixtureEnemySpawner : MonoBehaviour
    {
        [SerializeField]
        private RunController runController;

        [SerializeField]
        private Transform target;

        [SerializeField, Min(0.0001f)]
        private float spawnIntervalSeconds = 2f;

        [SerializeField, Min(0f)]
        private float spawnRadius = 8f;

        [SerializeField, Min(1)]
        private int maxAliveEnemies = 10;

        private readonly List<EnemyRuntime> _aliveEnemies = new List<EnemyRuntime>();
        private IReadOnlyList<EnemyDefinition> _fixtureDefinitions;
        private IReadOnlyList<Sprite> _fixtureVisuals;
        private ContinuousSpawnTimer _spawnTimer;
        private GameObjectPool<EnemyRuntime> _pool;
        private GameObjectPool<EnemyProjectileRuntime> _projectilePool;
        private int _nextDefinitionIndex;
        private bool _initialized;

        public int AliveCount => _aliveEnemies.Count;
        public string DevelopmentObservation
        {
            get
            {
                if (_aliveEnemies.Count > 0 && _aliveEnemies[0] != null)
                {
                    var enemy = _aliveEnemies[0];
                    return $"{enemy.Definition.Id} · {enemy.MovementPhase} · {enemy.AttackPattern?.ToString() ?? "Melee"}";
                }
                if (_fixtureDefinitions != null && _fixtureDefinitions.Count > 0)
                {
                    var definition = _fixtureDefinitions[0];
                    return $"Next: {definition.Id} · {definition.Movement.Kind} · {definition.Attack?.Pattern.ToString() ?? "Melee"}";
                }
                return "Enemy fixtures unavailable";
            }
        }

        private void Awake()
        {
            _spawnTimer = new ContinuousSpawnTimer(spawnIntervalSeconds);
        }

        private void Start()
        {
            if (_initialized)
                return;
            Debug.LogError("Enemy spawner must be initialized by the gameplay composition root.", this);
            enabled = false;
        }

        public void Initialize(EnemyDefinition definition, Sprite visual = null)
        {
            Initialize(new[] { definition }, new[] { visual });
        }

        public void Initialize(IReadOnlyList<EnemyDefinition> definitions, IReadOnlyList<Sprite> visuals = null)
        {
            if (_initialized)
                throw new System.InvalidOperationException("Enemy spawner is already initialized.");
            if (definitions == null)
                throw new System.ArgumentNullException(nameof(definitions));
            if (definitions.Count == 0)
                throw new System.ArgumentException("At least one enemy definition is required.", nameof(definitions));
            for (var i = 0; i < definitions.Count; i++)
            {
                if (definitions[i] == null)
                    throw new System.ArgumentException("Enemy definitions cannot contain null entries.", nameof(definitions));
            }
            if (visuals != null && visuals.Count != definitions.Count)
                throw new System.ArgumentException("Visual count must match enemy definition count.", nameof(visuals));

            _fixtureDefinitions = definitions;
            _fixtureVisuals = visuals;
            _pool ??= new GameObjectPool<EnemyRuntime>(EnemyFactory.CreateInstance, transform);
            _projectilePool ??= new GameObjectPool<EnemyProjectileRuntime>(EnemyProjectileFactory.CreateInstance, transform);
            _nextDefinitionIndex = 0;
            _initialized = true;
        }

        private void Update()
        {
            var isRunning = _initialized && runController != null &&
                            runController.Model != null &&
                            runController.Model.State == RunState.Running;
            var spawnCount = _spawnTimer.Tick(Time.deltaTime, isRunning);
            var availableCapacity = maxAliveEnemies - _aliveEnemies.Count;
            spawnCount = Mathf.Min(spawnCount, Mathf.Max(0, availableCapacity));

            for (var i = 0; i < spawnCount; i++)
                SpawnFixtureEnemy();
        }

        private void SpawnFixtureEnemy()
        {
            if (target == null || runController == null)
                return;

            var direction = Random.insideUnitCircle;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                direction = Vector2.right;
            direction.Normalize();

            var spawnPosition = (Vector2)target.position + direction * spawnRadius;
            var definitionIndex = _nextDefinitionIndex;
            _nextDefinitionIndex = (_nextDefinitionIndex + 1) % _fixtureDefinitions.Count;
            var visual = _fixtureVisuals == null ? null : _fixtureVisuals[definitionIndex];
            var enemy = EnemyFactory.Spawn(
                _fixtureDefinitions[definitionIndex],
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
            _fixtureDefinitions = null;
            _fixtureVisuals = null;
            _nextDefinitionIndex = 0;
            _initialized = false;
        }

        private void OnDestroy()
        {
            Shutdown();
        }
    }
}
