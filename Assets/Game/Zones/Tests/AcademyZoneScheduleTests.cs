using System;
using System.Linq;
using Game.Zones.Json;
using NUnit.Framework;
using UnityEngine;

namespace Game.Zones.Tests
{
    /// <summary>DECISION-0151: five schedule chains, repeatable kinds, screen-height portal pairs, experience and knockback fields.</summary>
    public sealed class AcademyZoneScheduleTests
    {
        private static readonly Rect View = new Rect(-20f, -5f, 40f, 10f);

        private static ZoneLayoutDefinition Layout(int chains, float delay, float? portalDelay, params (ZoneEffectData effect, int count)[] zones)
        {
            var data = ZoneTestData.Layout(zones.Select(z => z.effect).ToArray(), zones.Select(z => (z.effect.Id, z.count)).ToArray());
            data.RandomSchedule = new RandomZoneScheduleData { Chains = chains, IntervalMinSeconds = delay, IntervalMaxSeconds = delay,
                SpawnRadiusScreenWidths = 1f, PortalIntervalMinSeconds = portalDelay, PortalIntervalMaxSeconds = portalDelay };
            return new ZoneLayoutDefinition(data);
        }

        private static ZoneEffectData Permanent(ZoneEffectData data)
        {
            data.Lifetime = ZoneLifetimeMode.Permanent; data.RelocatesBetweenCycles = false;
            data.PulsePeriodSeconds = data.PulseVisibleSeconds = data.PulseFadeSeconds = data.PulsePrepareSeconds = data.PulseIdleVisibility = null;
            return data;
        }

        [Test]
        public void Schedule_FiveChainsOfOneKind_AllMayBePresentTogether_WithinOneScreenWidthOfThePlayer()
        {
            var layout = Layout(5, 1f, null, (ZoneTestData.ExperienceField(), 5));
            var zones = ZoneLayoutGenerator.Generate(layout, 120f, Vector2.zero, null, 6);
            using var scheduler = new RandomZoneScheduler(layout.RandomSchedule, new ZonePlacementRules(layout, 120f, Vector2.zero, null), zones, 6);
            var player = new Vector2(10f, -3f);
            scheduler.Tick(1f, View, player);
            Assert.AreEqual(5, zones.Count(z => z.IsPresent), "Duplicates of the same effect are allowed.");
            foreach (var zone in zones)
                Assert.LessOrEqual(Vector2.Distance(zone.Center, player), View.width + 1e-3f);
        }

        [Test]
        public void Schedule_PortalPair_LivesOnItsOwnChain_EndsExactlyOneScreenHeightApart_AndLeavesTogether()
        {
            var layout = Layout(1, 1f, 50f, (ZoneTestData.ExperienceField(), 1), (ZoneTestData.ScheduledPortal(), 2));
            var zones = ZoneLayoutGenerator.Generate(layout, 120f, Vector2.zero, null, 6);
            var portals = zones.Where(z => z.Effect.Kind == ZoneEffectKind.Portal).ToArray();
            Assert.AreEqual(portals[1].Index, portals[0].PartnerIndex); Assert.AreEqual(portals[0].Index, portals[1].PartnerIndex);
            using var scheduler = new RandomZoneScheduler(layout.RandomSchedule, new ZonePlacementRules(layout, 120f, Vector2.zero, null), zones, 6);
            for (var time = 1f; time < 50f; time += 1f)
            {
                scheduler.Tick(time, View, Vector2.zero); scheduler.FinishTick(time);
                Assert.IsTrue(portals.All(z => !z.IsPresent), "General chains never pick a portal; its own chain is still waiting.");
            }
            scheduler.Tick(50f, View, Vector2.zero);
            Assert.IsTrue(portals.All(z => z.IsPresent), "Both ends appear for the portal chain.");
            Assert.AreEqual(View.height, Vector2.Distance(portals[0].Center, portals[1].Center), 1e-3f);
            var end = portals[0].OccurrenceEndSeconds.Value;
            Assert.AreEqual(end, portals[1].OccurrenceEndSeconds.Value, 1e-4f);
            scheduler.FinishTick(end - .01f); Assert.IsTrue(portals.All(z => z.IsPresent), "Not before the visible life is over.");
            scheduler.FinishTick(end);
            Assert.IsTrue(portals.All(z => !z.IsPresent), "Both ends disappear together.");
        }

