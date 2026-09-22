using Game.ActiveSkill;
using Game.Enemy;
using Game.Presentation;
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
            Assert.AreEqual(2, catalog.Pickups.Definitions.Count);
            Assert.AreSame(catalog.Pickups.Potion, catalog.Registry.Get<Game.Pickup.PickupDefinition>(catalog.Pickups.Potion.Id));
            Assert.IsTrue(catalog.SourceSnapshot.ContainsKey("Content/Pickups/FixturePickups"));
            Assert.Greater(catalog.ActiveSkills.Count, 0);
            Assert.Greater(catalog.Passives.Count, 0);
            Assert.Greater(catalog.Sets.Count, 0);
            Assert.Greater(catalog.Enemies.Count, 0);
            Assert.AreEqual(2, catalog.Bosses.Count);
            Assert.IsTrue(catalog.SourceSnapshot.ContainsKey("Content/Bosses/FixtureBosses"));
            foreach (var boss in catalog.Bosses)
                Assert.AreSame(boss, catalog.Registry.Get<BossEncounterDefinition>(boss.Id));
            Assert.AreEqual(2, catalog.Characters.AllCharacters.Count);
            Assert.AreEqual(1, catalog.Characters.UnlockedCharacters.Count);
            Assert.AreEqual(2, catalog.SpriteMotionProfiles.Count);
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

            Assert.AreSame(catalog.WaveTimeline, catalog.Registry.Get<WaveTimelineDefinition>(catalog.WaveTimeline.Id));
            foreach (var reference in catalog.WaveTimeline.GetReferencedContent())
                Assert.IsInstanceOf<EnemyDefinition>(catalog.Registry.Get<EnemyDefinition>(reference.Id));

            Assert.IsTrue(
                catalog.Characters.TrySelect(catalog.RunSetup.StartingCharacterId, out _),
                "The configured starting character must be an unlocked roster entry.");

            var agile = catalog.Characters.AllCharacters[0];
            var agileVisual = agile.Visual.Resolve(catalog.Registry);
            Assert.IsInstanceOf<SpriteDefinition>(agileVisual);
            Assert.AreEqual("fixture-character-agile-body", agileVisual.Sprite.name);
            Assert.AreSame(
                catalog.SpriteMotionProfiles[0],
                agile.MotionProfile.Resolve(catalog.Registry));
        }
    }
}
