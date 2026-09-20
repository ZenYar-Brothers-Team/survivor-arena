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

            Assert.AreEqual(3, catalog.Count);
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
    }
}
