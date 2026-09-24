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

            var klepka = characters.Single();
            Assert.AreEqual("SKILL-001", klepka.ResolveStartingActiveSkill(registry).Id.ToString());
            klepka.ValidateDraftSkillReferences(registry);

            var profile = new ProfileService(MetaCatalog.Load(), new MemoryProfileStore());
            profile.LoadAsync().GetAwaiter().GetResult();
            var roster = ProductionCharacterDefinitionCatalog.Create(new ProfileAccessProvider(profile));
            CollectionAssert.AreEqual(new[] { "CHAR-001" }, roster.UnlockedCharacters.Select(c => c.Id.ToString()));
            Assert.IsTrue(skills.All(s => profile.IsUnlocked(s.Id.ToString())), "Every production skill is in the new-profile pool.");
            Assert.IsTrue(passives.All(p => profile.IsUnlocked(p.Id.ToString())), "Every production passive is in the new-profile pool.");
        }
    }
}
