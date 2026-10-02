using System;
using NUnit.Framework;
using UnityEngine;

namespace Game.Zones.Tests
{
    public sealed class ZonePreparationTests
    {
        [Test]
        public void TemporaryPositions_FixedSealsStay_RandomSealsMoveOnlyAtNextCycle()
        {
            var fixedData = ZoneTestData.Haste("T-FIXED", ZoneLifetimeMode.Pulsing);
            fixedData.RelocatesBetweenCycles = false;
            var randomData = ZoneTestData.Haste("T-RANDOM", ZoneLifetimeMode.Pulsing);
            randomData.RelocatesBetweenCycles = true;
            var layout = new ZoneLayoutDefinition(ZoneTestData.Layout(new[] { fixedData, randomData }, ("T-FIXED", 1), ("T-RANDOM", 1)));
            var fixedZone = new ZonePlacement(0, new ZoneEffectDefinition(fixedData), new Vector2(-25f, 0f));
            var randomZone = new ZonePlacement(1, new ZoneEffectDefinition(randomData), new Vector2(25f, 0f));
            using var runtime = new ZoneRuntime(new[] { fixedZone, randomZone }, new ZonePlacementRules(layout, 120f, Vector2.zero, null),
                6, new FakeZonePlayer(), new FakeZoneEnemies(), () => new Rect(-60, -60, 120, 120));
            runtime.Tick(29f);
            Assert.AreEqual(new Vector2(25f, 0f), randomZone.Center);
            runtime.Tick(1f);
            Assert.AreEqual(new Vector2(-25f, 0f), fixedZone.Center);
            Assert.AreNotEqual(new Vector2(25f, 0f), randomZone.Center);
            var center = randomZone.Center; runtime.Tick(5f);
            Assert.AreEqual(center, randomZone.Center, "The seal never moves during preparation or action.");
        }

        private static Game.Zones.Json.ZoneEffectData TemporaryPortal()
        {
            var data = ZoneTestData.Portal();
            data.Lifetime = ZoneLifetimeMode.Pulsing; data.RelocatesBetweenCycles = false;
            data.PulsePeriodSeconds = 30f; data.PulseVisibleSeconds = 20f; data.PulseFadeSeconds = 3f;
            data.PulsePrepareSeconds = 5f; data.PulseIdleVisibility = .14f;
            return data;
        }

        [Test]
        public void TemporaryPortals_ShareOnePhase_RejectRelocation()
        {
            var data = TemporaryPortal();
            var layout = new ZoneLayoutDefinition(ZoneTestData.Layout(new[] { data }, (data.Id, 2)));
            var pair = ZoneLayoutGenerator.Generate(layout, 120f, Vector2.zero, null, 6);
            Assert.AreEqual(pair[0].PhaseSeconds, pair[1].PhaseSeconds);
            Assert.AreEqual(1, pair[0].PartnerIndex); Assert.AreEqual(0, pair[1].PartnerIndex);
            data.RelocatesBetweenCycles = true;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
        }

        [Test]
        public void TemporaryPortals_OnlyTeleportWhenBothEndsFullyLit()
        {
            var data = TemporaryPortal(); var effect = new ZoneEffectDefinition(data);
            var layout = new ZoneLayoutDefinition(ZoneTestData.Layout(new[] { data }, (data.Id, 2)));
            var from = new ZonePlacement(0, effect, new Vector2(-40f, 0f));
            var to = new ZonePlacement(1, effect, new Vector2(40f, 0f));
            from.LinkPartner(1); to.LinkPartner(0);
            var player = new FakeZonePlayer { Position = from.Center };
            using var runtime = new ZoneRuntime(new[] { from, to }, new ZonePlacementRules(layout, 120f, Vector2.zero, null),
                6, player, new FakeZoneEnemies(), () => new Rect(-60, -60, 120, 120));
            runtime.Tick(4.9f); Assert.AreEqual(from.Center, player.Position);
            Assert.IsNull(from.LastApplicationSeconds); Assert.IsNull(to.LastApplicationSeconds);
            runtime.Tick(.1f); Assert.Greater(player.Position.x, 0f);
            Assert.AreEqual(5f, from.LastApplicationSeconds); Assert.AreEqual(5f, to.LastApplicationSeconds);
            player.Position = from.Center; runtime.Tick(12f);
            Assert.AreEqual(from.Center, player.Position, "Decorative fade cannot teleport.");
            Assert.AreEqual(5f, from.LastApplicationSeconds, "An inactive portal cannot trigger another flash.");
            runtime.Tick(13f);
            Assert.AreEqual(new Vector2(-40f, 0f), from.Center);
            Assert.AreEqual(new Vector2(40f, 0f), to.Center);
        }

        [TestCase(6f, 0f, true)]
        [TestCase(0f, 4.79f, true)]
        [TestCase(0f, 4.81f, false)]
        [TestCase(0f, 5.5f, false)]
        [TestCase(6.01f, 0f, false)]
        [TestCase(4f, 3f, true)]
        [TestCase(4f, 4f, false)]
        public void GroundProjection_ContainsOnlyPointsInsideFlattenedBoundary(float x, float y, bool inside)
        {
            var data = ZoneTestData.Haste(); data.Radius = 6f; data.VerticalScale = .8f;
            var effect = new ZoneEffectDefinition(data);
            var center = new Vector2(17f, -11f);
            Assert.AreEqual(inside, effect.Contains(center, center + new Vector2(x, y)));
        }

