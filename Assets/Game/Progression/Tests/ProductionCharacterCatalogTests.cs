using System.Collections.Generic;
using System.Linq;
using Game.Content;
using NUnit.Framework;

namespace Game.Progression.Tests
{
    /// <summary>
    /// Production roster: CHAR-001 from the approved FIELD-001 baseline (F1-03) and CHAR-002…010 from
    /// characters-v1 (DECISION-0087).
    /// </summary>
    public sealed class ProductionCharacterCatalogTests
    {
        private static CharacterDefinition Character(string id) =>
            ProductionCharacterDefinitionCatalog.CreateDefinitions().Single(c => c.Id.ToString() == id);

        [Test]
        public void Roster_ContainsTenCharactersInCardOrder()
        {
            CollectionAssert.AreEqual(
                Enumerable.Range(1, 10).Select(i => $"CHAR-{i:000}"),
                ProductionCharacterDefinitionCatalog.CreateDefinitions().Select(c => c.Id.ToString()));
        }

        [Test]
        public void Klepka_KeepsStoneStartAndNeutralStats()
        {
            var character = Character("CHAR-001");
            Assert.AreEqual("Клёпка", character.DisplayName);
            Assert.AreEqual("SKILL-001", character.StartingActiveSkill.Id.ToString());
            Assert.AreEqual(100f, character.BaseStats.MaxHealth);
            Assert.AreEqual(3f, character.BaseStats.MovementSpeed);
            Assert.AreEqual(0.5f, character.BaseStats.PickupRadius);
            Assert.AreEqual(1f, character.BaseStats.ActiveSkillDamageMultiplier);
            Assert.AreEqual("CHAR-001-VISUAL-BODY", character.Visual.Id.ToString());
            Assert.AreEqual("CHAR-001-MOTION", character.MotionProfile.Id.ToString());
            // DECISION-0075: Stone Throw specialization, 1.6 × 1.25 = ×2 power without passives.
            Assert.AreEqual(0.6f, character.StartingSkillBoost.ActiveSkillDamageMultiplierBonus, 1e-5f);
            Assert.AreEqual(0.25f, character.StartingSkillBoost.ActionSpeedBonus, 1e-5f);
            Assert.AreEqual(0f, character.StartingSkillBoost.EffectSizeMultiplierBonus);
        }

        [Test]
        public void Klepka_DraftWeightsCoverAllSkills_WithCardDirectionAndNoPassiveBias()
        {
            var character = Character("CHAR-001");
            Assert.AreEqual(16, character.DraftWeights.Count);
            Assert.AreEqual(1.35f, character.GetDraftWeight(new ContentId("SKILL-002")), 1e-5f);
            Assert.AreEqual(1.35f, character.GetDraftWeight(new ContentId("SKILL-005")), 1e-5f);
            Assert.AreEqual(1.35f, character.GetDraftWeight(new ContentId("SKILL-007")), 1e-5f);
            Assert.AreEqual(0.7f, character.GetDraftWeight(new ContentId("SKILL-014")), 1e-5f);
            Assert.AreEqual(1f, character.GetDraftWeight(new ContentId("SKILL-001")), 1e-5f);
            Assert.AreEqual(0.7f, character.GetDraftWeight(new ContentId("SKILL-009")), 1e-5f,
                "Late skills carry their baseline weight; profile access still filters locked ones (DECISION-0060).");
            Assert.AreEqual(1f, character.GetDraftWeight(new ContentId("PASSIVE-004")));
        }

        [Test]
        public void Klepka_PresentationUsesBaselineTextWithoutInventedStatHighlights()
        {
            var character = Character("CHAR-001");
            StringAssert.Contains("Начинает с Броска камня", character.Presentation.Role);
            Assert.AreEqual(0, character.Presentation.Highlights.Count);
            Assert.AreEqual("CHARACTER-BASELINE-001", character.Presentation.Baseline.Id.ToString());
            var baseline = ProductionCharacterDefinitionCatalog.CreateBaseline();
            Assert.AreEqual(character.BaseStats.MaxHealth, baseline.Stats.MaxHealth);
        }

