using System.Linq;
using Game.Content;
using Game.Enemy;
using Game.Field;
using Game.Presentation;
using Game.Traveler;
using NUnit.Framework;

namespace Game.Bootstrap.Tests
{
    /// <summary>FIELD-003 (field003-v2, DECISION-0067/0092): field, timeline, bosses and Traveler schedule resolve from production data.</summary>
    public sealed class ProductionField003ContentTests
    {
        private static RuntimeContentCatalog Catalog => RuntimeContentCatalog.CreateProduction();

        private static ResolvedFieldConfiguration Field003()
        {
            var catalog = Catalog;
            return catalog.Fields.Roster.AllFields.Single(f => f.Id.ToString() == "FIELD-003").Resolve(catalog.Registry);
        }

        [Test]
        public void Field003_ResolvesItsOwnEncountersTimelineAndSchedule()
        {
            var configuration = Field003();
            Assert.AreEqual("FIELD-003-TIMELINE", configuration.Timeline.Id.ToString());
            Assert.AreEqual(1f, configuration.Timeline.SpawnOppositeBias);
            Assert.AreEqual("FIELD-003-ENVIRONMENT", configuration.Environment.Id.ToString());
            CollectionAssert.AreEquivalent(new[] { "BOSS-003", "MIDBOSS-003" }, configuration.Bosses.Select(b => b.Id.ToString()));
            Assert.AreEqual(3, ((TravelerScheduleDefinition)configuration.Travelers).FieldRank, "Traveler K uses r = 3.");
            Assert.AreEqual(2, configuration.Field.Difficulty, "Card: difficulty 2/5.");
            Assert.AreEqual("FIELD-003-VISUAL-BACKGROUND", configuration.Field.Thumbnail.Value.Id.ToString());
        }

        [Test]
        public void EnemyPool_AddsTheGuardArcher_FromTheFirstWave_WithFieldThreeModifiers()
        {
            var configuration = Field003();
            CollectionAssert.AreEquivalent(new[] { "ENEMY-001", "ENEMY-002", "ENEMY-003", "ENEMY-004", "ENEMY-005", "ENEMY-006",
                "ENEMY-007", "ENEMY-008", "ENEMY-009", "ENEMY-010" }, configuration.Enemies.Select(e => e.Id.ToString()));
            Assert.IsTrue(configuration.Timeline.Phases[0].Composition.Any(c => c.Enemy.Id.ToString() == "ENEMY-010" && c.Weight > 0),
                "DECISION-0063/0067: the new type is visible from the first wave.");
            foreach (var phase in configuration.Timeline.Phases)
            {
                Assert.AreEqual(1.3f, phase.Modifiers.HealthMultiplier, 1e-5f, phase.Id.ToString());
                Assert.AreEqual(1.16f, phase.Modifiers.ContactDamageMultiplier, 1e-5f, phase.Id.ToString());
                Assert.AreEqual(1.16f, phase.Modifiers.AttackDamageMultiplier, 1e-5f, phase.Id.ToString());
                Assert.AreEqual(1f, phase.Modifiers.SpeedMultiplier, 1e-5f, phase.Id.ToString());
            }
            Assert.AreEqual(300, configuration.Timeline.MaxAliveEnemies, "Shared technical ordinary-enemy cap.");
            Assert.AreEqual(900f, configuration.Timeline.TotalDurationSeconds, 1e-3f);
        }

        [Test]
        public void Presentation_GeneratesSparseRoadsAndBookBranches_OnFirstMapGrass()
        {
            var presentation = FixtureFieldEnvironmentPresentationCatalog.Load(RuntimeContentCatalog.ProductionFieldPresentationPath)
                .Values.Single(p => p.Id.ToString() == "FIELD-003-PRESENTATION");
            Assert.IsNull(presentation.ObstacleLayout);
            Assert.IsNull(presentation.BlobLayout);
            Assert.AreEqual("FIELD-001-VISUAL-GROUND", presentation.Ground.Id.ToString());
            Assert.AreEqual(200, presentation.ArenaSideLength);
            var layout = FieldRoadLayoutGenerator.Generate(presentation.RoadLayout,presentation.RoadFallbackLayouts,42);
            Assert.GreaterOrEqual(layout.DeadEnds.Count,10);
            // Widths +20% rounded and ring 2 units from the border (DECISION-0137 revision 2026-10-02).
            Assert.AreEqual(10,layout.Profile.MainRoadWidth);
            Assert.AreEqual(8,layout.Profile.DeadEndWidth);
            Assert.AreEqual(7,layout.Profile.DeadEndEndRadius);
            Assert.AreEqual(2,layout.Profile.RingInset-layout.Profile.MainRoadWidth*.5f,"Grass between the ring and the arena border.");
            Assert.AreEqual(1,layout.Profile.BookUpgradeCount);
            Assert.LessOrEqual(layout.MainDistance(layout.SpawnPosition),1e-4f);
        }

        [Test]
        public void TravelerSchedule_UsesTheGlobalPoolAtRankThree()
        {
            var schedules = FixtureTravelerCatalog.CreateProduction().Schedules;
            var one = schedules.Single(s => s.Id.ToString() == "FIELD-001-TRAVELERS");
            var three = schedules.Single(s => s.Id.ToString() == "FIELD-003-TRAVELERS");
            CollectionAssert.AreEqual(one.TravelerIds, three.TravelerIds, "One global pool of implemented Travelers.");
            Assert.AreEqual(3, three.FieldRank);
            Assert.AreEqual(1.2f * 1.5f, three.Scale(780f, 900f), 1e-4f, "K = (1 + 0.1·(3−1))·(1 + 0.5·1).");
        }
    }
}
