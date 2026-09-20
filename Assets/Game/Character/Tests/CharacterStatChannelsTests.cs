using System;
using Game.Combat;
using NUnit.Framework;

namespace Game.Character.Tests
{
    public sealed class CharacterStatChannelsTests
    {
        [Test]
        public void NewChannels_StackReplaceAndRemoveWithoutDoubleCounting()
        {
            var stats = new CharacterStats(new CharacterBaseStats(100f, 3f, pickupRadius: 2f));
            var modifier = new CharacterStatModifier(knockbackResistanceBonus: 0.6f,
                outgoingKnockbackBonus: 0.2f, pickupRadiusMultiplierBonus: 0.1f,
                effectSizeMultiplierBonus: 0.15f, effectRangeMultiplierBonus: 0.25f,
                potionDropMultiplierBonus: 0.3f, actionSpeedBonus: 0.2f);
            stats.SetModifier("first", modifier);
            stats.SetModifier("first", modifier);
            stats.SetModifier("second", modifier);
            Assert.AreEqual(1f, stats.KnockbackResistance);
            Assert.AreEqual(1.4f, stats.OutgoingKnockbackMultiplier, 0.0001f);
            Assert.AreEqual(2.4f, stats.PickupRadius, 0.0001f);
            Assert.AreEqual(1.3f, stats.EffectSizeMultiplier, 0.0001f);
            Assert.AreEqual(1.5f, stats.EffectRangeMultiplier, 0.0001f);
            Assert.AreEqual(1.6f, stats.PotionDropMultiplier, 0.0001f);
            Assert.AreEqual(0.08f, 0.05f * stats.PotionDropMultiplier, 0.0001f);
            Assert.AreEqual(1f / 1.4f, stats.ActiveSkillCooldownMultiplier, 0.0001f);
            stats.RemoveModifier("first");
            Assert.AreEqual(0.6f, stats.KnockbackResistance);
            stats.RemoveModifier("second");
            Assert.AreEqual(2f, stats.PickupRadius);
            Assert.AreEqual(1f, stats.OutgoingKnockbackMultiplier);
        }

        [TestCase(100f, 1f)]
        [TestCase(55f, 1.35f)]
        [TestCase(10f, 1.7f)]
        [TestCase(5f, 1.7f)]
        public void LowHealthCurve_UsesApprovedBoundaries(float hp, float expected)
        {
            var stats = new CharacterStats(new CharacterBaseStats(100f, 3f));
            stats.SetModifier("stubborn", new CharacterStatModifier(lowHealthDamageMaxBonus: 0.7f));
            using (var health = new Health(stats))
            using (var binding = new CharacterHealthStatBinding(health, stats))
            {
                health.TakeDamage(100f - hp);
                Assert.AreEqual(expected, stats.LowHealthDamageMultiplier, 0.0001f);
                Assert.AreEqual(1f, stats.ActiveSkillDamageMultiplier, "Hit-time applicability belongs to IP-05.");
            }
        }

        [Test]
        public void HealingAndMaxHealthRescale_UpdateCurveWithoutRecursiveLoop()
        {
            var stats = new CharacterStats(new CharacterBaseStats(100f, 3f));
            using (var health = new Health(stats))
            using (var binding = new CharacterHealthStatBinding(health, stats))
            {
                stats.SetModifier("low", new CharacterStatModifier(lowHealthDamageMaxBonus: 0.7f));
                var changed = 0;
                stats.Changed += () => { if (++changed > 10) Assert.Fail("Recursive stats loop"); };
                health.TakeDamage(90f);
                health.Heal(45f);
                Assert.AreEqual(1.35f, stats.LowHealthDamageMultiplier, 0.0001f);
                stats.SetModifier("hp", new CharacterStatModifier(maxHealthMultiplierBonus: 1f));
                Assert.AreEqual(110f, health.CurrentHealth, 0.0001f);
                Assert.AreEqual(1.35f, stats.LowHealthDamageMultiplier, 0.0001f);
                health.Heal(200f);
                Assert.AreEqual(1f, stats.LowHealthDamageMultiplier);
                Assert.LessOrEqual(changed, 5);
                binding.Dispose();
                health.TakeDamage(180f);
                Assert.AreEqual(1f, stats.LowHealthDamageMultiplier, "Detached binding must not mutate stats.");
            }
        }

        [Test]
        public void OverflowingComposition_IsRejectedWithoutChangingLiveState()
        {
            var stats = new CharacterStats(new CharacterBaseStats(100f, 3f, effectRangeMultiplier: 2f));
            Assert.Throws<ArgumentOutOfRangeException>(() => stats.SetModifier("overflow",
                new CharacterStatModifier(effectRangeMultiplierBonus: float.MaxValue)));
            Assert.AreEqual(0, stats.ModifierCount);
            Assert.AreEqual(2f, stats.EffectRangeMultiplier);
            stats.SetModifier("valid", new CharacterStatModifier(effectRangeMultiplierBonus: 0.5f));
            Assert.AreEqual(3f, stats.EffectRangeMultiplier);
        }

        [Test]
        public void InvalidChannels_RejectNonFiniteAndOutOfRangeInputs()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterBaseStats(100f, 3f, knockbackResistance: 1.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterStatModifier(effectSizeMultiplierBonus: float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterStatModifier(lowHealthDamageMaxBonus: float.PositiveInfinity));
            var stats = new CharacterStats(new CharacterBaseStats(100f, 3f));
            Assert.Throws<ArgumentOutOfRangeException>(() => stats.UpdateHealthRatio(-0.1f));
        }
    }
}