        [TestCase("CHAR-002", "SKILL-003", 150f, 2.55f, 1f, 1.15f)]
        [TestCase("CHAR-003", "SKILL-005", 65f, 3.45f, 1.35f, 1f)]
        [TestCase("CHAR-004", "SKILL-009", 80f, 3.45f, 0.85f, 0.75f)]
        [TestCase("CHAR-005", "SKILL-007", 60f, 2.85f, 1.4f, 1f)]
        [TestCase("CHAR-006", "SKILL-010", 170f, 2.4f, 1f, 1.3f)]
        [TestCase("CHAR-007", "SKILL-006", 70f, 3.9f, 0.85f, 1f)]
        [TestCase("CHAR-008", "SKILL-012", 80f, 2.7f, 1f, 0.85f)]
        [TestCase("CHAR-009", "SKILL-013", 75f, 3f, 1.3f, 0.9f)]
        [TestCase("CHAR-010", "SKILL-015", 130f, 2.7f, 0.9f, 1f)]
        public void LateCharacter_HasApprovedStartAndMainStats(
            string id, string startingSkill, float health, float speed, float damage, float cooldown)
        {
            var character = Character(id);
            Assert.AreEqual(startingSkill, character.StartingActiveSkill.Id.ToString());
            Assert.AreEqual(health, character.BaseStats.MaxHealth, 1e-5f);
            Assert.AreEqual(speed, character.BaseStats.MovementSpeed, 1e-5f);
            Assert.AreEqual(damage, character.BaseStats.ActiveSkillDamageMultiplier, 1e-5f);
            Assert.AreEqual(cooldown, character.BaseStats.ActiveSkillCooldownMultiplier, 1e-5f);
            Assert.AreEqual($"{id}-VISUAL-BODY", character.Visual.Id.ToString());
            Assert.AreEqual($"{id}-VISUAL-PORTRAIT", character.Presentation.Crop.Id.ToString());
            Assert.AreEqual($"{id}-VISUAL-ICON", character.Presentation.Icon.Id.ToString());
            Assert.AreEqual("CHAR-001-MOTION", character.MotionProfile.Id.ToString());
            Assert.Greater(character.Presentation.Highlights.Count, 0, "Changed stats are shown on Character Select.");
            Assert.AreEqual(1f, character.GetDraftWeight(character.StartingActiveSkill.Id),
                "The starting skill is never blocked or boosted.");
        }

        [Test]
        public void LateCharacter_StartingSkillBoostFollowsCard()
        {
            // DECISION-0075 specializations are unchanged by characters-v1.
            Assert.AreEqual(0.25f, Character("CHAR-002").StartingSkillBoost.EffectSizeMultiplierBonus, 1e-5f);
            Assert.AreEqual(0.45f, Character("CHAR-005").StartingSkillBoost.ActiveSkillDamageMultiplierBonus, 1e-5f);
            Assert.AreEqual(0.4f, Character("CHAR-005").StartingSkillBoost.ActionSpeedBonus, 1e-5f);
            Assert.AreEqual(0.25f, Character("CHAR-008").StartingSkillBoost.EffectRangeMultiplierBonus, 1e-5f);
        }

