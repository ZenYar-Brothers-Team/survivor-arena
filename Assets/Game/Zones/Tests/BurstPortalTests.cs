using System;
using Game.Content;
using Game.Zones.Json;
using NUnit.Framework;
using UnityEngine;

namespace Game.Zones.Tests
{
    public sealed class BurstPortalTests
    {
        private static ZoneEffectData Data() => new ZoneEffectData
        {
            Id = "T-JUMP", Kind = ZoneEffectKind.Portal, Radius = 3f, Color = "#b780ff",
            Lifetime = ZoneLifetimeMode.Burst, PulsePeriodSeconds = 40f, TelegraphSeconds = 5f,
            FlashSeconds = .6f, PortalJumpDistance = 5f, VerticalScale = .8f, ActiveRadiusFraction = .7f,
            RelocatesBetweenCycles = true
        };

        [TestCase(.01f)]
        [TestCase(2f)]
        public void Portal_FiresOnceAtWarningEnd_EvenWhenTickCrossesFlash(float crossingStep)
        {
            var data = Data();
            var layout = new ZoneLayoutDefinition(ZoneTestData.Layout(new[] { data }, (data.Id, 1)));
            var zone = new ZonePlacement(0, layout.Effects[new ContentId(data.Id)], Vector2.zero);
            var player = new FakeZonePlayer { Position = Vector2.zero };
            var enemies = new FakeZoneEnemies().Add(Vector2.zero);
            using var runtime = new ZoneRuntime(new[] { zone }, new ZonePlacementRules(layout, 120f, Vector2.zero, null),
                6, player, enemies, () => new Rect(-20f, -12f, 40f, 24f));
            runtime.Tick(4.99f); Assert.AreEqual(0, player.Teleports.Count);
            runtime.Tick(crossingStep);
            Assert.AreEqual(1, player.Teleports.Count);
            Assert.AreEqual(5f, player.Position.magnitude, .0001f);
            Assert.AreEqual(-1, zone.PartnerIndex);
            Assert.AreEqual(0, enemies.Teleports);
            Assert.AreEqual(0f, runtime.PortalCooldownRemaining);
            Assert.AreEqual(0f, runtime.BuffRemaining(zone.Effect), "Teleport is not a timed stat buff.");
            player.Position = Vector2.zero;
            runtime.Tick(.1f); Assert.AreEqual(1, player.Teleports.Count, "No retrigger by entering the fading seal.");
            runtime.Tick(0f); Assert.AreEqual(1, player.Teleports.Count, "Pause does not advance the trigger.");
        }

        [TestCase(false, 0f)]
        [TestCase(true, 2.11f)]
        public void Portal_DeadOrOutsideMidpoint_DoesNotTeleport(bool alive, float offset)
        {
            var data = Data();
            var layout = new ZoneLayoutDefinition(ZoneTestData.Layout(new[] { data }, (data.Id, 1)));
            var player = new FakeZonePlayer { IsAlive = alive, Position = Vector2.right * offset };
            using var runtime = new ZoneRuntime(new[] { new ZonePlacement(0, layout.Effects[new ContentId(data.Id)], Vector2.zero) },
                new ZonePlacementRules(layout, 120f, Vector2.zero, null), 6, player, new FakeZoneEnemies(),
                () => new Rect(-20f, -12f, 40f, 24f));
            runtime.Tick(5f); Assert.AreEqual(0, player.Teleports.Count);
        }

        [Test]
        public void PortalExit_NearArenaEdge_KeepsExactDistanceAndSafeBodyClearance()
        {
            var data = Data();
            var layout = new ZoneLayoutDefinition(ZoneTestData.Layout(new[] { data }, (data.Id, 1)));
            var rules = new ZonePlacementRules(layout, 120f, Vector2.zero, null);
            var origin = new Vector2(55f, 55f);
            Assert.IsTrue(rules.TryPickPortalExit(origin, 5f, new System.Random(6), out var target));
            Assert.AreEqual(5f, Vector2.Distance(origin, target), .0001f);
            Assert.LessOrEqual(Mathf.Abs(target.x), 52.5f); Assert.LessOrEqual(Mathf.Abs(target.y), 52.5f);
            Assert.IsFalse(rules.TryPickPortalExit(Vector2.zero, 200f, new System.Random(6), out _),
                "An impossible exit never shortens or clamps the jump.");
        }

        [Test]
        public void Portal_SpawnsAloneOnGeneralChain_AndLayoutAllowsOddCount()
        {
            var data = Data();
            var authoring = ZoneTestData.Layout(new[] { data }, (data.Id, 5));
            authoring.RandomSchedule = new RandomZoneScheduleData { Chains = 1, IntervalMinSeconds = 1f,
                IntervalMaxSeconds = 1f, SpawnRadiusScreenWidths = 1f };
            var layout = new ZoneLayoutDefinition(authoring);
            var rules = new ZonePlacementRules(layout, 120f, Vector2.zero, null);
            var zones = ZoneLayoutGenerator.Generate(layout, 120f, Vector2.zero, null, 6);
            using var scheduler = new RandomZoneScheduler(layout.RandomSchedule, rules, zones, 6);
            scheduler.Tick(1f, new Rect(-20f, -12f, 40f, 24f), Vector2.zero);
            var present = 0;
            foreach (var zone in zones)
            {
                Assert.AreEqual(-1, zone.PartnerIndex);
                if (zone.IsPresent) present++;
            }
            Assert.AreEqual(1, present);
            Assert.IsFalse(layout.RandomSchedule.HasPortalChain);
            Assert.IsTrue(scheduler.FinishTick(7f));
            foreach (var zone in zones) Assert.IsFalse(zone.IsPresent);
        }

        [Test]
        public void Portal_RejectsPairedFieldsEnemyTargetsAndMissingDistance()
        {
            var data = Data(); data.PortalPairScreenHeights = 1f;
            Assert.Throws<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = Data(); data.AffectsBothSides = true;
            Assert.Throws<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = Data(); data.PortalJumpDistance = null;
            Assert.Throws<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = ZoneTestData.Haste(); data.PortalJumpDistance = 5f;
            Assert.Throws<ArgumentException>(() => new ZoneEffectDefinition(data));
        }
    }
}