        [Test]
        public void Portal_PlayerOnlyPair_TeleportsThePlayerButNeverAnEnemy()
        {
            var layout = new ZoneLayoutDefinition(ZoneTestData.Layout(new[] { ZoneTestData.Portal() }, ("T-PORTAL", 2)));
            var zones = ZoneLayoutGenerator.Generate(layout, 120f, Vector2.zero, null, 6);
            var player = new FakeZonePlayer { Position = new Vector2(500f, 500f) };
            var enemies = new FakeZoneEnemies().Add(zones[0].Center);
            using var runtime = new ZoneRuntime(zones, new ZonePlacementRules(layout, 120f, Vector2.zero, null), 6, player, enemies,
                () => new Rect(zones[0].Center.x - 20f, zones[0].Center.y - 12f, 40f, 24f));
            runtime.Tick(.1f);
            Assert.AreEqual(0, enemies.Teleports, "Enemies stand in the doorway untouched.");
            player.Position = zones[0].Center;
            runtime.Tick(.1f);
            Assert.AreEqual(1, player.Teleports.Count);
        }

        [Test]
        public void Experience_WhileInside_MultipliesPickedUpExperienceByFive_AndEndsOutside()
        {
            var effect = ZoneTestData.ExperienceField(); Permanent(effect);
            var layout = new ZoneLayoutDefinition(ZoneTestData.Layout(new[] { effect }, (effect.Id, 1)));
            var player = new FakeZonePlayer { Position = Vector2.zero };
            using var runtime = new ZoneRuntime(new[] { new ZonePlacement(0, layout.Effects[new Game.Content.ContentId(effect.Id)], Vector2.zero) },
                new ZonePlacementRules(layout, 120f, Vector2.zero, null), 1, player, new FakeZoneEnemies(), () => View);
            runtime.Tick(.1f);
            Assert.AreEqual(4f, player.Zone.PickedUpXpMultiplierBonus, 1e-4f, "x5 is a +4 bonus on the picked-up multiplier.");
            player.Position = new Vector2(15f, 0f);
            runtime.Tick(.1f);
            Assert.AreEqual(0f, player.Zone.PickedUpXpMultiplierBonus, 1e-4f);
        }

        [Test]
        public void Knockback_PushesEnemiesInsideAwayFromTheCenter_NeverThePlayerNorOutsiders()
        {
            var data = new ZoneEffectData { Id = "T-KB", Kind = ZoneEffectKind.Knockback, Radius = 5f, Color = "#ff9be0",
                Lifetime = ZoneLifetimeMode.Permanent, EnemyPushSpeed = 6f };
            var layout = new ZoneLayoutDefinition(ZoneTestData.Layout(new[] { data }, ("T-KB", 1)));
            var player = new FakeZonePlayer { Position = new Vector2(1f, 0f) };
            var enemies = new FakeZoneEnemies().Add(new Vector2(2f, 0f)).Add(new Vector2(-3f, 0f)).Add(new Vector2(30f, 0f));
            using var runtime = new ZoneRuntime(new[] { new ZonePlacement(0, layout.Effects[new Game.Content.ContentId("T-KB")], Vector2.zero) },
                new ZonePlacementRules(layout, 120f, Vector2.zero, null), 1, player, enemies, () => View);
            runtime.Tick(.1f);
            Assert.AreEqual(new Vector2(2.6f, 0f).x, enemies.Positions[0].x, 1e-4f, "Right of the center moves right: 6 units/s for 0.1 s.");
            Assert.AreEqual(-3.6f, enemies.Positions[1].x, 1e-4f, "Left of the center moves left.");
            Assert.AreEqual(30f, enemies.Positions[2].x, "Outside the radius nothing moves.");
            Assert.AreEqual(new Vector2(1f, 0f), player.Position);
            Assert.AreEqual(0, player.Teleports.Count); Assert.IsFalse(player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey));
            Assert.AreEqual(0, runtime.PlayerActiveZoneCount, "The player does not count it as an effect on them.");
        }