        [TestCase("CHAR-002", new[] { "SKILL-004", "SKILL-006", "SKILL-015", "PASSIVE-001", "PASSIVE-008", "PASSIVE-011" }, new[] { "SKILL-010", "SKILL-012", "PASSIVE-005", "PASSIVE-006" })]
        [TestCase("CHAR-003", new[] { "SKILL-001", "SKILL-012", "SKILL-013", "PASSIVE-003", "PASSIVE-004", "PASSIVE-013" }, new[] { "SKILL-003", "SKILL-009", "PASSIVE-008", "PASSIVE-014" })]
        [TestCase("CHAR-004", new[] { "SKILL-004", "SKILL-006", "SKILL-014", "PASSIVE-003", "PASSIVE-005", "PASSIVE-012" }, new[] { "SKILL-010", "SKILL-012", "PASSIVE-001", "PASSIVE-014" })]
        [TestCase("CHAR-005", new[] { "SKILL-001", "SKILL-008", "SKILL-011", "PASSIVE-005", "PASSIVE-010", "PASSIVE-013" }, new[] { "SKILL-003", "SKILL-009", "PASSIVE-001", "PASSIVE-003" })]
        [TestCase("CHAR-006", new[] { "SKILL-004", "SKILL-014", "SKILL-015", "PASSIVE-001", "PASSIVE-011", "PASSIVE-012" }, new[] { "SKILL-006", "SKILL-008", "PASSIVE-006", "PASSIVE-013" })]
        [TestCase("CHAR-007", new[] { "SKILL-002", "SKILL-008", "SKILL-013", "PASSIVE-002", "PASSIVE-003", "PASSIVE-007" }, new[] { "SKILL-009", "SKILL-010", "PASSIVE-009", "PASSIVE-012" })]
        [TestCase("CHAR-008", new[] { "SKILL-001", "SKILL-005", "SKILL-007", "PASSIVE-004", "PASSIVE-005", "PASSIVE-013" }, new[] { "SKILL-003", "SKILL-009", "PASSIVE-002", "PASSIVE-003" })]
        [TestCase("CHAR-009", new[] { "SKILL-002", "SKILL-005", "SKILL-015", "PASSIVE-004", "PASSIVE-011", "PASSIVE-013" }, new[] { "SKILL-004", "SKILL-010", "PASSIVE-005", "PASSIVE-010" })]
        [TestCase("CHAR-010", new[] { "SKILL-003", "SKILL-004", "SKILL-011", "PASSIVE-001", "PASSIVE-009", "PASSIVE-014" }, new[] { "SKILL-001", "SKILL-012", "PASSIVE-006", "PASSIVE-007" })]
        public void LateCharacter_DraftWeightsBoostThreeAndBlockTwoOfEachKind(string id, string[] boosted, string[] blocked)
        {
            var character = Character(id);
            Assert.AreEqual(16 + 14, character.DraftWeights.Count, "Every skill and passive has an explicit weight.");
            foreach (var pair in character.DraftWeights)
            {
                var entry = pair.Key.ToString();
                var expected = boosted.Contains(entry) ? 1.35f : blocked.Contains(entry) ? 0f : 1f;
                Assert.AreEqual(expected, pair.Value, 1e-5f, $"{id} weight of {entry}");
            }
        }

        [Test]
        public void Blocks_MakeDifferentSetsUnreachablePerCharacter()
        {
            var sets = ProductionSetCatalog.Create();
            var lostSets = new Dictionary<string, string>();
            foreach (var character in ProductionCharacterDefinitionCatalog.CreateDefinitions())
            {
                var lost = sets.Where(set => set.Recipe.Any(component => character.GetDraftWeight(component.Id) <= 0f))
                    .Select(set => set.Id.ToString()).ToArray();
                var reachableOwn = sets.Count(set => !lost.Contains(set.Id.ToString()) &&
                                                     set.Recipe.Any(c => c.Id == character.StartingActiveSkill.Id));
                var id = character.Id.ToString();
                if (id == "CHAR-001")
                {
                    Assert.AreEqual(0, lost.Length, "Klepka can assemble every set.");
                    continue;
                }
                Assert.That(lost.Length, Is.InRange(6, 8), $"{id} unreachable sets");
                Assert.Greater(reachableOwn, 0, $"{id} keeps a set built on the starting skill");
                lostSets.Add(id, string.Join(",", lost));
            }
            Assert.AreEqual(9, lostSets.Values.Distinct().Count(), "Each character loses a different set list.");
            StringAssert.Contains("SET-010", lostSets["CHAR-003"], "Шепотка cannot take Orbital Blades (SKILL-003).");
            StringAssert.DoesNotContain("SET-016", lostSets["CHAR-003"], "Шепотка keeps Giant's Shot on her spear.");
        }
    }
}
