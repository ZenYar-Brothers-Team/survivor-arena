using System;
using Game.Zones.Json;
using NUnit.Framework;

namespace Game.Zones.Tests
{
    public sealed class ZoneEffectDefinitionTests
    {
        [Test]
        public void Definition_EachKindTakesItsOwnValues()
        {
            Assert.AreEqual(-0.4f, new ZoneEffectDefinition(ZoneTestData.Slow()).PlayerMovementBonus);
            Assert.AreEqual(0.35f, new ZoneEffectDefinition(ZoneTestData.Haste()).PlayerMovementBonus);
            Assert.AreEqual(4f, new ZoneEffectDefinition(ZoneTestData.Regeneration()).PlayerRegenerationPerSecond);
            Assert.AreEqual(0.25f, new ZoneEffectDefinition(ZoneTestData.Arcane()).PlayerActionSpeedBonus);
            Assert.AreEqual(20f, new ZoneEffectDefinition(ZoneTestData.Rift()).EnemyDamagePerSecond);
            Assert.AreEqual(2f, new ZoneEffectDefinition(ZoneTestData.Portal()).PortalExitDistance);
        }

        [Test]
        public void Definition_RejectsMissingWrongSignedAndForeignValues()
        {
            var data = ZoneTestData.Slow(); data.PlayerMovementBonus = 0.2f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A slow cannot speed the player up.");
            data = ZoneTestData.Slow(); data.PlayerMovementBonus = 0f; data.EnemySlowFraction = 0f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A slow must slow someone.");
            data = ZoneTestData.Haste(); data.PlayerMovementBonus = -0.1f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = ZoneTestData.Haste(); data.PlayerRegenerationPerSecond = 2f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A haste zone carries no regeneration.");
            data = ZoneTestData.Rift(); data.PlayerDamagePerSecond = 0f; data.EnemyDamagePerSecond = 0f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = ZoneTestData.Portal(); data.PortalCooldownSeconds = null;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = ZoneTestData.Regeneration(); data.Color = "not a color";
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = ZoneTestData.Regeneration(); data.Radius = 0f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = ZoneTestData.Regeneration(); data.Lifetime = null;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "Lifetime is required.");
        }

        [Test]
        public void Definition_PulsingNeedsAConsistentCycle_PermanentCarriesNone_PortalStaysPermanent()
        {
            var data = ZoneTestData.Haste(mode: ZoneLifetimeMode.Pulsing); data.PulseVisibleSeconds = 31f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "Shown longer than the period.");
            data = ZoneTestData.Haste(mode: ZoneLifetimeMode.Pulsing); data.PulseVisibleSeconds = 5f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "Shorter than both fades.");
            data = ZoneTestData.Haste(mode: ZoneLifetimeMode.Pulsing); data.PulseFadeSeconds = null;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = ZoneTestData.Haste(); data.PulsePeriodSeconds = 30f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A permanent zone has no pulse values.");
            data = ZoneTestData.Portal(); ZoneTestData.Lifetime(data, ZoneLifetimeMode.Pulsing);
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A portal pair cannot relocate together.");
        }

        [TestCase(0f, 0f)]
        [TestCase(1.5f, 0.5f)]
        [TestCase(3f, 1f)]
        [TestCase(10f, 1f)]
        [TestCase(17f, 1f)]
        [TestCase(18.5f, 0.5f)]
        [TestCase(20f, 0f)]
        [TestCase(29.9f, 0f)]
        [TestCase(31.5f, 0.5f)]
        public void Visibility_FadesInStaysFadesOutAndRepeats(float time, float expected)
        {
            var effect = new ZoneEffectDefinition(ZoneTestData.Haste(mode: ZoneLifetimeMode.Pulsing));
            Assert.AreEqual(expected, effect.Visibility(0f, time), 1e-4f);
        }

        [Test]
        public void Visibility_PhaseShiftsTheCycle_AndPermanentIsAlwaysFull()
        {
            var pulsing = new ZoneEffectDefinition(ZoneTestData.Haste(mode: ZoneLifetimeMode.Pulsing));
            Assert.AreEqual(1f, pulsing.Visibility(10f, 0f), 1e-4f, "10 s into its cycle at run start.");
            Assert.AreEqual(0f, pulsing.Visibility(25f, 0f), 1e-4f, "Hidden at run start.");
            var permanent = new ZoneEffectDefinition(ZoneTestData.Haste());
            Assert.AreEqual(1f, permanent.Visibility(7f, 123f));
        }
    }
}
