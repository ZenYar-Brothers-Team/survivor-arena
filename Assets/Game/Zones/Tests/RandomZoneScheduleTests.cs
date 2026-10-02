using System;
using System.Linq;
using Game.Zones.Json;
using NUnit.Framework;
using UnityEngine;

namespace Game.Zones.Tests
{
    public sealed class RandomZoneScheduleTests
    {
        private static ZoneLayoutDefinition Layout(bool burst = false, float minDelay = 1f, float maxDelay = 2f)
        {
            var effect = burst ? ZoneTestData.SpeedBurst() : ZoneTestData.Haste(mode: ZoneLifetimeMode.Pulsing);
            effect.Radius = burst ? 1.25f : 6f; effect.MinRadius = burst ? 1.25f : 2f;
            if (!burst) { effect.PulsePrepareSeconds = 5f; effect.PulseIdleVisibility = .14f; }
            var data = ZoneTestData.Layout(new[] { effect }, (effect.Id, 4));
            data.RandomSchedule = new RandomZoneScheduleData { Chains = 2, IntervalMinSeconds = minDelay,
                IntervalMaxSeconds = maxDelay, SpawnRadiusScreenWidths = 1f };
            data.ActiveScreenMargin = 2f; // wide enough that anything spawned within one screen width is simulated
            return new ZoneLayoutDefinition(data);
        }

        [Test]
        public void Schedule_TwoIndependentSlots_CameraBoundSpawns_FiniteLivesAndSeededRandomDelays()
        {
            var layout = Layout();
            var view = new Rect(-20f, -12f, 40f, 24f);
            var zones = ZoneLayoutGenerator.Generate(layout, 120f, Vector2.zero, null, 6);
            var rules = new ZonePlacementRules(layout, 120f, Vector2.zero, null);
            var scheduler = new RandomZoneScheduler(layout.RandomSchedule, rules, zones, 6);
            var firstTimes = new System.Collections.Generic.List<float>();
            var lastCycles = zones.Select(z => z.Cycle).ToArray();
            var spawnCount = 0; var sawTwo = false;
            for (var time = 0f; time < 180f; time += .1f)
            {
                if (time > 60f) view = new Rect(8f, 8f, 40f, 24f);
                scheduler.Tick(time, view, view.center);
                Assert.LessOrEqual(zones.Count(z => z.IsPresent), 2, "Preparation and fade also occupy slots.");
                sawTwo |= zones.Count(z => z.IsPresent) == 2;
                for (var i = 0; i < zones.Count; i++)
                {
                    var zone = zones[i];
                    if (zone.Cycle == lastCycles[i]) continue;
                    lastCycles[i] = zone.Cycle; spawnCount++;
                    if (firstTimes.Count < 2) firstTimes.Add(time);
                    Assert.LessOrEqual(Vector2.Distance(zone.Center, view.center), view.width + 1e-3f, "Within one screen width of the player.");
                    Assert.That(zone.Radius, Is.InRange(2f, 6f));
                    Assert.AreEqual(.14f, zone.Visibility(time), .0001f);
                    Assert.IsFalse(zone.IsActive(time));
                }
                scheduler.FinishTick(time);
            }
            Assert.Greater(spawnCount, 8); Assert.IsTrue(sawTwo);
            Assert.AreNotEqual(firstTimes[0], firstTimes[1], "The chains have separate random clocks.");
            scheduler.Dispose(); Assert.IsTrue(zones.All(z => !z.IsPresent && !z.IsNear));
        }

