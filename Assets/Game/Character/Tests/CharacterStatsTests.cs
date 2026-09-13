using System;
using NUnit.Framework;

namespace Game.Character.Tests
{
    public class CharacterStatsTests
    {
        [Test]
        public void BaseStats_AreExposedWithoutModifiers()
        {
            var stats = CreateStats();

            Assert.AreEqual(100f, stats.MaxHealth);
            Assert.AreEqual(3f, stats.MovementSpeed);
            Assert.AreEqual(1f, stats.ActiveSkillDamageMultiplier);
            Assert.AreEqual(1f, stats.ActiveSkillCooldownMultiplier);
            Assert.AreEqual(1f, stats.IncomingDamageMultiplier);
            Assert.AreEqual(1f, stats.HealthRestorationMultiplier);
            Assert.AreEqual(0f, stats.HealthRegenerationPerSecond);
            Assert.AreEqual(0.1f, stats.DisappearingXpRecovery);
        }

        [Test]
        public void SettingSameSource_ReplacesModifierWithoutDoubleCounting()
        {
            var stats = CreateStats();

            stats.SetModifier("fixture", new CharacterStatModifier(
                maxHealthMultiplierBonus: 0.5f,
                movementSpeedMultiplierBonus: 0.5f,
                activeSkillDamageMultiplierBonus: 0.2f,
                activeSkillCooldownMultiplierBonus: -0.1f));
            stats.SetModifier("fixture", new CharacterStatModifier(
                maxHealthMultiplierBonus: 0.25f,
                movementSpeedMultiplierBonus: 0.25f,
                activeSkillDamageMultiplierBonus: 0.1f,
                activeSkillCooldownMultiplierBonus: -0.2f));

            Assert.AreEqual(1, stats.ModifierCount);
            Assert.AreEqual(125f, stats.MaxHealth, 0.0001f);
            Assert.AreEqual(3.75f, stats.MovementSpeed, 0.0001f);
            Assert.AreEqual(1.1f, stats.ActiveSkillDamageMultiplier, 0.0001f);
            Assert.AreEqual(0.8f, stats.ActiveSkillCooldownMultiplier, 0.0001f);
        }

        [Test]
        public void DifferentSources_AreSummedFromBase()
        {
            var stats = CreateStats();

            stats.SetModifier("one", new CharacterStatModifier(
                movementSpeedMultiplierBonus: 0.1f,
                healthRegenerationPerSecondBonus: 1f,
                disappearingXpRecoveryBonus: 0.2f));
            stats.SetModifier("two", new CharacterStatModifier(
                movementSpeedMultiplierBonus: 0.2f,
                healthRegenerationPerSecondBonus: 2f,
                disappearingXpRecoveryBonus: 0.3f));

            Assert.AreEqual(3.9f, stats.MovementSpeed, 0.0001f);
            Assert.AreEqual(3f, stats.HealthRegenerationPerSecond, 0.0001f);
            Assert.AreEqual(0.6f, stats.DisappearingXpRecovery, 0.0001f);
        }

        [Test]
        public void RemovingModifier_RecomputesFromBase()
        {
            var stats = CreateStats();
            stats.SetModifier("fixture", new CharacterStatModifier(movementSpeedMultiplierBonus: 0.5f));

            var removed = stats.RemoveModifier("fixture");

            Assert.IsTrue(removed);
            Assert.AreEqual(3f, stats.MovementSpeed, 0.0001f);
            Assert.AreEqual(0, stats.ModifierCount);
        }

        [Test]
        public void RecoveryAndMultipliers_AreClampedToValidRanges()
        {
            var stats = CreateStats();

            stats.SetModifier("fixture", new CharacterStatModifier(
                movementSpeedMultiplierBonus: -2f,
                incomingDamageMultiplierBonus: -2f,
                disappearingXpRecoveryBonus: 2f));

            Assert.AreEqual(0f, stats.MovementSpeed);
            Assert.AreEqual(0f, stats.IncomingDamageMultiplier);
            Assert.AreEqual(1f, stats.DisappearingXpRecovery);
        }

        [Test]
        public void EmptyModifierSource_IsRejected()
        {
            var stats = CreateStats();

            Assert.Throws<ArgumentException>(() => stats.SetModifier(" ", default));
        }

        private static CharacterStats CreateStats()
        {
            return new CharacterStats(new CharacterBaseStats(
                maxHealth: 100f,
                movementSpeed: 3f,
                disappearingXpRecovery: 0.1f));
        }
    }
}
