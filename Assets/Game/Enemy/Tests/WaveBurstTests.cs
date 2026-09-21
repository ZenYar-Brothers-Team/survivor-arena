using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Game.Enemy.Tests
{
    public sealed class WaveBurstTests
    {
        private static WavePhaseDefinition Burst(string id = "FIXTURE-B", int count = 12,
            float offset = 1, float window = 2) => new WavePhaseDefinition(id, "Burst", WavePhaseTag.Pressure,
                5, 1, 2, new[] { WaveTestData.Entry("FIXTURE-ENEMY-A") },
                spawnMode: WaveSpawnMode.Burst, burst: new WaveBurstDefinition(count, offset, window));

        private static WaveDirector Director(params WavePhaseDefinition[] phases) => new WaveDirector(
            new WaveTimelineDefinition("FIXTURE-T", 42, 6, phases), WaveTestData.TestEnemies(), 30);

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(12)]
        public void Burst_AtFullCap_EmitsWholeGroupExactlyOnce(int count)
        {
            var director = Director(Burst(count: count));
            Assert.AreEqual(0, director.Advance(.9f, .9f, true, 500));
            Assert.AreEqual(count, director.Advance(1, .1f, true, 500));
            Assert.AreEqual(count, director.LastDecision.Requested);
            Assert.AreEqual(0, director.LastDecision.Suppressed);
            Assert.AreEqual(0, director.LastDecision.Deferred);
            Assert.IsTrue(director.BurstConsumed);
            Assert.AreEqual(0, director.Advance(1, 0, true, 500));
            Assert.AreEqual(0, director.Advance(2, 1, true, 0));
        }

        [TestCase(3f)]
        [TestCase(100f)]
        public void Burst_SkippedWindowIncludingLastHold_ExpiresWithoutReplay(float time)
        {
            var director = Director(Burst());
            Assert.AreEqual(0, director.Advance(time, time, true, 0));
            Assert.AreEqual(12, director.LastDecision.Expired);
            Assert.AreEqual(0, director.LastDecision.Requested);
            Assert.AreEqual(0, director.Advance(time + 1, 1, true, 0));
            Assert.AreEqual(0, director.LastDecision.Expired);
        }

        [Test]
        public void Burst_PauseAndTerminal_DoNotConsumeWindowOrSpawn()
        {
            var director = Director(Burst());
            Assert.AreEqual(0, director.Advance(1, 1, false, 0));
            Assert.AreEqual(0, director.Elapsed);
            Assert.IsFalse(director.BurstConsumed);
            Assert.AreEqual(12, director.Advance(1, 1, true, 0));
            Assert.AreEqual(0, director.Advance(30, 29, false, 0));
            Assert.AreEqual(1, director.Elapsed);
            Assert.AreEqual(12, Director(Burst()).Advance(1, 1, true, 0), "New run owns a new director.");
        }

        [Test]
        public void Burst_SkipAcrossPhases_ExpiresOldGroupsAndEmitsOnlyOpenGroup()
        {
            var director = Director(Burst("FIXTURE-A"), Burst("FIXTURE-B"), Burst("FIXTURE-C"));
            Assert.AreEqual(12, director.Advance(11, 11, true, 200));
            Assert.AreEqual(24, director.LastDecision.Expired);
            Assert.AreEqual(2, director.CurrentPhaseIndex);
        }

        [Test]
        public void Continuous_SkippedBoundary_ChargesOnlyTimeInCurrentPhaseAndDiscardsCapSuppression()
        {
            var director = Director(Burst(), WaveTestData.Phase("FIXTURE-C", WavePhaseTag.Rest,
                10, 1, 2, null, WaveTestData.Entry("FIXTURE-ENEMY-A")));
            Assert.AreEqual(1, director.Advance(6, 6, true, 1));
            Assert.AreEqual(12, director.LastDecision.Expired);
            Assert.AreEqual(1, director.LastDecision.Requested);
            Assert.AreEqual(0, director.Advance(8, 2, true, 2));
            Assert.AreEqual(2, director.LastDecision.Suppressed);
            Assert.AreEqual(0, director.Advance(8, 0, true, 0));
            Assert.AreEqual(1, director.Advance(9, 1, true, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => director.Advance(8, 1, true, 0));
        }

        [Test]
        public void Hooks_SameTimeAndSkippedBoundaries_AreOrderedOnceBeforeBurst()
        {
            var director = new WaveDirector(new WaveTimelineDefinition("FIXTURE-T", 42, 6,
                new[] { Burst(offset: 0) }, new[] {
                    new WaveHookDefinition(WaveHookKind.FinalBoss, 0),
                    new WaveHookDefinition(WaveHookKind.MidBoss, 0) }), WaveTestData.TestEnemies(), 30);
            var hooks = new List<WaveHookKind>();
            director.HookTriggered += hook => hooks.Add(hook.Kind);
            Assert.AreEqual(12, director.Advance(0, 0, true, 0));
            CollectionAssert.AreEqual(new[] { WaveHookKind.MidBoss, WaveHookKind.FinalBoss }, hooks);
            director.Advance(4, 4, true, 0);
            Assert.AreEqual(2, hooks.Count);
        }

        [Test]
        public void Geometry_SameSeed_RepeatsAnglesWithoutChangingCompositionStream()
        {
            var first = Director(Burst());
            var second = Director(Burst());
            for (var i = 0; i < 100; i++)
            {
                first.SelectEnemy();
                var angle = first.SelectSpawnAngle();
                Assert.AreEqual(angle, second.SelectSpawnAngle());
                Assert.That(angle, Is.InRange(0d, Math.PI * 2d));
            }
        }

        [Test]
        public void BurstDefinition_InvalidModeWindowOrCount_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveBurstDefinition(-1, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveBurstDefinition(1, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveBurstDefinition(1, 0, float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveBurstDefinition(1, 0, 0));
            Assert.Throws<ArgumentException>(() => Burst(offset: 4, window: 2));
            Assert.Throws<ArgumentException>(() => new WavePhaseDefinition("FIXTURE-P", "P", WavePhaseTag.Pressure,
                5, 1, 2, new[] { WaveTestData.Entry("FIXTURE-ENEMY-A") }, spawnMode: WaveSpawnMode.Burst));
            Assert.Throws<ArgumentException>(() => new WavePhaseDefinition("FIXTURE-P", "P", WavePhaseTag.Pressure,
                5, 1, 2, new[] { WaveTestData.Entry("FIXTURE-ENEMY-A") }, burst: new WaveBurstDefinition(1, 0, 1)));
        }

        [Test]
        public void Catalog_ExplicitModesAndRequiredBurstFields_AreValidated()
        {
            var json = UnityEngine.Resources.Load<UnityEngine.TextAsset>("Content/Waves/FixtureWaveTimeline").text;
            var timeline = FixtureWaveTimelineCatalog.FromJson(json);
            Assert.Throws<InvalidOperationException>(() => FixtureWaveTimelineCatalog.FromJson(json.Replace("\"seed\": 24680,", "")));
            CollectionAssert.AreEqual(new[] { 18, 26, 34 }, timeline.Phases.Where(p => p.Burst != null).Select(p => p.Burst.Count));
            Assert.Throws<InvalidOperationException>(() => FixtureWaveTimelineCatalog.FromJson(json.Replace("\"spawnMode\": \"Continuous\",", "")));
            Assert.Throws<InvalidOperationException>(() => FixtureWaveTimelineCatalog.FromJson(json.Replace("\"count\": 18,", "")));
            Assert.Throws<InvalidOperationException>(() => FixtureWaveTimelineCatalog.FromJson(json.Replace("\"offsetSeconds\": 0.0,", "")));
            Assert.Throws<InvalidOperationException>(() => FixtureWaveTimelineCatalog.FromJson(json.Replace(", \"windowSeconds\": 1.0", "")));
        }
    }
}
