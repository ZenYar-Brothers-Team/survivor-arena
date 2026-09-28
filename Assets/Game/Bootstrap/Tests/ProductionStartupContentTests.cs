using System.Collections.Generic;
using System.Linq;
using Game.ActiveSkill;
using Game.Content;
using Game.Meta;
using Game.Presentation;
using Game.Progression;
using NUnit.Framework;

namespace Game.Bootstrap.Tests
{
    /// <summary>F1-03: production startup content resolves together and a new profile exposes exactly it.</summary>
    public sealed class ProductionStartupContentTests
    {
        [Test]
        public void StartupDefinitions_ResolveInOneRegistry_WithApprovedVisualsAndProfileAccess()
        {
            var skills = ProductionActiveSkillCatalog.Create();
            var passives = ProductionPassiveCatalog.Create();
            var characters = ProductionCharacterDefinitionCatalog.CreateDefinitions();
            var definitions = new List<IContentDefinition>();
            definitions.AddRange(skills);
            definitions.AddRange(passives);
            definitions.AddRange(characters);
            definitions.Add(ProductionCharacterDefinitionCatalog.CreateBaseline());
            definitions.AddRange(FixtureSpriteMotionProfileCatalog.Create());
            var visuals = definitions.OfType<IReferencesContent>().SelectMany(d => d.GetReferencedContent())
                .Where(r => r.ExpectedType == typeof(SpriteDefinition)).Select(r => r.Id);
            definitions.AddRange(FixtureSpriteCatalog.CreateFor(visuals));
            var registry = ContentRegistry.BuildFrom(definitions);

            var klepka = characters.Single(c => c.Id.ToString() == "CHAR-001");
            Assert.AreEqual("SKILL-001", klepka.ResolveStartingActiveSkill(registry).Id.ToString());
            // DECISION-0087: every character's skill and passive weights resolve against production entries.
            foreach (var character in characters)
            {
                character.ResolveStartingActiveSkill(registry);
                character.ValidateDraftSkillReferences(registry);
            }

            var profile = new ProfileService(MetaCatalog.Load(), new MemoryProfileStore());
            profile.LoadAsync().GetAwaiter().GetResult();
            var roster = ProductionCharacterDefinitionCatalog.Create(new ProfileAccessProvider(profile));
            CollectionAssert.AreEqual(new[] { "CHAR-001" }, roster.UnlockedCharacters.Select(c => c.Id.ToString()));
            // DECISION-0050: a new profile opens exactly the startup ten + ten; late IDs (DECISION-0060) stay locked.
            var startupSkills = new[] { "SKILL-001", "SKILL-002", "SKILL-003", "SKILL-004", "SKILL-005",
                "SKILL-006", "SKILL-007", "SKILL-010", "SKILL-013", "SKILL-014" };
            var startupPassives = new[] { "PASSIVE-001", "PASSIVE-002", "PASSIVE-003", "PASSIVE-004", "PASSIVE-005",
                "PASSIVE-007", "PASSIVE-008", "PASSIVE-009", "PASSIVE-011", "PASSIVE-012" };
            CollectionAssert.AreEqual(startupSkills, skills.Select(s => s.Id.ToString()).Where(profile.IsUnlocked));
            CollectionAssert.AreEqual(startupPassives, passives.Select(p => p.Id.ToString()).Where(profile.IsUnlocked));
        }
    }
}
