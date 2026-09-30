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
                maxAlive,
                new[]
                {
                    WaveTestData.Phase("FIXTURE-P", WavePhaseTag.Ordinary, 60f, 1f, null,
                        WaveTestData.Entry("FIXTURE-ENEMY-A"))
                });
            return new WaveDirector(timeline, WaveTestData.TestEnemies(), 60f);
        }

        private List<EnemyRuntime> LiveEnemies() =>
            _spawnerObject.GetComponentsInChildren<EnemyRuntime>(false).ToList();

        private WaveDirector CreateBurstDirector(int count) => new WaveDirector(
            new WaveTimelineDefinition("FIXTURE-BURST-T", 3, WaveTestData.SpawnRadius, 300,
                new[] { new WavePhaseDefinition("FIXTURE-BURST-P", "Burst", WavePhaseTag.Pressure, 60, 1,
                    new[] { WaveTestData.Entry("FIXTURE-ENEMY-A") }, spawnMode: WaveSpawnMode.Burst,
                    burst: new WaveBurstDefinition(count, 0, 1)) }), WaveTestData.TestEnemies(), 60);

        [Test]
        public void Tick_DenseOrdinaryGroup_StaggersOneFanAfterTenSeconds()
        {
            var breakup = new BlobBreakupDefinition(10f, 11, .7f, 60, 65f, 3f, 1.5f, 6f);
            var phase = new WavePhaseDefinition("FIXTURE-BLOB-P", "Blob", WavePhaseTag.Pressure,
                60f, 1f, new[] { WaveTestData.Entry("FIXTURE-ENEMY-A") },
                spawnMode: WaveSpawnMode.Burst, burst: new WaveBurstDefinition(30, 0, 1),
                blobBreakup: breakup);
            var timeline = new WaveTimelineDefinition("FIXTURE-BLOB-T", 37, 6f, 300, new[] { phase });
            _spawner.Initialize(new WaveDirector(timeline, WaveTestData.TestEnemies(), 60f));
            Assert.AreEqual(30, _spawner.Tick(0f, 0f, true));
            var enemies = LiveEnemies();
            for (var i = 0; i < enemies.Count; i++)
                enemies[i].transform.position = new Vector3(-5f + i % 6 * .25f, -.5f + i / 6 * .25f);

            _spawner.Tick(9.9f, 0f, true);
            Assert.AreEqual(0, _spawner.LastBlobBreakupCount);
            _spawner.Tick(10f, 0f, true);

            Assert.Greater(_spawner.LastBlobBreakupCount, 2);
            Assert.Less(_spawner.LastBlobBreakupCount, 30);
            Assert.AreEqual(_spawner.LastBlobBreakupCount, enemies.Count(enemy => enemy.BlobBreakupActive));
            Assert.AreEqual(30, _spawner.AliveCount);
        }

        [Test]
        public void Tick_FilteredBlob_CountsAllOrdinaryButMovesOnlyAllowedType()
        {
            var breakup = new BlobBreakupDefinition(10f, 30, 1f, 60, 65f, 3f, 1.5f, 4f,
                new[] { new Game.Content.ContentId("FIXTURE-ENEMY-A") });
            var phase = new WavePhaseDefinition("FIXTURE-BLOB-FILTER-P", "Blob", WavePhaseTag.Pressure,
                60f, 1f, new[] { WaveTestData.Entry("FIXTURE-ENEMY-A"), WaveTestData.Entry("FIXTURE-ENEMY-B") },
                spawnMode: WaveSpawnMode.Burst, burst: new WaveBurstDefinition(30, 0, 1),
                blobBreakup: breakup);
            var timeline = new WaveTimelineDefinition("FIXTURE-BLOB-FILTER-T", 37, 6f, 300, new[] { phase });
            _spawner.Initialize(new WaveDirector(timeline, WaveTestData.TestEnemies(), 60f));
            Assert.AreEqual(30, _spawner.Tick(0f, 0f, true));
            var enemies = LiveEnemies();
            for (var i = 0; i < enemies.Count; i++)
                enemies[i].transform.position = new Vector3(-5f + i % 6 * .25f, -.5f + i / 6 * .25f);
            var allowed = enemies.Where(enemy => enemy.ContentId == "FIXTURE-ENEMY-A").ToList();
            var restricted = enemies.Where(enemy => enemy.ContentId == "FIXTURE-ENEMY-B").ToList();
            Assert.IsNotEmpty(allowed);
            Assert.IsNotEmpty(restricted);

            _spawner.Tick(10f, 0f, true);

            Assert.AreEqual(allowed.Count, _spawner.LastBlobBreakupCount);
            Assert.IsTrue(allowed.All(enemy => enemy.BlobBreakupActive));
            Assert.IsTrue(restricted.All(enemy => !enemy.BlobBreakupActive));
        }

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
            Assert.AreEqual(1, _spawner.Tick(2, 1, true));
            Assert.AreEqual(0, _spawner.LastSpawnOutcome.Decision.Suppressed);
            Assert.IsTrue(boss.IsAlive);
            Assert.IsTrue(traveler.IsAlive);
            Assert.AreEqual(_registryBaseline + 3, EnemyRegistry.Count);
            boss.Despawn();
            traveler.Despawn();
        }

        [Test]
        public void Tick_AtCap_ErasesFarthestOrdinaryWithoutLifeOrRewardEvents()
        {
            _spawner.Initialize(CreateDirector(maxAlive: 2));
            Assert.AreEqual(1, _spawner.Tick(1, 1, true));
            Assert.AreEqual(1, _spawner.Tick(2, 1, true));
            var enemies = LiveEnemies();
            enemies[0].transform.position = Vector3.right;
            enemies[1].transform.position = Vector3.right * 20f;
            var nearLife = enemies[0].LifeId;
            var farLife = enemies[1].LifeId;
            var events = new List<EnemyLifeEvent>();
            _spawner.LifeEvent += events.Add;

            Assert.AreEqual(1, _spawner.Tick(3, 1, true));

            Assert.AreEqual(2, _spawner.AliveCount);
            Assert.AreEqual(_registryBaseline + 2, EnemyRegistry.Count);
            Assert.IsTrue(LiveEnemies().Any(enemy => enemy.LifeId == nearLife));
            Assert.IsFalse(LiveEnemies().Any(enemy => enemy.LifeId == farLife));
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(EnemyLifeEventKind.Spawned, events[0].Kind);
            Assert.AreEqual(0, _spawner.Capture().Kills);
        }

        [Test]
        public void Burst_Load300AcrossTenRuns_ReusesPoolWithinApprovedSpawnBudgets()
        {
            // Current fixture maximum: 300 enemies over 10 pool-reuse cycles.
            // This measures spawn/pool CPU only, not physics/render frame time.
            var everSeen = new HashSet<EnemyRuntime>();
            Vector3[] firstPositions = null;
            var cold = 0d;
            var warmMax = 0d;
            for (var cycle = 0; cycle < 10; cycle++)
            {
                _spawner.Initialize(CreateBurstDirector(300));
                var clock = System.Diagnostics.Stopwatch.StartNew();
                var actual = _spawner.Tick(0, 0, true);
                clock.Stop();
                var milliseconds = clock.Elapsed.TotalMilliseconds;
                if (cycle == 0) cold = milliseconds; else warmMax = System.Math.Max(warmMax, milliseconds);
                Assert.AreEqual(300, actual);
                Assert.AreEqual(300, _spawner.LastSpawnOutcome.Actual);
                Assert.AreEqual(0, _spawner.LastSpawnOutcome.Decision.Suppressed);
                var live = LiveEnemies();
                var positions = live.Select(e => e.transform.position).OrderBy(p => p.x).ThenBy(p => p.y).ToArray();
                if (cycle == 0) firstPositions = positions; else CollectionAssert.AreEqual(firstPositions, positions);
                foreach (var enemy in live) everSeen.Add(enemy);
                Assert.AreEqual(_registryBaseline + 300, EnemyRegistry.Count);
                Assert.AreEqual(0, _spawner.Tick(.5f, .5f, true));
                _spawner.Shutdown();
                Assert.AreEqual(_registryBaseline, EnemyRegistry.Count);
                Assert.AreEqual(300, _spawnerObject.GetComponentsInChildren<EnemyRuntime>(true).Length);
            }
            TestContext.WriteLine($"IP-14 load: CPU={SystemInfo.processorType}; RAM={SystemInfo.systemMemorySize}MB; Unity={Application.unityVersion}; count=300; cycles=10; cold={cold:F3}ms; warmMax={warmMax:F3}ms; unique={everSeen.Count}");
            Assert.AreEqual(300, everSeen.Count);
            Assert.LessOrEqual(cold, 250d, "Approved cold spawn budget (ms).");
            Assert.LessOrEqual(warmMax, 50d, "Approved warm spawn budget (ms).");
        }

        [Test]
        public void Tick_SpawnsUpToTimelineTechnicalCapAndRegistersEnemies()
        {
            _spawner.Initialize(CreateDirector());

            var spawned = 0;
            for (var second = 1; second <= 6; second++)
                spawned += _spawner.Tick(second, 1f, true);

            Assert.AreEqual(6, spawned, "Cadence continues while the cap is full.");
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