        [Test]
        public void Schedule_BurstHitsOnceAtExactRadius_ExpiresAndLeavesTimedBuff()
        {
            var layout = Layout(burst: true, minDelay: 1f, maxDelay: 1f);
            var zones = ZoneLayoutGenerator.Generate(layout, 120f, Vector2.zero, null, 6);
            var player = new FakeZonePlayer();
            using var runtime = new ZoneRuntime(zones, new ZonePlacementRules(layout, 120f, Vector2.zero, null),
                6, player, new FakeZoneEnemies(), () => new Rect(-20f, -12f, 40f, 24f));
            Assert.IsTrue(zones.All(z => !z.IsPresent));
            runtime.Tick(1f);
            var chosen = zones.First(z => z.IsPresent);
            player.Position = chosen.Center;
            Assert.IsTrue(chosen.Contains(chosen.Center + Vector2.right * 1.249f));
            Assert.IsFalse(chosen.Contains(chosen.Center + Vector2.right * 1.251f));
            runtime.Tick(4f);
            Assert.AreEqual(1f, runtime.SpeedBuffRemaining01);
            runtime.Tick(.6f);
            Assert.IsFalse(chosen.IsPresent, "No faint idle drawing remains after a one-shot life.");
            Assert.AreEqual(0, runtime.NearZoneCount, "Disappeared occurrences no longer count as nearby zones.");
            Assert.Greater(runtime.SpeedBuffRemaining01, 0f);
            var fraction = runtime.SpeedBuffRemaining01;
            runtime.Tick(0f); Assert.AreEqual(fraction, runtime.SpeedBuffRemaining01, "Pause does not advance the chain or buff.");
            runtime.Dispose(); Assert.IsTrue(zones.All(z => !z.IsPresent));
        }

        [Test]
        public void Schedule_NoCameraOrImpossibleScreen_DoesNotReuseOldOffscreenPositions()
        {
            var layout = Layout(); var zones = ZoneLayoutGenerator.Generate(layout, 120f, Vector2.zero, null, 6);
            using var schedule = new RandomZoneScheduler(layout.RandomSchedule,
                new ZonePlacementRules(layout, 120f, Vector2.zero, null), zones, 6);
            schedule.Tick(10f, default, Vector2.zero); Assert.IsTrue(zones.All(z => !z.IsPresent));
            schedule.Tick(20f, new Rect(1000f, 1000f, 20f, 20f), new Vector2(1010f, 1010f));
            Assert.IsTrue(zones.All(z => !z.IsPresent));
        }

        [Test]
        public void Radius_FixedLocationsUseSampledRadius_ContainmentMatchesAndSeedRepeats()
        {
            var effect = ZoneTestData.Haste(mode: ZoneLifetimeMode.Pulsing);
            effect.RelocatesBetweenCycles = false; effect.MinRadius = effect.Radius / 3f; effect.VerticalScale = .8f;
            var layout = new ZoneLayoutDefinition(ZoneTestData.Layout(new[] { effect }, (effect.Id, 4)));
            var a = ZoneLayoutGenerator.Generate(layout, 120f, Vector2.zero, null, 16);
            var b = ZoneLayoutGenerator.Generate(layout, 120f, Vector2.zero, null, 16);
            Assert.Greater(a.Select(z => z.Radius).Distinct().Count(), 1);
            for (var i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a[i].Radius, b[i].Radius); Assert.AreEqual(a[i].Center, b[i].Center);
                Assert.That(a[i].Radius, Is.InRange(effect.MinRadius.Value, effect.Radius.Value));
                Assert.IsTrue(a[i].Contains(a[i].Center + Vector2.up * a[i].Radius * .799f));
                Assert.IsFalse(a[i].Contains(a[i].Center + Vector2.up * a[i].Radius * .801f));
            }
        }

        [Test]
        public void ScheduleAndRadius_RejectIncompleteInvertedAndNonfiniteData()
        {
            Assert.Throws<ArgumentException>(() => new RandomZoneScheduleDefinition(new RandomZoneScheduleData()));
            Assert.Catch<ArgumentException>(() => Layout(minDelay: 2f, maxDelay: 1f));
            var data = ZoneTestData.Haste(); data.MinRadius = 6f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data.MinRadius = float.NaN; Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
        }
    }
}
