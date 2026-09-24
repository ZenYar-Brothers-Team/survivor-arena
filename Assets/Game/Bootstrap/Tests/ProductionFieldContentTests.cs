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
        public void Field001_UsesTheProductionEncounterReferences()
        {
            var catalog = FixtureFieldCatalog.FromJson(JsonContentFile.ReadText(FixtureRuntimeContentCatalog.ProductionFieldsPath));
            var field = catalog.Roster.AllFields.Single();
            Assert.AreEqual("FIELD-001", field.Id.ToString());
            Assert.AreEqual("Деревенская окраина", field.DisplayName);
            Assert.AreEqual(1, field.Difficulty);
            Assert.AreEqual("BOSS-001", field.FinalBoss.Id.ToString());
            Assert.AreEqual("MIDBOSS-001", field.MidBoss.Value.Id.ToString());
            Assert.AreEqual("FIELD-001-TRAVELERS", field.Travelers.Value.Id.ToString());
            Assert.AreEqual(6, field.Enemies.Count);
            Assert.AreEqual("SpawnPoint", catalog.Environments.Single().SpawnPointName);
        }

        [Test]
        public void Presentation_Places64AuthoredObstaclesWithFreeStart()
        {
            var presentation = FixtureFieldEnvironmentPresentationCatalog.Load(FixtureRuntimeContentCatalog.ProductionFieldPresentationPath).Values.Single();
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
        public void ProductionComposition_ContainsOnlyStartupDefinitions_AndResolves()
        {
            var catalog = FixtureRuntimeContentCatalog.CreateProduction();
            Assert.IsTrue(catalog.IsProduction);
            Assert.AreEqual(25, catalog.BuildEntries.Count, "10 skills + 10 passives + 5 sets.");
            Assert.IsFalse(catalog.BuildEntries.Any(e => e.Id.ToString().StartsWith("FIXTURE-")));
            Assert.AreEqual("SET-017-ATTACK", catalog.SetAttackTemplates.Single().Id.ToString());
            Assert.AreEqual("CHAR-001", catalog.RunSetup.StartingCharacterId.ToString());
            Assert.AreEqual(0.5f, catalog.RunSetup.Draft.SetDraftChance);
            var configuration = catalog.Fields.Roster.AllFields.Single().Resolve(catalog.Registry);
            Assert.AreEqual("FIELD-001-TIMELINE", configuration.Timeline.Id.ToString());
            Assert.IsTrue(catalog.FieldEnvironmentPresentations.ContainsKey(configuration.Environment.Id));
        }
    }
}
