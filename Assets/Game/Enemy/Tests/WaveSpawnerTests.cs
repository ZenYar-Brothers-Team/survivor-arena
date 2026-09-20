using System.Collections.Generic;
using System.Linq;
using Game.Run;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Enemy.Tests
{
    public class WaveSpawnerTests
    {
        private GameObject _runObject;
        private GameObject _targetObject;
        private GameObject _spawnerObject;
        private ContinuousFixtureEnemySpawner _spawner;
        private int _registryBaseline;

        [SetUp]
        public void SetUp()
        {
            _registryBaseline = EnemyRegistry.Count;
            _runObject = new GameObject("RunController");
            var runController = _runObject.AddComponent<RunController>();
            _targetObject = new GameObject("Target");
            _spawnerObject = new GameObject("Spawner");
            _spawner = _spawnerObject.AddComponent<ContinuousFixtureEnemySpawner>();

            var serialized = new SerializedObject(_spawner);
            serialized.FindProperty("runController").objectReferenceValue = runController;
            serialized.FindProperty("target").objectReferenceValue = _targetObject.transform;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            if (_spawnerObject != null)
                Object.DestroyImmediate(_spawnerObject);
            if (_targetObject != null)
                Object.DestroyImmediate(_targetObject);
            if (_runObject != null)
                Object.DestroyImmediate(_runObject);
        }

        private WaveDirector CreateDirector(int maxAlive = 3)
        {
            var timeline = new WaveTimelineDefinition(
                "FIXTURE-WAVE-SPAWNER",
                3,
                WaveTestData.SpawnRadius,
                new[]
                {
                    WaveTestData.Phase("FIXTURE-P", WavePhaseTag.Ordinary, 60f, 1f, maxAlive, null,
                        WaveTestData.Entry("FIXTURE-ENEMY-A"))
                });
            return new WaveDirector(timeline, WaveTestData.TestEnemies(), 60f);
        }

        private List<EnemyRuntime> LiveEnemies() =>
            _spawnerObject.GetComponentsInChildren<EnemyRuntime>(false).ToList();

        [Test]
        public void Tick_SpawnsUpToPhaseCapAndRegistersEnemies()
        {
            _spawner.Initialize(CreateDirector());

            var spawned = 0;
            for (var second = 1; second <= 6; second++)
                spawned += _spawner.Tick(second, 1f, true);

            Assert.AreEqual(3, spawned);
            Assert.AreEqual(3, _spawner.AliveCount);
            Assert.AreEqual(_registryBaseline + 3, EnemyRegistry.Count);
        }

        [Test]
        public void Tick_WhenNotRunningOrNotInitializedSpawnsNothing()
        {
            Assert.AreEqual(0, _spawner.Tick(1f, 5f, true));

            _spawner.Initialize(CreateDirector());
            Assert.AreEqual(0, _spawner.Tick(1f, 5f, false));
            Assert.AreEqual(0, _spawner.AliveCount);
        }

        [Test]
        public void RepeatedSpawnAndDespawn_ReusesPooledEnemiesAndLeavesNoStaleTargets()
        {
            _spawner.Initialize(CreateDirector());
            var everSeen = new HashSet<EnemyRuntime>();

            for (var cycle = 0; cycle < 6; cycle++)
            {
                for (var tick = 0; tick < 3; tick++)
                    _spawner.Tick(cycle * 10 + tick, 1f, true);

                var live = LiveEnemies();
                Assert.AreEqual(3, live.Count, $"Cycle {cycle} should refill to the cap.");
                Assert.AreEqual(_registryBaseline + 3, EnemyRegistry.Count);
                foreach (var enemy in live)
                {
                    everSeen.Add(enemy);
                    Assert.IsTrue(enemy.IsAlive);
                }

                var released = live.ToList();
                foreach (var enemy in live)
                    enemy.Despawn();

                Assert.AreEqual(0, _spawner.AliveCount);
                Assert.AreEqual(_registryBaseline, EnemyRegistry.Count, "Released enemies must unregister.");
                if (EnemyRegistry.TryFindNearest(Vector2.zero, out var nearest))
                    CollectionAssert.DoesNotContain(released, nearest);
                Assert.IsEmpty(LiveEnemies(), "Released enemies are deactivated in the pool.");
            }

            Assert.LessOrEqual(everSeen.Count, 3, "Enemies must be reused instead of re-instantiated.");
        }

        [Test]
        public void KilledEnemy_UnregistersAndReturnsToPoolForReuse()
        {
            _spawner.Initialize(CreateDirector(maxAlive: 1));
            _spawner.Tick(1f, 1f, true);
            var first = LiveEnemies().Single();

            first.TakeDamage(1000f);

            Assert.AreEqual(0, _spawner.AliveCount);
            Assert.AreEqual(_registryBaseline, EnemyRegistry.Count);

            _spawner.Tick(2f, 1f, true);
            var second = LiveEnemies().Single();
            Assert.AreSame(first, second);
            Assert.IsTrue(second.IsAlive);
            Assert.AreEqual(second.Definition.MaxHealth, second.Health.CurrentHealth);
        }

        [Test]
        public void Shutdown_DespawnsLiveEnemiesAndClearsRegistry()
        {
            _spawner.Initialize(CreateDirector());
            for (var second = 1; second <= 3; second++)
                _spawner.Tick(second, 1f, true);
            Assert.AreEqual(3, _spawner.AliveCount);

            _spawner.Shutdown();

            Assert.AreEqual(0, _spawner.AliveCount);
            Assert.AreEqual(_registryBaseline, EnemyRegistry.Count);
            Assert.IsNull(_spawner.Director);
        }
    }
}
