using System.Linq;
using Game.Content;
using NUnit.Framework;

namespace Game.Progression.Tests
{
    /// <summary>F1-03: production CHAR-001 generated from the approved FIELD-001 baseline.</summary>
    public sealed class ProductionCharacterCatalogTests
    {
        [Test]
        public void Roster_ContainsOnlyKlepka_WithStoneStartAndNeutralStats()
        {
            var character = ProductionCharacterDefinitionCatalog.CreateDefinitions().Single();
            Assert.AreEqual("CHAR-001", character.Id.ToString());
            Assert.AreEqual("Клёпка", character.DisplayName);
            Assert.AreEqual("SKILL-001", character.StartingActiveSkill.Id.ToString());
            Assert.AreEqual(100f, character.BaseStats.MaxHealth);
            Assert.AreEqual(3f, character.BaseStats.MovementSpeed);
            Assert.AreEqual(0.5f, character.BaseStats.PickupRadius);
            Assert.AreEqual(1f, character.BaseStats.ActiveSkillDamageMultiplier);
            Assert.AreEqual("CHAR-001-VISUAL-BODY", character.Visual.Id.ToString());
            Assert.AreEqual("CHAR-001-MOTION", character.MotionProfile.Id.ToString());
        }

        [Test]
        public void DraftWeights_CoverOnlyStartupSkills_WithCardDirection()
        {
            var character = ProductionCharacterDefinitionCatalog.CreateDefinitions().Single();
            Assert.AreEqual(10, character.DraftWeights.Count);
            Assert.AreEqual(1.35f, character.GetDraftWeight(new ContentId("SKILL-002")), 1e-5f);
            Assert.AreEqual(1.35f, character.GetDraftWeight(new ContentId("SKILL-005")), 1e-5f);
            Assert.AreEqual(1.35f, character.GetDraftWeight(new ContentId("SKILL-007")), 1e-5f);
            Assert.AreEqual(0.7f, character.GetDraftWeight(new ContentId("SKILL-014")), 1e-5f);
            Assert.AreEqual(1f, character.GetDraftWeight(new ContentId("SKILL-001")), 1e-5f);
            Assert.IsFalse(character.DraftWeights.ContainsKey(new ContentId("SKILL-009")),
                "Late skills are not production definitions in FIELD-001; their weights stay baseline metadata.");
        }

        [Test]
        public void Presentation_UsesBaselineTextWithoutInventedStatHighlights()
        {
            var character = ProductionCharacterDefinitionCatalog.CreateDefinitions().Single();
            StringAssert.Contains("Начинает с Броска камня", character.Presentation.Role);
            Assert.AreEqual(0, character.Presentation.Highlights.Count);
            Assert.AreEqual("CHARACTER-BASELINE-001", character.Presentation.Baseline.Id.ToString());
            var baseline = ProductionCharacterDefinitionCatalog.CreateBaseline();
            Assert.AreEqual(character.BaseStats.MaxHealth, baseline.Stats.MaxHealth);
        }
    }
}
