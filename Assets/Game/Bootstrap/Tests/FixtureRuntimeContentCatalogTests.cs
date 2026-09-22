using System.Linq;
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
            Assert.AreEqual(3, catalog.SpriteMotionProfiles.Count);
            Assert.AreEqual(.3f, catalog.EnemyDeathPresentation.TotalDurationSeconds, .0001f);
            Assert.IsTrue(catalog.SourceSnapshot.ContainsKey("Content/Presentation/FixtureEnemyDeathPresentation"));
            Assert.AreEqual(.86f, catalog.GroundShadowPresentation.FallbackWidth, .0001f);
            Assert.AreEqual(1.2f, catalog.GroundShadowPresentation.ContactWidthScale, .0001f);
            Assert.IsTrue(catalog.SourceSnapshot.ContainsKey("Content/Presentation/FixtureGroundShadowPresentation"));
            Assert.AreEqual(1, catalog.FieldEnvironmentPresentations.Count);
            var fieldPresentation = catalog.FieldEnvironmentPresentations.Single().Value;
            Assert.AreEqual("FIXTURE-ENVIRONMENT-ARENA", fieldPresentation.EnvironmentId.ToString());
            Assert.AreEqual(SpriteRole.Tile, fieldPresentation.Ground.Resolve(catalog.Registry).Role);
            Assert.AreEqual(SpriteRole.Prop, fieldPresentation.Obstacle.Resolve(catalog.Registry).Role);
            Assert.AreEqual(64, fieldPresentation.InteriorObstacleCount);
            Assert.AreEqual(16, fieldPresentation.NearObstacleCount);
            Assert.IsTrue(catalog.SourceSnapshot.ContainsKey("Content/Presentation/FixtureFieldEnvironmentPresentation"));
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

            var stone = catalog.Registry.Get<SpriteDefinition>("SKILL-001-VISUAL-PROJECTILE");
            Assert.AreEqual(SpriteRole.Projectile, stone.Role);
            Assert.AreEqual(140f, stone.ProjectilePresentation.SpinDegreesPerSecond);
            var bolt = catalog.ActiveSkills.Single(skill => skill.Id == "FIXTURE-SKILL-BOLT");
            Assert.IsTrue(bolt.Levels.All(level => level.Visual.Id == stone.Id));
            Assert.IsTrue(catalog.ActiveSkills.All(skill => skill.Icon.Id.IsValid));
            Assert.AreEqual(catalog.ActiveSkills.Count,
                catalog.ActiveSkills.Select(skill => skill.Icon.Id).Distinct().Count());
            foreach (var skill in catalog.ActiveSkills)
            {
                var icon = skill.Icon.Resolve(catalog.Registry);
                Assert.AreEqual(SpriteRole.Icon, icon.Role, $"{skill.Id} icon role");
                Assert.IsNotNull(icon.Sprite, $"{skill.Id} icon sprite");
            }
            Assert.IsTrue(catalog.Passives.All(passive => passive.Icon.Id.IsValid));
            Assert.AreEqual(catalog.Passives.Count,
                catalog.Passives.Select(passive => passive.Icon.Id).Distinct().Count());
            foreach (var passive in catalog.Passives)
            {
                var icon = passive.Icon.Resolve(catalog.Registry);
                Assert.AreEqual(SpriteRole.Icon, icon.Role, $"{passive.Id} icon role");
                Assert.IsNotNull(icon.Sprite, $"{passive.Id} icon sprite");
            }
            Assert.IsTrue(catalog.Sets.All(set => set.Icon.Id.IsValid));
            Assert.AreEqual(catalog.Sets.Count, catalog.Sets.Select(set => set.Icon.Id).Distinct().Count());
            foreach (var set in catalog.Sets)
            {
                var icon = set.Icon.Resolve(catalog.Registry);
                Assert.AreEqual(SpriteRole.Icon, icon.Role, $"{set.Id} icon role");
                Assert.IsNotNull(icon.Sprite, $"{set.Id} icon sprite");
            }
            var fan = catalog.Enemies.Single(enemy => enemy.Id == "FIXTURE-ENEMY-FAN");
            Assert.AreEqual("FIXTURE-ENEMY-FAN-VISUAL-PROJECTILE", fan.Attack.ProjectileVisual.Id.ToString());
            Assert.AreEqual(SpriteRole.Pickup, catalog.Pickups.ExperienceVisual.Resolve(catalog.Registry).Role);
            Assert.IsTrue(catalog.Pickups.Definitions.All(definition =>
                definition.Visual.Resolve(catalog.Registry).Role == SpriteRole.Pickup));
        }
    }
}
