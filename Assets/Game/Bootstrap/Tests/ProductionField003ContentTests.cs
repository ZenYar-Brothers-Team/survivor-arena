using System.Linq;
using Game.Content;
using Game.Enemy;
using Game.Field;
using Game.Presentation;
using Game.Traveler;
using NUnit.Framework;

namespace Game.Bootstrap.Tests
{
    /// <summary>FIELD-003 (field003-v1, DECISION-0067): field, timeline, bosses and Traveler schedule resolve from production data.</summary>
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
                Assert.AreEqual(1.24f, phase.Modifiers.HealthMultiplier, 1e-5f, phase.Id.ToString());
                Assert.AreEqual(1.16f, phase.Modifiers.ContactDamageMultiplier, 1e-5f, phase.Id.ToString());
                Assert.AreEqual(1.16f, phase.Modifiers.AttackDamageMultiplier, 1e-5f, phase.Id.ToString());
                Assert.AreEqual(1f, phase.Modifiers.SpeedMultiplier, 1e-5f, phase.Id.ToString());
            }
            Assert.AreEqual(200, configuration.Timeline.MaxAliveEnemies, "Shared technical ordinary-enemy cap.");
            Assert.AreEqual(900f, configuration.Timeline.TotalDurationSeconds, 1e-3f);
        }

        [Test]
        public void Presentation_PlacesTheRuins_WallsAsFencesAndRubbleAsBoulders_WithAFreeStart()
        {
            var presentation = FixtureFieldEnvironmentPresentationCatalog.Load(RuntimeContentCatalog.ProductionFieldPresentationPath)
                .Values.Single(p => p.Id.ToString() == "FIELD-003-PRESENTATION");
            // DECISION-0068: one ruined wall fragment with its rubble per 48-unit cell (4×4), turned at random every run.
            var obstacles = FieldObstacleLayoutGenerator.Generate(presentation.ObstacleLayout, 200f, UnityEngine.Vector2.zero,
                presentation.ObstacleLayout.ReferenceSeed, "FIELD-003-ENVIRONMENT");
            Assert.That(obstacles.Count, Is.InRange(96, 112), "16 fragments of 6–7 pieces.");
            Assert.That(obstacles.Count(o => o.Kind == FieldObstacleKind.Fence), Is.InRange(48, 64), "Wall fragments.");
            Assert.IsTrue(obstacles.Any(o => o.Kind == FieldObstacleKind.Stump), "Rubble.");
            Assert.IsTrue(obstacles.Any(o => o.Kind == FieldObstacleKind.Fence && o.Height > o.Width), "Some walls stand vertically.");
            Assert.IsTrue(obstacles.Any(o => o.Kind == FieldObstacleKind.Fence && o.Width > o.Height), "Some walls lie horizontally.");
            Assert.IsTrue(obstacles.All(o => FieldObstacleLayoutGenerator.Distance(
                    new UnityEngine.Rect(o.X - o.Width / 2, o.Y - o.Height / 2, o.Width, o.Height), UnityEngine.Vector2.zero) >= 10f - 1e-3f),
                "Start circle of 10 units stays free.");
            Assert.AreEqual("FIELD-003-VISUAL-GROUND", presentation.Ground.Id.ToString());
            Assert.AreEqual("FIELD-003-VISUAL-WALL", presentation.Fence.Id.ToString());
            Assert.AreEqual("FIELD-003-VISUAL-RUBBLE", presentation.Obstacle.Id.ToString());
            Assert.AreEqual("FIELD-003-VISUAL-WATER", presentation.Bush.Id.ToString());
            var props = new[] { "FIELD-003-VISUAL-WALL", "FIELD-003-VISUAL-RUINED-ARCH", "FIELD-003-VISUAL-RUBBLE",
                "FIELD-003-VISUAL-BROKEN-URNS", "FIELD-003-VISUAL-FALLEN-CAPSTONE", "FIELD-003-VISUAL-COLLAPSED-WELL" };
            for (var seed = 0; seed < 20; seed++)
            {
                var run = FieldObstacleLayoutGenerator.Generate(presentation.ObstacleLayout, 200f, UnityEngine.Vector2.zero,
                    seed, "FIELD-003-ENVIRONMENT");
                CollectionAssert.AreEquivalent(props, run.Where(item => item.VisualId.IsValid)
                    .Select(item => item.VisualId.ToString()).Distinct().ToArray(),
                    $"DECISION-0073: every approved FIELD-003 ruin prop appears in each run (seed {seed}).");
            }
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