        [Test]
        public void Contains_UsesTheMiddleOfTheRimBand_NotItsOuterEdge()
        {
            var data = ZoneTestData.ExperienceField(); data.ActiveRadiusFraction = .7f; data.VerticalScale = .8f;
            var effect = new ZoneEffectDefinition(data);
            Assert.IsTrue(effect.Contains(Vector2.zero, Vector2.right * 6f * .699f));
            Assert.IsFalse(effect.Contains(Vector2.zero, Vector2.right * 6f * .701f), "Touching the outer rim does nothing yet.");
            Assert.IsTrue(effect.Contains(Vector2.zero, Vector2.up * 6f * .8f * .699f), "The ground ellipse keeps its flattening.");
            var zone = new ZonePlacement(0, effect, Vector2.zero, radius: 3f);
            Assert.IsTrue(zone.Contains(Vector2.right * 3f * .699f)); Assert.IsFalse(zone.Contains(Vector2.right * 3f * .701f));
            data.ActiveRadiusFraction = 1.2f; Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data.ActiveRadiusFraction = null; Assert.AreEqual(1f, new ZoneEffectDefinition(data).ActiveRadiusFraction, "Absent keeps the full radius.");
        }

        [Test]
        public void Definition_RejectsInconsistentPortalExperienceAndKnockbackData()
        {
            var portal = ZoneTestData.ScheduledPortal(); portal.PortalMinPairDistance = 30f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(portal), "A fixed minimum contradicts the screen-height distance.");
            portal = ZoneTestData.ScheduledPortal(); portal.RelocatesBetweenCycles = false;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(portal), "A scheduled pair relocates.");
            var fixedPortal = ZoneTestData.Portal(); fixedPortal.RelocatesBetweenCycles = true; fixedPortal.Lifetime = ZoneLifetimeMode.Pulsing;
            fixedPortal.PulsePeriodSeconds = 30f; fixedPortal.PulseVisibleSeconds = 20f; fixedPortal.PulseFadeSeconds = 3f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(fixedPortal), "A fixed pair does not relocate.");
            var experience = ZoneTestData.ExperienceField(); experience.PlayerExperienceMultiplier = 1f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(experience));
            experience = ZoneTestData.ExperienceField(); experience.AffectsBothSides = true;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(experience));
            var carried = ZoneTestData.Haste(); carried.PlayerExperienceMultiplier = 5f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(carried));
            carried = ZoneTestData.Haste(); carried.EnemyPushSpeed = 6f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(carried));
            var knockback = new ZoneEffectData { Id = "T-KB", Kind = ZoneEffectKind.Knockback, Radius = 5f, Color = "#ff9be0",
                Lifetime = ZoneLifetimeMode.Permanent, EnemyPushSpeed = 6f, AffectsBothSides = true };
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(knockback), "The player is never pushed.");
            knockback.AffectsBothSides = false; knockback.EnemyPushSpeed = 0f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(knockback));
            var layout = ZoneTestData.Layout(new[] { ZoneTestData.ScheduledPortal() }, ("T-SCHED-PORTAL", 2));
            Assert.Catch<ArgumentException>(() => new ZoneLayoutDefinition(layout), "A scheduled pair needs a schedule.");
            Assert.Catch<ArgumentException>(() => Layout(1, 1f, null, (ZoneTestData.ExperienceField(), 1), (ZoneTestData.ScheduledPortal(), 2)),
                "A scheduled pair needs its own portal interval.");
            Assert.Catch<ArgumentException>(() => Layout(1, 1f, 5f, (ZoneTestData.ExperienceField(), 1)), "No pair, no portal interval.");
        }
    }
}
