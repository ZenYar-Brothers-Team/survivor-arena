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

        private WaveDirector CreateBurstDirector(int count) => new WaveDirector(
            new WaveTimelineDefinition("FIXTURE-BURST-T", 3, WaveTestData.SpawnRadius,
                new[] { new WavePhaseDefinition("FIXTURE-BURST-P", "Burst", WavePhaseTag.Pressure, 60, 1, 1,
                    new[] { WaveTestData.Entry("FIXTURE-ENEMY-A") }, spawnMode: WaveSpawnMode.Burst,
                    burst: new WaveBurstDefinition(count, 0, 1)) }), WaveTestData.TestEnemies(), 60);

        [Test]
        public void Tick_MissingTarget_ReportsZeroActualWithoutRetryingBurst()
        {
            _spawner.Initialize(CreateBurstDirector(12));
            var serialized = new SerializedObject(_spawner);
            serialized.FindProperty("target").objectReferenceValue = null;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.AreEqual(0, _spawner.Tick(0, 0, true));
            Assert.AreEqual(12, _spawner.LastSpawnOutcome.Decision.Requested);
            Assert.AreEqual(12, _spawner.LastSpawnOutcome.Unavailable);
            Assert.AreEqual(0, _spawner.LastSpawnOutcome.Decision.Deferred);
            serialized.FindProperty("target").objectReferenceValue = _targetObject.transform;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.AreEqual(0, _spawner.Tick(.5f, .5f, true));
            Assert.AreEqual(_registryBaseline, EnemyRegistry.Count);
        }

        [Test]
        public void Tick_BossAndTraveler_DoNotConsumeRegularCapacity()
        {
            _spawner.Initialize(CreateDirector(1));
            var run = _runObject.GetComponent<RunController>();
            var boss = EnemyFactory.Spawn(WaveTestData.Enemy("FIXTURE-BOSS"), Vector2.zero,
                _targetObject.transform, run, _targetObject.transform, category: EnemyCategory.Boss);
            var traveler = EnemyFactory.Spawn(WaveTestData.Enemy("FIXTURE-TRAVELER"), Vector2.zero,
                _targetObject.transform, run, _targetObject.transform, category: EnemyCategory.Traveler);
            Assert.AreEqual(1, _spawner.Tick(1, 1, true));
            Assert.AreEqual(1, _spawner.AliveCount);
            Assert.AreEqual(_registryBaseline + 3, EnemyRegistry.Count);
            Assert.AreEqual(0, _spawner.Tick(2, 1, true));
            Assert.AreEqual(1, _spawner.LastSpawnOutcome.Decision.Suppressed);
            boss.Despawn();
            traveler.Despawn();
        }

        [Test]
        public void Burst_Load100AcrossTenRuns_ReusesPoolWithinApprovedSpawnBudgets()
        {
            // User-approved IP-14 spawn-only bound: cold <=250ms, warm <=50ms,
            // 100 enemies, 10 cycles. Does not claim physics/render frame time.
            var everSeen = new HashSet<EnemyRuntime>();
            Vector3[] firstPositions = null;
            var cold = 0d;
            var warmMax = 0d;
            for (var cycle = 0; cycle < 10; cycle++)
            {
                _spawner.Initialize(CreateBurstDirector(100));
                var clock = System.Diagnostics.Stopwatch.StartNew();
                var actual = _spawner.Tick(0, 0, true);
                clock.Stop();
                var milliseconds = clock.Elapsed.TotalMilliseconds;
                if (cycle == 0) cold = milliseconds; else warmMax = System.Math.Max(warmMax, milliseconds);
                Assert.AreEqual(100, actual);
                Assert.AreEqual(100, _spawner.LastSpawnOutcome.Actual);
                Assert.AreEqual(0, _spawner.LastSpawnOutcome.Decision.Suppressed);
                var live = LiveEnemies();
                var positions = live.Select(e => e.transform.position).OrderBy(p => p.x).ThenBy(p => p.y).ToArray();
                if (cycle == 0) firstPositions = positions; else CollectionAssert.AreEqual(firstPositions, positions);
                foreach (var enemy in live) everSeen.Add(enemy);
                Assert.AreEqual(_registryBaseline + 100, EnemyRegistry.Count);
                Assert.AreEqual(0, _spawner.Tick(.5f, .5f, true));
                _spawner.Shutdown();
                Assert.AreEqual(_registryBaseline, EnemyRegistry.Count);
                Assert.AreEqual(100, _spawnerObject.GetComponentsInChildren<EnemyRuntime>(true).Length);
            }
            TestContext.WriteLine($"IP-14 load: CPU={SystemInfo.processorType}; RAM={SystemInfo.systemMemorySize}MB; Unity={Application.unityVersion}; count=100; cycles=10; cold={cold:F3}ms; warmMax={warmMax:F3}ms; unique={everSeen.Count}");
            Assert.AreEqual(100, everSeen.Count);
            Assert.LessOrEqual(cold, 250d, "Approved cold spawn budget (ms).");
            Assert.LessOrEqual(warmMax, 50d, "Approved warm spawn budget (ms).");
        }

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

            Assert.AreEqual(1, _spawner.Capture().Kills);
            Assert.AreEqual(EnemyLifeReason.Killed, _spawner.LastLifeEvent.Reason);

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
