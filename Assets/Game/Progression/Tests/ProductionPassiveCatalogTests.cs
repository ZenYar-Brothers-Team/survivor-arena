using System.Linq;
using Game.Character;
using NUnit.Framework;

namespace Game.Progression.Tests
{
    /// <summary>F1-02: production PASSIVE-001…014 levels and their stat channels (late four: DECISION-0060).</summary>
    public sealed class ProductionPassiveCatalogTests
    {
        private static PassiveProgressionDefinition Passive(string id) =>
            ProductionPassiveCatalog.Create().Single(p => p.Id.ToString() == id);

        private static CharacterStats StatsWith(string id, int level)
        {
            var stats = new CharacterStats(new CharacterBaseStats(100f, 3f, pickupRadius: 0.5f));
            stats.SetModifier(id, Passive(id).GetLevel(level));
            return stats;
        }

        [Test]
        public void Catalog_ContainsAllFourteenPassives_WithSixLevelsAndIcons()
        {
            var passives = ProductionPassiveCatalog.Create();
            CollectionAssert.AreEqual(Enumerable.Range(1, 14).Select(n => $"PASSIVE-{n:000}"), passives.Select(p => p.Id.ToString()));
            foreach (var passive in passives)
            {
                Assert.AreEqual(6, passive.Levels.Count);
                Assert.AreEqual(passive.Id + "-VISUAL-ICON", passive.Icon.Id.ToString());
                Assert.IsFalse(string.IsNullOrWhiteSpace(passive.DisplayName));
            }
        }

        [Test]
        public void LevelSix_MapsEveryCardChannel()
        {
            Assert.AreEqual(150f, StatsWith("PASSIVE-001", 6).MaxHealth, 1e-3f);
            var gatherer = StatsWith("PASSIVE-002", 6);
            Assert.AreEqual(2f, gatherer.HealthRegenerationPerSecond, 1e-4f);
            Assert.AreEqual(1.6f, gatherer.PotionDropMultiplier, 1e-4f);
            Assert.AreEqual(3.9f, StatsWith("PASSIVE-003", 6).MovementSpeed, 1e-4f);
            Assert.AreEqual(1.5f, StatsWith("PASSIVE-004", 6).ActiveSkillDamageMultiplier, 1e-4f);
            var metronome = StatsWith("PASSIVE-005", 6);
            Assert.AreEqual(0.25f, metronome.ActionSpeedBonus, 1e-4f);
            Assert.AreEqual(1f / 1.25f, metronome.ActiveSkillCooldownMultiplier, 1e-4f);
            // DECISION-0059: L6 is 1.5 times smaller than the DECISION-0055 value (2.5 units); L1-L2 unchanged.
            Assert.AreEqual(1.665f, StatsWith("PASSIVE-007", 6).PickupRadius, 1e-4f);
            Assert.AreEqual(0.95f, StatsWith("PASSIVE-007", 2).PickupRadius, 1e-4f);
            Assert.AreEqual(1.1f, StatsWith("PASSIVE-007", 3).PickupRadius, 1e-4f);
            Assert.AreEqual(0.7f, StatsWith("PASSIVE-007", 1).PickupRadius, 1e-4f);
            Assert.AreEqual(0.75f, StatsWith("PASSIVE-008", 6).IncomingDamageMultiplier, 1e-4f);
            Assert.AreEqual(1.6f, StatsWith("PASSIVE-009", 6).HealthRestorationMultiplier, 1e-4f);
            var belt = StatsWith("PASSIVE-011", 6);
            Assert.AreEqual(0.6f, belt.KnockbackResistance, 1e-4f);
            Assert.AreEqual(1.6f, belt.OutgoingKnockbackMultiplier, 1e-4f);
            Assert.AreEqual(1.3f, StatsWith("PASSIVE-012", 6).EffectSizeMultiplier, 1e-4f);
            Assert.AreEqual(1f, StatsWith("PASSIVE-012", 6).EffectRangeMultiplier, 1e-4f, "Size never grows range.");
        }