        [Test]
        public void GroundProjection_DefaultIsCircular_RejectsInvalidScale()
        {
            var data = ZoneTestData.Haste();
            Assert.AreEqual(1f, new ZoneEffectDefinition(data).VerticalScale);
            foreach (var value in new[] { 0f, .09f, 1.01f, float.NaN, float.PositiveInfinity })
            { data.VerticalScale = value; Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data)); }
        }

        [Test]
        public void GroundProjection_PlayerOutsideFlattenedRimReceivesNoBonus()
        {
            var data = ZoneTestData.Haste(); data.Radius = 6f; data.VerticalScale = .8f;
            var layout = new ZoneLayoutDefinition(ZoneTestData.Layout(new[] { data }, ("T-HASTE", 1)));
            var player = new FakeZonePlayer { Position = new Vector2(0f, 5.5f) };
            var zone = new ZonePlacement(0, new ZoneEffectDefinition(data), Vector2.zero);
            using var runtime = new ZoneRuntime(new[] { zone }, new ZonePlacementRules(layout, 120f, Vector2.zero, null),
                1, player, new FakeZoneEnemies(), () => new Rect(-100, -100, 200, 200));
            runtime.Tick(.1f);
            Assert.IsFalse(player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey));
            player.Position = new Vector2(0f, 4.5f); runtime.Tick(.1f);
            Assert.Greater(player.Zone.MovementSpeedMultiplierBonus, 0f);
        }

        private static ZoneEffectDefinition Prepared()
        {
            var data = ZoneTestData.Haste(mode: ZoneLifetimeMode.Pulsing);
            data.PulsePrepareSeconds = 5f; data.PulseIdleVisibility = .14f;
            return new ZoneEffectDefinition(data);
        }

        [TestCase(0f, .14f, false)]
        [TestCase(2.5f, .57f, false)]
        [TestCase(4.999f, 1f, false)]
        [TestCase(5f, 1f, true)]
        [TestCase(16.999f, 1f, true)]
        [TestCase(17f, 1f, false)]
        [TestCase(18.5f, .57f, false)]
        [TestCase(20f, .14f, false)]
        [TestCase(29f, .14f, false)]
        [TestCase(32.5f, .57f, false)]
        public void Preparation_WorksOnlyAfterFullLight_NeverDuringFade(float time, float visibility, bool active)
        {
            var effect = Prepared();
            Assert.AreEqual(visibility, effect.Visibility(0f, time), .0001f);
            Assert.AreEqual(active, effect.IsActive(0f, time));
            Assert.AreEqual(active, new ZonePlacement(0, effect, Vector2.zero).IsActive(time));
        }

        [Test]
        public void Preparation_PhaseAndCurve_AreNonlinearAndRepeatable()
        {
            var effect = Prepared();
            Assert.AreEqual(.14f + .86f * .104f, effect.Visibility(0f, 1f), .0001f);
            Assert.AreEqual(effect.Visibility(0f, 2.5f), effect.Visibility(2.5f, 30f));
            Assert.IsTrue(effect.IsActive(5f, 0f));
            Assert.IsFalse(effect.IsActive(25f, 0f));
        }

        [Test]
        public void Preparation_RejectsIncompleteForeignAndNoActiveWindowData()
        {
            var data = ZoneTestData.Haste(mode: ZoneLifetimeMode.Pulsing);
            data.PulsePrepareSeconds = 5f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data.PulseIdleVisibility = float.NaN;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data.PulseIdleVisibility = .14f; data.PulseVisibleSeconds = 8f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = ZoneTestData.Altar(); data.PulsePrepareSeconds = 5f; data.PulseIdleVisibility = .14f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "The altar packet is outside the academy seal scope.");
        }

        [Test]
        public void Preparation_PlayerGetsNoBonusUntilReady_AndLosesItAtFadeStart()
        {
            var effect = Prepared();
            var player = new FakeZonePlayer { Position = Vector2.zero };
            var layout = new ZoneLayoutDefinition(ZoneTestData.Layout(new[] { ZoneTestData.Haste() }, ("T-HASTE", 1)));
            var zone = new ZonePlacement(0, effect, Vector2.zero);
            using var runtime = new ZoneRuntime(new[] { zone }, new ZonePlacementRules(layout, 120f, Vector2.zero, null),
                1, player, new FakeZoneEnemies(), () => new Rect(-100, -100, 200, 200));
            runtime.Tick(4.5f);
            Assert.IsFalse(player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey));
            runtime.Tick(.5f);
            Assert.AreEqual(.35f, player.Zone.MovementSpeedMultiplierBonus);
            runtime.Tick(12f);
            Assert.IsFalse(player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey), "Fully visible at the boundary but already off; no harm during fading.");
        }
    }
}
