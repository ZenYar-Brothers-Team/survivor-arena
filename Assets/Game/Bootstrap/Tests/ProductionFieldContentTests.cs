using System.Linq;
using Game.Content;
using Game.Content.Json;
using Game.Enemy;
using Game.Field;
using Game.Presentation;
using NUnit.Framework;

namespace Game.Bootstrap.Tests
{
    /// <summary>F1-08: FIELD-001 production field, 900-second timeline, authored obstacles and composition.</summary>
    public sealed class ProductionFieldContentTests
    {
        private static WaveTimelineDefinition Timeline() =>
            FixtureWaveTimelineCatalog.FromJson(JsonContentFile.ReadText(FixtureRuntimeContentCatalog.ProductionWaveTimelinePath));

        [Test]
        public void Timeline_Covers900Seconds_WithBossHooksAndAllSixOrdinaries()
        {
            var timeline = Timeline();
            Assert.AreEqual("FIELD-001-TIMELINE", timeline.Id.ToString());
            Assert.AreEqual(24, timeline.Phases.Count);
            Assert.AreEqual(900f, timeline.TotalDurationSeconds, 1e-3f);
            Assert.AreEqual(12f, timeline.SpawnRadius);
            Assert.AreEqual(20f, timeline.OpeningSpawn.DurationSeconds, "DECISION-0057: screen-edge opening spawn.");
            Assert.AreEqual(1f, timeline.OpeningSpawn.ScreenMargin);
            Assert.AreEqual(450f, timeline.Hooks.Single(h => h.Kind == WaveHookKind.MidBoss).TimeSeconds);
            Assert.AreEqual(810f, timeline.Hooks.Single(h => h.Kind == WaveHookKind.FinalBoss).TimeSeconds);
            var ids = timeline.Phases.SelectMany(p => p.Composition).Select(c => c.Enemy.Id.ToString()).Distinct().OrderBy(i => i);
            CollectionAssert.AreEqual(new[] { "ENEMY-001", "ENEMY-002", "ENEMY-003", "ENEMY-004", "ENEMY-005", "ENEMY-007" }, ids);
            Assert.AreEqual(200, timeline.Phases.Max(p => p.MaxAliveEnemies));
            CollectionAssert.AreEqual(new[] { 12, 18, 18, 24, 24, 20 },
                timeline.Phases.Where(p => p.SpawnMode == WaveSpawnMode.Burst).Select(p => p.Burst.Count).ToArray());
            Assert.IsFalse(timeline.Phases.Take(9).Any(p => p.Composition.Any(c => c.Enemy.Id.ToString() == "ENEMY-005")),
                "Archer enters only from 6:00.");
        }

        [Test]
        public void BossesAndTravelers_ResolveApprovedBodyArt()
        {
            // DECISION-0057: bosses and Travelers were spawned without their body art (placeholder squares).
            var catalog = FixtureRuntimeContentCatalog.CreateProduction();
            // FIELD-002 bosses have no approved body art yet (DECISION-0063 art gate); they must not borrow any.
            foreach (var id in new[] { "BOSS-002", "MIDBOSS-002" })
                Assert.IsFalse(catalog.Bosses.Single(boss => boss.Id.ToString() == id).Body.Visual.Id.IsValid, id);
            var bodies = catalog.Bosses.Where(boss => boss.Id.ToString().EndsWith("-001")).Select(boss => boss.Body)
                .Concat(catalog.Travelers.Definitions.Values.Select(traveler => traveler.Body)).ToArray();
            Assert.AreEqual(5, bodies.Length, "MIDBOSS-001, BOSS-001, TRAVELER-001/002/005.");
            foreach (var body in bodies)
            {
                var visual = EnemyBodyVisual.Resolve(body, catalog.Registry);
                Assert.IsNotNull(visual.Sprite, $"{body.Id} body sprite");
                Assert.IsNotNull(visual.Motion, $"{body.Id} motion profile");
                StringAssert.EndsWith("-body", visual.Sprite.name, $"{body.Id} uses its approved body image");
            }

            // Boss shots resolve through the registry the boss runtime now receives.
            var bossAttacks = catalog.Bosses.SelectMany(boss => boss.OwnedAttacks).ToArray();
            Assert.AreEqual(4, bossAttacks.Length, "BOSS-001 fan/ring, normal and enraged.");
            foreach (var attack in bossAttacks)
            {
                var projectile = attack.Attack.ProjectileVisual.Resolve(catalog.Registry);
                Assert.AreEqual(SpriteRole.Projectile, projectile.Role, $"{attack.Id} projectile role");
                Assert.AreEqual("boss-001-projectile", projectile.Sprite.name, $"{attack.Id} projectile image");
            }
        }

