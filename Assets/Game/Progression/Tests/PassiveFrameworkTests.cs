using System.Linq;
using Game.Character;
using Game.Combat;
using NUnit.Framework;

namespace Game.Progression.Tests
{
    public class PassiveFrameworkTests
    {
        [Test]
        public void Preview_ReportsPassiveBonusDeltaWithoutChangingLiveStats()
        {
            var definition = FixturePassiveCatalog.Create()[0];
            var preview = definition.CreateDraftPreview(1, 2);
            Assert.AreEqual(1, preview.CurrentLevel);
            Assert.AreEqual(2, preview.NextLevel);
            var hp = preview.Values[0];
            Assert.AreEqual("Max HP", hp.Label);
            Assert.AreEqual(10f, hp.Current, 0.001f);
            Assert.AreEqual(20f, hp.Next, 0.001f);
            Assert.AreEqual("%", hp.Unit);
        }

        [Test]
        public void PassiveUpgrade_ReplacesKeyedModifierAndPreservesHealthRatio()
        {
            var vitality = FixturePassiveCatalog.Create()[0];
            var stats = new CharacterStats(new CharacterBaseStats(100f, 3f));
            using (var health = new Health(stats))
            {
                stats.SetModifier("passive:vitality", vitality.GetLevel(1));
                Assert.AreEqual(110f, health.MaxHealth, 0.0001f);
                Assert.AreEqual(110f, health.CurrentHealth, 0.0001f);

                health.TakeDamage(55f);
                stats.SetModifier("passive:vitality", vitality.GetLevel(2));

                Assert.AreEqual(120f, health.MaxHealth, 0.0001f);
                Assert.AreEqual(60f, health.CurrentHealth, 0.0001f);
                Assert.AreEqual(1, stats.ModifierCount);
            }
        }