        [Test]
        public void LevelUp_ReplacesPreviousValue_AndMaxHealthKeepsRatio()
        {
            var stats = new CharacterStats(new CharacterBaseStats(100f, 3f));
            stats.SetModifier("PASSIVE-008", Passive("PASSIVE-008").GetLevel(3));
            stats.SetModifier("PASSIVE-008", Passive("PASSIVE-008").GetLevel(6));
            Assert.AreEqual(0.75f, stats.IncomingDamageMultiplier, 1e-4f, "Levels replace, not stack.");

            var heart = new CharacterStats(new CharacterBaseStats(100f, 3f));
            heart.SetModifier("PASSIVE-001", Passive("PASSIVE-001").GetLevel(1));
            Assert.AreEqual(108f, heart.MaxHealth, 1e-3f);
            heart.SetModifier("PASSIVE-001", Passive("PASSIVE-001").GetLevel(2));
            Assert.AreEqual(116f, heart.MaxHealth, 1e-3f);
        }

        [Test]
        public void CardProgressions_MatchContentDesignSteps()
        {
            CollectionAssert.AreEqual(new[] { 0.05f, 0.09f, 0.13f, 0.17f, 0.21f, 0.25f },
                Passive("PASSIVE-005").Levels.Select(l => l.ActionSpeedBonus).ToArray());
            CollectionAssert.AreEqual(new[] { 0.2f, 0.4f, 0.7f, 1f, 1.4f, 2f },
                Passive("PASSIVE-002").Levels.Select(l => l.HealthRegenerationPerSecondBonus).ToArray());
            CollectionAssert.AreEqual(new[] { 0.08f, 0.16f, 0.24f, 0.32f, 0.4f, 0.5f },
                Passive("PASSIVE-004").Levels.Select(l => l.ActiveSkillDamageMultiplierBonus).ToArray());
        }

        [Test]
        public void LatePassives_MapCardValuesToTheirSingleChannel()
        {
            CollectionAssert.AreEqual(new[] { .1f, .2f, .3f, .4f, .5f, .6f },
                Passive("PASSIVE-006").Levels.Select(l => l.DisappearingXpRecoveryBonus).ToArray());
            CollectionAssert.AreEqual(new[] { .05f, .1f, .15f, .2f, .25f, .3f },
                Passive("PASSIVE-010").Levels.Select(l => l.PickedUpXpMultiplierBonus).ToArray());
            CollectionAssert.AreEqual(new[] { .08f, .16f, .24f, .32f, .4f, .5f },
                Passive("PASSIVE-013").Levels.Select(l => l.EffectRangeMultiplierBonus).ToArray());
            CollectionAssert.AreEqual(new[] { .15f, .25f, .35f, .45f, .55f, .7f },
                Passive("PASSIVE-014").Levels.Select(l => l.LowHealthDamageMaxBonus).ToArray());
            Assert.AreEqual(0.6f, StatsWith("PASSIVE-006", 6).DisappearingXpRecovery, 1e-4f);
            Assert.AreEqual(1.3f, StatsWith("PASSIVE-010", 6).PickedUpXpMultiplier, 1e-4f);
            var reach = StatsWith("PASSIVE-013", 6);
            Assert.AreEqual(1.5f, reach.EffectRangeMultiplier, 1e-4f);
            Assert.AreEqual(1f, reach.EffectSizeMultiplier, 1e-4f, "Range never grows size.");
        }

        [Test]
        public void Stubbornness_ScalesLinearlyWithMissingHealth_AndCapsAtTenPercent()
        {
            var stats = StatsWith("PASSIVE-014", 6);
            Assert.AreEqual(1f, stats.LowHealthDamageMultiplier, 1e-4f, "Full HP gives no bonus.");
            stats.UpdateHealthRatio(0.55f);
            Assert.AreEqual(1.35f, stats.LowHealthDamageMultiplier, 1e-4f, "IP-09 worked example: 55% HP -> half of +70%.");
            stats.UpdateHealthRatio(0.1f);
            Assert.AreEqual(1.7f, stats.LowHealthDamageMultiplier, 1e-4f);
            stats.UpdateHealthRatio(0.02f);
            Assert.AreEqual(1.7f, stats.LowHealthDamageMultiplier, 1e-4f);
        }
    }
}
