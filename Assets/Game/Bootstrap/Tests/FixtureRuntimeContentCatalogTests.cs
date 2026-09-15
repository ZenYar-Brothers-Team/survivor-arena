using Game.ActiveSkill;
using Game.Progression;
using NUnit.Framework;

namespace Game.Bootstrap.Tests
{
    public class FixtureRuntimeContentCatalogTests
    {
        [Test]
        public void Create_BuildsOneValidatedRegistryForEveryRuntimeDefinition()
        {
            var catalog = FixtureRuntimeContentCatalog.Create();

            Assert.IsTrue(catalog.Registry.IsBuilt);
            Assert.Greater(catalog.ActiveSkills.Count, 0);
            Assert.Greater(catalog.Passives.Count, 0);
            Assert.Greater(catalog.Sets.Count, 0);
            Assert.Greater(catalog.Enemies.Count, 0);
            Assert.AreEqual(2, catalog.Characters.AllCharacters.Count);
            Assert.AreEqual(1, catalog.Characters.UnlockedCharacters.Count);
            Assert.AreEqual(catalog.ActiveSkills.Count + catalog.Passives.Count + catalog.Sets.Count, catalog.BuildEntries.Count);

            foreach (var buildEntry in catalog.BuildEntries)
            {
                Assert.AreSame(buildEntry, catalog.Registry.Get<BuildEntryDefinition>(buildEntry.Id));
                if (buildEntry.Kind == BuildEntryKind.ActiveSkill)
                    Assert.IsInstanceOf<ActiveSkillProgressionDefinition>(buildEntry);
                else if (buildEntry.Kind == BuildEntryKind.PassiveItem)
                    Assert.IsInstanceOf<PassiveProgressionDefinition>(buildEntry);
                else
                    Assert.IsInstanceOf<SetDefinition>(buildEntry);
            }

            foreach (var character in catalog.Characters.AllCharacters)
                Assert.AreSame(character, catalog.Registry.Get<CharacterDefinition>(character.Id));
        }
    }
}
