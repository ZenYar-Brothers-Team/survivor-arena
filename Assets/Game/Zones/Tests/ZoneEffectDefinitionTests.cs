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

        [Test]
        public void Definition_ProtectionAndSpeedBurstTakeTheirValues_AndKeepTheirLifetimes()
        {
            Assert.AreEqual(0.8f, new ZoneEffectDefinition(ZoneTestData.Protection()).PlayerIncomingDamageReduction);
            var burst = new ZoneEffectDefinition(ZoneTestData.SpeedBurst());
            Assert.AreEqual(0.6f, burst.PlayerMovementBonus);
            Assert.AreEqual(8f, burst.PlayerBuffSeconds);
            Assert.AreEqual(ZoneLifetimeMode.Burst, burst.Lifetime);
            var data = ZoneTestData.Protection(); data.PlayerIncomingDamageReduction = 0.99f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A ward cannot make the player immune.");
            data = ZoneTestData.Protection(); data.PlayerIncomingDamageReduction = null;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = ZoneTestData.SpeedBurst(); data.PlayerBuffSeconds = null;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = ZoneTestData.SpeedBurst(); data.TelegraphSeconds = 39.8f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "Telegraph and flash must fit the cycle.");
            data = ZoneTestData.SpeedBurst(); data.PulseFadeSeconds = 2f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A burst carries no fade values.");
            data = ZoneTestData.SpeedBurst(); data.Lifetime = ZoneLifetimeMode.Permanent; data.PulsePeriodSeconds = null;
            data.TelegraphSeconds = null; data.FlashSeconds = null;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A speed burst must be a burst.");
            data = ZoneTestData.Haste(); data.Lifetime = ZoneLifetimeMode.Burst; data.PulsePeriodSeconds = 40f;
            data.TelegraphSeconds = 4f; data.FlashSeconds = 1f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "Only the speed burst may use the burst lifetime.");
            data = ZoneTestData.Haste(); data.PlayerBuffSeconds = 5f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A haste zone carries no buff duration.");
        }

        [Test]
        public void EnemyAltars_AreNegativeMirrors_AndRejectMissingOrForeignValues()
        {
            foreach (var kind in new[] { ZoneEffectKind.EnemyHaste, ZoneEffectKind.EnemyRegeneration, ZoneEffectKind.EnemyProtection, ZoneEffectKind.EnemyPower })
            {
                var effect = new ZoneEffectDefinition(ZoneTestData.EnemyAltar(kind, "T-E"));
                Assert.IsTrue(effect.EmpowersEnemies); Assert.IsFalse(effect.HarmsPlayer);
                var positive = ZoneTestData.EnemyAltar(kind, "T-E"); positive.Polarity = ZoneAltarPolarity.Positive;
                Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(positive), "Strengthening enemies is never positive.");
            }
            var missing = ZoneTestData.EnemyAltar(ZoneEffectKind.EnemyPower, "T-E"); missing.EnemyDamageBonus = null;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(missing));
            var foreign = ZoneTestData.Haste(); foreign.EnemyDamageBonus = 0.5f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(foreign), "Enemy-altar values belong only to enemy altars.");
            var carried = ZoneTestData.EnemyAltar(ZoneEffectKind.EnemyHaste, "T-E"); carried.PlayerMovementBonus = 0.5f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(carried));
        }

        [Test]
        public void ExperienceShrine_RequiresBothValues_AndAMultiplierAboveOne()
        {
            var effect = new ZoneEffectDefinition(ZoneTestData.ExperienceShrine());
            Assert.AreEqual(5f, effect.RewardExperienceMultiplier); Assert.AreEqual(30f, effect.RewardExperienceSeconds);
            var noSeconds = ZoneTestData.ExperienceShrine(); noSeconds.RewardExperienceSeconds = null;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(noSeconds));
            var flat = ZoneTestData.ExperienceShrine(); flat.RewardExperienceMultiplier = 1f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(flat));
            var onWrongKind = ZoneTestData.Haste(); onWrongKind.RewardExperienceSeconds = 30f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(onWrongKind));
        }

        [Test]
        public void Definition_Altars_NeedAPolarityThatMatchesWhatTheyDo()
        {
            var data = ZoneTestData.Altar(); data.Polarity = null;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "An altar must say whose side it is on.");
            data = ZoneTestData.Charge(); data.Polarity = null;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            Assert.DoesNotThrow(() => new ZoneEffectDefinition(ZoneTestData.Haste()), "A plain zone needs no polarity.");
            data = ZoneTestData.Rift(); data.Polarity = ZoneAltarPolarity.Positive;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A rift that hurts the player is not positive.");
            data = ZoneTestData.Rift(); data.Polarity = ZoneAltarPolarity.Negative;
            Assert.AreEqual(ZoneAltarPolarity.Negative, new ZoneEffectDefinition(data).Polarity);
            data = ZoneTestData.Rift(); data.EnemyDamagePerSecond = 0f; data.Polarity = ZoneAltarPolarity.Neutral;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "Neutral hurts the enemies too.");
            data = ZoneTestData.Altar(); data.Polarity = ZoneAltarPolarity.Negative;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A haste altar does not harm the player.");
        }

        [Test]
        public void Definition_StrikeAltar_TakesItsValues_AndKeepsItsRules()
        {
            var strike = new ZoneEffectDefinition(ZoneTestData.Strike());
            Assert.AreEqual(2, strike.StrikeCount);
            Assert.AreEqual(20f, strike.PhaseRange, "A permanent strike altar still gets a random starting phase.");
            Assert.IsTrue(strike.HarmsPlayer && strike.HarmsEnemies);
            var data = ZoneTestData.Strike(); data.StrikeRadius = 8f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "Circles must be smaller than the altar.");
            data = ZoneTestData.Strike(); data.StrikePlayerDamage = 0f; data.StrikeEnemyDamage = 0f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A strike must hit someone.");
            data = ZoneTestData.Strike(); data.StrikeTelegraphSeconds = 19.9f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "Warning plus flash must fit the period.");
            data = ZoneTestData.Strike(); data.StrikeCount = null;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = ZoneTestData.Strike(); ZoneTestData.Lifetime(data, ZoneLifetimeMode.Pulsing);
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A strike altar stays in place.");
            data = ZoneTestData.Haste(); data.StrikeCount = 2;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "Strike values belong only to strike altars.");
            data = ZoneTestData.Strike(polarity: ZoneAltarPolarity.Positive);
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A strike that hits the player is not positive.");
            data = ZoneTestData.Strike(polarity: ZoneAltarPolarity.Positive); data.StrikePlayerDamage = 0f;
            Assert.DoesNotThrow(() => new ZoneEffectDefinition(data), "An enemies-only strike is a positive altar.");
        }

        [Test]
        public void Definition_Shrine_NeedsARewardAndPositivePolarity()
        {
            var shrine = new ZoneEffectDefinition(ZoneTestData.Shrine());
            Assert.AreEqual(60f, shrine.ShrineCooldownSeconds);
            Assert.AreEqual(8f, shrine.TimedBuffSeconds);
            Assert.AreEqual(0.5f, shrine.TimedBuffMovementBonus);
            var data = ZoneTestData.Shrine();
            data.RewardBuffSeconds = null; data.RewardMovementBonus = null; data.RewardHealFraction = null;
            data.RewardBlastDamage = null; data.RewardBlastRadius = null; data.RewardShieldSeconds = null; data.RewardIncomingDamageReduction = null;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A shrine without a reward is pointless.");
            data = ZoneTestData.Shrine(); data.RewardBuffSeconds = null;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A buff reward needs its duration.");
            data = ZoneTestData.Shrine(); data.RewardBlastRadius = null;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A blast needs damage and radius together.");
            data = ZoneTestData.Shrine(); data.RewardHealFraction = 1.5f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = ZoneTestData.Shrine(); data.ShrineCooldownSeconds = 0f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = ZoneTestData.Shrine(); data.Polarity = ZoneAltarPolarity.Neutral;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A shrine rewards the player: positive only.");
            data = ZoneTestData.Haste(); data.RewardHealFraction = 0.1f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "Shrine values belong only to shrines.");
        }

        [Test]
        public void Definition_ChargeAndCyclingAltars_TakeTheirValues_AndKeepTheirRules()
        {
            var charge = new ZoneEffectDefinition(ZoneTestData.Charge());
            Assert.AreEqual(20f, charge.ChargeSecondsToMax);
            Assert.AreEqual(1f, charge.ChargeSkillDamageBonus);
            Assert.IsTrue(charge.AlwaysShown, "Charging altars stay drawn.");
            var altar = new ZoneEffectDefinition(ZoneTestData.Altar());
            Assert.AreEqual(ZoneLifetimeMode.Cycling, altar.Lifetime);
            Assert.IsTrue(altar.AlwaysShown, "Resting altars stay faintly drawn.");
            Assert.IsFalse(new ZoneEffectDefinition(ZoneTestData.Haste()).AlwaysShown);
            Assert.AreEqual(1f, altar.Visibility(0f, 10f), 1e-4f);
            Assert.AreEqual(0f, altar.Visibility(0f, 60f), 1e-4f, "Resting for most of the long cycle.");
            Assert.AreEqual(1f, altar.Visibility(0f, 100f), 1e-4f, "On again in the next cycle.");
            var data = ZoneTestData.Charge(); data.ChargeSecondsToMax = 0f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = ZoneTestData.Charge(); data.ChargeDecaySeconds = null;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data));
            data = ZoneTestData.Charge(); data.ChargeSkillDamageBonus = 0f; data.ChargeActionSpeedBonus = 0f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A charge must be worth something.");
            data = ZoneTestData.Charge(mode: ZoneLifetimeMode.Pulsing);
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "A charging altar stays in place.");
            data = ZoneTestData.Haste(); data.ChargeSecondsToMax = 5f;
            Assert.Catch<ArgumentException>(() => new ZoneEffectDefinition(data), "Charge values belong only to the Charge kind.");
        }

        [TestCase(0f, 0f)]
        [TestCase(2f, 0.5f)]
        [TestCase(4f, 1f)]
        [TestCase(4.25f, 1.1f)]
        [TestCase(4.5f, 0f)]
        [TestCase(20f, 0f)]
        [TestCase(41f, 0.25f)]
        public void Burst_RadiusScale_SwellsThenFlashesThenVanishes(float time, float expected)
        {
            var effect = new ZoneEffectDefinition(ZoneTestData.SpeedBurst());
            // At the end of the flash the zone is gone; during it the disc is a little larger than the radius.
            var actual = effect.RadiusScale(0f, time);
            if (time >= 4f && time < 4.5f) Assert.AreEqual(1f + ZoneEffectDefinition.BurstFlashGrowth * ((time - 4f) / 0.5f), actual, 1e-4f);
            else Assert.AreEqual(expected, actual, 1e-4f);
        }

        [Test]
        public void Burst_FiresOncePerCycle_WhenTheTelegraphEnds_AcrossTickBoundariesAndPhases()
        {
            var effect = new ZoneEffectDefinition(ZoneTestData.SpeedBurst());
            Assert.IsFalse(effect.BurstFiresBetween(0f, 0f, 3.99f));
            Assert.IsTrue(effect.BurstFiresBetween(0f, 3.99f, 4.01f), "Goes off at t = 4 s.");
            Assert.IsFalse(effect.BurstFiresBetween(0f, 4.01f, 30f));
            Assert.IsTrue(effect.BurstFiresBetween(0f, 43.9f, 44.1f), "And again at 44 s.");
            Assert.IsTrue(effect.BurstFiresBetween(10f, 33.9f, 34.1f), "A phase of 10 s shifts it to 34 s.");
            var fires = 0;
            for (var t = 0f; t < 200f; t += 0.37f) if (effect.BurstFiresBetween(7f, t, t + 0.37f)) fires++;
            Assert.AreEqual(5, fires, "Five 40 s cycles in 200 s, however the ticks fall.");
            Assert.IsFalse(new ZoneEffectDefinition(ZoneTestData.Haste()).BurstFiresBetween(0f, 0f, 100f), "Only burst zones go off.");
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
