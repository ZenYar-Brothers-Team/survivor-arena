using System.Collections.Generic;
using Game.Content;
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

        [SerializeField]
        private string fixtureContentId = "FIXTURE-ENEMY-SEEKER";

        [SerializeField, Min(0.0001f)]
        private float fixtureMaxHealth = 10f;

        [SerializeField, Min(0.0001f)]
        private float fixtureCollisionSize = 1f;

        [SerializeField, Min(0f)]
        private float fixtureMovementSpeed = 1f;

        [SerializeField, Min(0f)]
        private float fixtureContactDamage = 1f;

        [SerializeField, Min(0.0001f)]
        private float fixtureContactDamageInterval = 1f;

        [SerializeField, Min(0.0001f)]
        private float spawnIntervalSeconds = 2f;

        [SerializeField, Min(0f)]
        private float spawnRadius = 8f;

        [SerializeField, Min(1)]
        private int maxAliveEnemies = 10;

        private readonly List<EnemyRuntime> _aliveEnemies = new List<EnemyRuntime>();
        private EnemyDefinition _fixtureDefinition;
        private ContinuousSpawnTimer _spawnTimer;

        public int AliveCount => _aliveEnemies.Count;

        private void Awake()
        {
            _fixtureDefinition = new EnemyDefinition(
                new ContentId(fixtureContentId),
                fixtureMaxHealth,
                fixtureCollisionSize,
                fixtureMovementSpeed,
                fixtureContactDamage,
                fixtureContactDamageInterval);
            _spawnTimer = new ContinuousSpawnTimer(spawnIntervalSeconds);
        }

        private void Update()
        {
            var isRunning = runController != null &&
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
            var enemy = EnemyFactory.Spawn(_fixtureDefinition, spawnPosition, target, runController, transform);
            enemy.Despawned += HandleEnemyDespawned;
            _aliveEnemies.Add(enemy);
        }

        private void HandleEnemyDespawned(EnemyRuntime enemy)
        {
            enemy.Despawned -= HandleEnemyDespawned;
            _aliveEnemies.Remove(enemy);
        }

        private void OnDestroy()
        {
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
        }
    }
}