        [Test]
        public void FixtureCatalog_ProvidesSixLevelsAcrossAllStatCategories()
        {
            var catalog = FixturePassiveCatalog.Create();

            Assert.AreEqual(9, catalog.Count);
            foreach (var definition in catalog)
                Assert.AreEqual(BuildEntryDefinition.MaxLevel, definition.Levels.Count);

            var finalHaste = catalog[1].GetLevel(6);
            Assert.Greater(finalHaste.MovementSpeedMultiplierBonus, 0f);
            Assert.Greater(finalHaste.ActiveSkillDamageMultiplierBonus, 0f);
            Assert.Greater(finalHaste.ActionSpeedBonus, 0f);

            var finalMemory = catalog[2].GetLevel(6);
            Assert.Greater(finalMemory.IncomingDamageReductionBonus, 0f);
            Assert.Greater(finalMemory.HealthRestorationMultiplierBonus, 0f);
            Assert.Greater(finalMemory.HealthRegenerationPerSecondBonus, 0f);
            Assert.Greater(finalMemory.DisappearingXpRecoveryBonus, 0f);
            Assert.Greater(finalMemory.PickedUpXpMultiplierBonus, 0f);
            Assert.Greater(finalMemory.XpDropLifetimeBonusSeconds, 0f);
        }
        [Test]
        public void AllFixtureLevels_ReplaceRatherThanAccumulateAndRemoveToNeutral()
        {
            foreach (var definition in FixturePassiveCatalog.Create())
            {
                var stats = new CharacterStats(new CharacterBaseStats(100f, 3f, pickupRadius: 2f));
                for (var level = 1; level <= 6; level++)
                {
                    var modifier = definition.GetLevel(level);
                    stats.SetModifier("passive", modifier);
                    stats.SetModifier("passive", modifier);
                    Assert.AreEqual(1, stats.ModifierCount);
                    Assert.AreEqual(100f * (1f + modifier.MaxHealthMultiplierBonus), stats.MaxHealth, 0.001f);
                    Assert.AreEqual(3f * (1f + modifier.MovementSpeedMultiplierBonus), stats.MovementSpeed, 0.001f);
                    Assert.AreEqual(1f + modifier.ActiveSkillDamageMultiplierBonus, stats.ActiveSkillDamageMultiplier, 0.001f);
                    Assert.AreEqual(1f / (1f + modifier.ActionSpeedBonus), stats.ActiveSkillCooldownMultiplier, 0.001f);
                    Assert.AreEqual(2f * (1f + modifier.PickupRadiusMultiplierBonus), stats.PickupRadius, 0.001f);
                    Assert.AreEqual(1f + modifier.PotionDropMultiplierBonus, stats.PotionDropMultiplier, 0.001f);
                    Assert.AreEqual(1f + modifier.EffectSizeMultiplierBonus, stats.EffectSizeMultiplier, 0.001f);
                    Assert.AreEqual(1f + modifier.EffectRangeMultiplierBonus, stats.EffectRangeMultiplier, 0.001f);
                    Assert.AreEqual(modifier.KnockbackResistanceBonus, stats.KnockbackResistance, 0.001f);
                    Assert.AreEqual(1f + modifier.OutgoingKnockbackBonus, stats.OutgoingKnockbackMultiplier, 0.001f);
                    Assert.AreEqual(modifier.HealthRegenerationPerSecondBonus, stats.HealthRegenerationPerSecond, 0.001f);
                    Assert.AreEqual(modifier.DisappearingXpRecoveryBonus, stats.DisappearingXpRecovery, 0.001f);
                    Assert.AreEqual(1f + modifier.PickedUpXpMultiplierBonus, stats.PickedUpXpMultiplier, 0.001f);
                    Assert.AreEqual(1f + modifier.HealthRestorationMultiplierBonus, stats.HealthRestorationMultiplier, 0.001f);
                    Assert.AreEqual(1f - modifier.IncomingDamageReductionBonus, stats.IncomingDamageMultiplier, 0.001f);
                    Assert.AreEqual(modifier.XpDropLifetimeBonusSeconds, stats.XpDropLifetimeBonusSeconds, 0.001f);
                    stats.UpdateHealthRatio(0.1f);
                    Assert.AreEqual(1f + modifier.LowHealthDamageMaxBonus, stats.LowHealthDamageMultiplier, 0.001f);
                }
                stats.RemoveModifier("passive");
                Assert.AreEqual(0, stats.ModifierCount);
                Assert.AreEqual(100f, stats.MaxHealth);
                Assert.AreEqual(2f, stats.PickupRadius);
                Assert.AreEqual(1f, stats.LowHealthDamageMultiplier);
                Assert.AreEqual(1f, stats.PotionDropMultiplier);
                Assert.AreEqual(1f, stats.EffectSizeMultiplier);
                Assert.AreEqual(1f, stats.EffectRangeMultiplier);
            }
        }

        [Test]
        public void CollectorAndMagnet_ExposeRelativePotionChanceAndLeaveLifetimeUnchanged()
        {
            var catalog = FixturePassiveCatalog.Create();
            var stats = new CharacterStats(new CharacterBaseStats(100f, 3f, pickupRadius: 2f));
            stats.SetModifier("collector", catalog.Single(x => x.Id.ToString() == "FIXTURE-PASSIVE-COLLECTOR").GetLevel(6));
            stats.SetModifier("magnet", catalog.Single(x => x.Id.ToString() == "FIXTURE-PASSIVE-PICKUP-RADIUS").GetLevel(6));
            Assert.AreEqual(0.08f, 0.05f * stats.PotionDropMultiplier, 0.0001f);
            Assert.AreEqual(2f, stats.HealthRegenerationPerSecond);
            Assert.AreEqual(3.2f, stats.PickupRadius, 0.0001f);
            Assert.AreEqual(0f, stats.XpDropLifetimeBonusSeconds);
            Assert.AreEqual(0f, stats.DisappearingXpRecovery);
        }

        [Test]
        public void NewChannelPreviews_ShowFinalCurrentAndNextValues()
        {
            foreach (var definition in FixturePassiveCatalog.Create().Skip(3))
            {
                for (var level = 1; level < 6; level++)
                {
                    var preview = definition.CreateDraftPreview(level, level + 1);
                    Assert.IsNotEmpty(preview.Values);
                    foreach (var value in preview.Values)
                        Assert.Greater(value.Next, value.Current);
                }
            }
        }

    }
}
