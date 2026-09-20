using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Game.Enemy.Tests
{
    public class WaveDirectorTests
    {
        private const float RunDuration = 30f;

        private static WaveDirector CreateDirector(int seed = 7) =>
            new WaveDirector(WaveTestData.ThreePhaseTimeline(seed), WaveTestData.TestEnemies(), RunDuration);

        [Test]
        public void Advance_WalksPhasesInOrderAndRaisesOneEventPerTransition()
        {
            var director = CreateDirector();
            var transitions = new List<string>();
            director.PhaseChanged += (phase, index) => transitions.Add($"{index}:{phase.Id}");

            director.Advance(0f, 0f, true, 0);
            Assert.AreEqual(0, director.CurrentPhaseIndex);
            director.Advance(9.9f, 0f, true, 0);
            Assert.AreEqual(0, director.CurrentPhaseIndex);
            director.Advance(10f, 0f, true, 0);
            Assert.AreEqual(1, director.CurrentPhaseIndex);
            director.Advance(24f, 0f, true, 0);
            Assert.AreEqual(1, director.CurrentPhaseIndex);
            director.Advance(26f, 0f, true, 0);
            Assert.AreEqual(2, director.CurrentPhaseIndex);

            CollectionAssert.AreEqual(
                new[] { "1:FIXTURE-PHASE-PRESSURE", "2:FIXTURE-PHASE-REST" },
                transitions);
        }

        [Test]
        public void Advance_SkippingSeveralPhasesRaisesSingleTransitionToTheLatest()
        {
            var director = CreateDirector();
            var transitions = 0;
            director.PhaseChanged += (_, __) => transitions++;

            director.Advance(28f, 0f, true, 0);

            Assert.AreEqual(2, director.CurrentPhaseIndex);
            Assert.AreEqual(1, transitions);
        }

        [Test]
        public void Advance_LastPhaseHoldsUntilRunEnd()
        {
            var director = CreateDirector();

            director.Advance(500f, 0f, true, 0);

            Assert.AreEqual(2, director.CurrentPhaseIndex);
            Assert.AreEqual(WavePhaseTag.Rest, director.CurrentPhase.Tag);
        }

        [Test]
        public void Advance_WhenNotRunningSpawnsNothingAndDoesNotTransition()
        {
            var director = CreateDirector();
            var transitions = 0;
            director.PhaseChanged += (_, __) => transitions++;

            Assert.AreEqual(0, director.Advance(15f, 5f, false, 0));

            Assert.AreEqual(0, director.CurrentPhaseIndex);
            Assert.AreEqual(0f, director.Elapsed);
            Assert.AreEqual(0, transitions);

            Assert.AreEqual(2, director.Advance(15f, 1f, true, 0));
            Assert.AreEqual(1, director.CurrentPhaseIndex);
            Assert.AreEqual(1, transitions);
        }

        [Test]
        public void Advance_IntensityFollowsPhaseIntervalAndCapacity()
        {
            var director = CreateDirector();

            Assert.AreEqual(0, director.Advance(1f, 1f, true, 0));
            Assert.AreEqual(1, director.Advance(2f, 1f, true, 0), "Ordinary phase spawns every 2s.");
            Assert.AreEqual(0, director.Advance(4.5f, 2f, true, 4), "Alive at the cap blocks spawning.");

            director.Advance(10f, 0f, true, 0);
            Assert.AreEqual(4, director.Advance(12f, 2f, true, 0), "Pressure phase spawns every 0.5s.");
            Assert.AreEqual(1, director.Advance(13f, 1f, true, 7), "Only the remaining capacity is spawned.");
        }

        [Test]
        public void Advance_RejectsInvalidArguments()
        {
            var director = CreateDirector();

            Assert.Throws<ArgumentOutOfRangeException>(() => director.Advance(-1f, 0f, true, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => director.Advance(0f, float.NaN, true, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => director.Advance(0f, 0f, true, -1));
        }

        [Test]
        public void SelectEnemy_UsesOnlyCurrentPhaseCompositionWithPhaseModifiers()
        {
            var director = CreateDirector();

            for (var i = 0; i < 20; i++)
            {
                var ordinary = director.SelectEnemy();
                Assert.AreEqual("FIXTURE-ENEMY-A", ordinary.Id.ToString());
                Assert.AreEqual(10f, ordinary.MaxHealth);
            }

            director.Advance(10f, 0f, true, 0);
            for (var i = 0; i < 20; i++)
            {
                var pressure = director.SelectEnemy();
                Assert.AreEqual("FIXTURE-ENEMY-B", pressure.Id.ToString());
                Assert.AreEqual(10f, pressure.MaxHealth, "Base health 20 x 0.5.");
                Assert.AreEqual(3f, pressure.MovementSpeed, "Base speed 2 x 1.5.");
            }
        }

        [Test]
        public void SelectEnemy_IsDeterministicPerSeedAndRespectsWeights()
        {
            WaveDirector Build(int seed) => new WaveDirector(
                new WaveTimelineDefinition(
                    "FIXTURE-WAVE-WEIGHTS",
                    seed,
                    WaveTestData.SpawnRadius,
                    new[]
                    {
                        WaveTestData.Phase("FIXTURE-P", WavePhaseTag.Ordinary, 60f, 1f, 5, null,
                            WaveTestData.Entry("FIXTURE-ENEMY-A", 3f),
                            WaveTestData.Entry("FIXTURE-ENEMY-B", 1f))
                    }),
                WaveTestData.TestEnemies(),
                RunDuration);

            var first = Build(42);
            var second = Build(42);
            var counts = new Dictionary<string, int>();
            for (var i = 0; i < 2000; i++)
            {
                var a = first.SelectEnemy().Id.ToString();
                Assert.AreEqual(a, second.SelectEnemy().Id.ToString());
                counts[a] = counts.TryGetValue(a, out var seen) ? seen + 1 : 1;
            }

            var ratio = (float)counts["FIXTURE-ENEMY-A"] / counts["FIXTURE-ENEMY-B"];
            Assert.That(ratio, Is.InRange(2.4f, 3.7f));
        }

        [Test]
        public void Hooks_FireOnceInTimeOrderAndOnlyWhileRunning()
        {
            var director = CreateDirector();
            var fired = new List<WaveHookKind>();
            director.HookTriggered += hook => fired.Add(hook.Kind);
            Assert.AreEqual(WaveHookKind.MidBoss, director.NextHook.Kind);

            director.Advance(11.9f, 0f, true, 0);
            Assert.IsEmpty(fired);

            director.Advance(13f, 0f, false, 0);
            Assert.IsEmpty(fired, "A paused run must not raise hooks.");

            director.Advance(13f, 0f, true, 0);
            director.Advance(14f, 0f, true, 0);
            Assert.AreEqual(new[] { WaveHookKind.MidBoss }, fired.ToArray());
            Assert.AreEqual(WaveHookKind.FinalBoss, director.NextHook.Kind);

            director.Advance(29f, 0f, true, 0);
            director.Advance(30f, 0f, true, 0);
            Assert.AreEqual(new[] { WaveHookKind.MidBoss, WaveHookKind.FinalBoss }, fired.ToArray());
            Assert.IsNull(director.NextHook);
        }

        [Test]
        public void Constructor_RejectsUnknownEnemiesAndHooksAfterRunEnd()
        {
            var missing = WaveTestData.Enemies(WaveTestData.Enemy("FIXTURE-ENEMY-A"));
            Assert.Throws<InvalidOperationException>(() =>
                new WaveDirector(WaveTestData.ThreePhaseTimeline(), missing, RunDuration));

            Assert.Throws<InvalidOperationException>(() =>
                new WaveDirector(WaveTestData.ThreePhaseTimeline(), WaveTestData.TestEnemies(), 20f));
            Assert.Throws<ArgumentNullException>(() =>
                new WaveDirector(null, WaveTestData.TestEnemies(), RunDuration));
        }

        [Test]
        public void FixtureTimeline_DrivesDirectorAcrossAllPhasesWithinRunDuration()
        {
            var enemies = FixtureEnemyCatalog.Create().ToDictionary(enemy => enemy.Id);
            var director = new WaveDirector(FixtureWaveTimelineCatalog.Create(), enemies, 15f * 60f);
            var seen = new List<int>();
            director.PhaseChanged += (_, index) => seen.Add(index);

            for (var second = 0; second <= 15 * 60; second++)
                director.Advance(second, 1f, true, 0);

            CollectionAssert.AreEqual(Enumerable.Range(1, director.PhaseCount - 1), seen);
            Assert.AreEqual(director.PhaseCount - 1, director.CurrentPhaseIndex);
            Assert.IsNull(director.NextHook);
        }
    }
}