        [Test]
        public void Field001_UsesTheProductionEncounterReferences()
        {
            var catalog = FixtureFieldCatalog.FromJson(JsonContentFile.ReadText(FixtureRuntimeContentCatalog.ProductionFieldsPath));
            var field = catalog.Roster.AllFields.Single(f => f.Id.ToString() == "FIELD-001");
            Assert.AreEqual("Деревенская окраина", field.DisplayName);
            Assert.AreEqual(1, field.Difficulty);
            Assert.AreEqual("BOSS-001", field.FinalBoss.Id.ToString());
            Assert.AreEqual("MIDBOSS-001", field.MidBoss.Value.Id.ToString());
            Assert.AreEqual("FIELD-001-TRAVELERS", field.Travelers.Value.Id.ToString());
            Assert.AreEqual("FIELD-001-VISUAL-BACKGROUND", field.Thumbnail.Value.Id.ToString());
            Assert.AreEqual(6, field.Enemies.Count);
            Assert.AreEqual("SpawnPoint", catalog.Environments.Single(e => e.Id.ToString() == "FIELD-001-ENVIRONMENT").SpawnPointName);
        }

        [Test]
        public void Presentation_Places64AuthoredObstaclesWithFreeStart()
        {
            var presentation = FixtureFieldEnvironmentPresentationCatalog.Load(FixtureRuntimeContentCatalog.ProductionFieldPresentationPath).Values
                .Single(p => p.Id.ToString() == "FIELD-001-PRESENTATION");
            Assert.AreEqual(64, presentation.ExplicitObstacles.Count);
            foreach (var obstacle in presentation.ExplicitObstacles)
            {
                Assert.LessOrEqual(System.Math.Abs(obstacle.X) + obstacle.Width / 2, 100f);
                Assert.LessOrEqual(System.Math.Abs(obstacle.Y) + obstacle.Height / 2, 100f);
                Assert.Greater(obstacle.X * obstacle.X + obstacle.Y * obstacle.Y, 4f, "Start area stays free.");
            }
            Assert.AreEqual(16, presentation.ExplicitObstacles.Count(o => System.Math.Abs(o.X) <= 20 && System.Math.Abs(o.Y) <= 20));
        }

        [Test]
        public void ProductionComposition_ContainsProductionDefinitions_AndResolves()
        {
            var catalog = FixtureRuntimeContentCatalog.CreateProduction();
            Assert.IsTrue(catalog.IsProduction);
            Assert.AreEqual(50, catalog.BuildEntries.Count,
                "16 skills + 14 passives + 20 sets; profile access filters locked ones from the draft (DECISION-0060/0061).");
            Assert.IsFalse(catalog.BuildEntries.Any(e => e.Id.ToString().StartsWith("FIXTURE-")));
            CollectionAssert.AreEqual(new[] { "SET-013-ATTACK", "SET-015-ATTACK", "SET-016-ATTACK", "SET-017-ATTACK",
                "SET-018-ATTACK", "SET-019-ATTACK", "SET-020-ATTACK" }, catalog.SetAttackTemplates.Select(t => t.Id.ToString()));
            Assert.AreEqual("CHAR-001", catalog.RunSetup.StartingCharacterId.ToString());
            Assert.AreEqual(0.5f, catalog.RunSetup.Draft.SetDraftChance);
            Assert.AreEqual(45f, catalog.RunSetup.Experience.BaseDropLifetimeSeconds,
                "DECISION-0057: dropped XP disappears after 45 s (playtest 2026-09-25_5233a664 OBS-04).");
            CollectionAssert.AreEqual(new[] { "FIELD-001", "FIELD-002" }, catalog.Fields.Roster.AllFields.Select(f => f.Id.ToString()));
            var configuration = catalog.Fields.Roster.AllFields.Single(f => f.Id.ToString() == "FIELD-001").Resolve(catalog.Registry);
            Assert.AreEqual(SpriteRole.Background,
                configuration.Field.Thumbnail.Value.Resolve(catalog.Registry).Role);
            Assert.AreEqual("FIELD-001-TIMELINE", configuration.Timeline.Id.ToString());
            Assert.IsTrue(catalog.FieldEnvironmentPresentations.ContainsKey(configuration.Environment.Id));
        }
    }
}
