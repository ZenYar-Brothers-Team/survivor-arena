using System.Linq;
using Game.Content;
using Game.Enemy;
using Game.Field;
using Game.Presentation;
using Game.Traveler;
using NUnit.Framework;

namespace Game.Bootstrap.Tests
{
    /// <summary>FIELD-004 (field004-v1, DECISION-0129): field, timeline, bosses and Traveler schedule resolve from production data.</summary>
    public sealed class ProductionField004ContentTests
    {
        private static RuntimeContentCatalog Catalog => RuntimeContentCatalog.CreateProduction();

        private static ResolvedFieldConfiguration Field004()
        {
            var catalog = Catalog;
            return catalog.Fields.Roster.AllFields.Single(f => f.Id.ToString() == "FIELD-004").Resolve(catalog.Registry);
        }

        [Test]
        public void Field004_ResolvesItsOwnEncountersTimelineAndSchedule()
        {
            var configuration = Field004();
            Assert.AreEqual("FIELD-004-TIMELINE", configuration.Timeline.Id.ToString());
            Assert.AreEqual(1f, configuration.Timeline.SpawnOppositeBias);
            Assert.AreEqual("FIELD-004-ENVIRONMENT", configuration.Environment.Id.ToString());
            CollectionAssert.AreEquivalent(new[] { "BOSS-004", "MIDBOSS-004" }, configuration.Bosses.Select(b => b.Id.ToString()));
            Assert.AreEqual(4, ((TravelerScheduleDefinition)configuration.Travelers).FieldRank, "Traveler K uses r = 4.");
            Assert.AreEqual(2, configuration.Field.Difficulty, "Card: difficulty 2/5.");
            Assert.AreEqual("FIELD-004-VISUAL-BACKGROUND", configuration.Field.Thumbnail.Value.Id.ToString());
        }

        [Test]
        public void EnemyPool_AddsTheRoyalSpearman_FromTheFirstWave_WithFieldFourModifiers()
        {
            var configuration = Field004();
            CollectionAssert.AreEquivalent(new[] { "ENEMY-001", "ENEMY-002", "ENEMY-003", "ENEMY-004", "ENEMY-005", "ENEMY-006",
                "ENEMY-007", "ENEMY-008", "ENEMY-009", "ENEMY-010", "ENEMY-012", "ENEMY-013" }, configuration.Enemies.Select(e => e.Id.ToString()));
            Assert.IsTrue(configuration.Timeline.Phases[0].Composition.Any(c => c.Enemy.Id.ToString() == "ENEMY-012" && c.Weight > 0),
                "DECISION-0063/0067: the new type is visible from the first wave.");
            foreach (var phase in configuration.Timeline.Phases)
            {
                Assert.AreEqual(1.45f, phase.Modifiers.HealthMultiplier, 1e-5f, phase.Id.ToString());
                Assert.AreEqual(1.24f, phase.Modifiers.ContactDamageMultiplier, 1e-5f, phase.Id.ToString());
                Assert.AreEqual(1.24f, phase.Modifiers.AttackDamageMultiplier, 1e-5f, phase.Id.ToString());
                Assert.AreEqual(1f, phase.Modifiers.SpeedMultiplier, 1e-5f, phase.Id.ToString());
            }
            Assert.AreEqual(300, configuration.Timeline.MaxAliveEnemies, "Shared technical ordinary-enemy cap.");
            Assert.AreEqual(900f, configuration.Timeline.TotalDurationSeconds, 1e-3f);
        }

        [Test]
        public void Presentation_PlacesSparseCampBarricades_WithAFreeStart()
        {
            var presentation = FixtureFieldEnvironmentPresentationCatalog.Load(RuntimeContentCatalog.ProductionFieldPresentationPath)
                .Values.Single(p => p.Id.ToString() == "FIELD-004-PRESENTATION");
            // User request 2026-10-03: the fourth map is 115 x 115 (5 x 9 trap screens of 1.2 x the camera view) with smaller obstacles; 25-unit cells give sixteen camp setups.
            var side = presentation.ArenaSideLength.Value;
            Assert.AreEqual(115f, side);
            var obstacles = FieldObstacleLayoutGenerator.Generate(presentation.ObstacleLayout, side, UnityEngine.Vector2.zero,
                presentation.ObstacleLayout.ReferenceSeed, "FIELD-004-ENVIRONMENT");
            Assert.That(obstacles.Count, Is.InRange(10, 40), "One sparse camp setup per 25-unit cell (crates, dummies, braziers, carts).");
            Assert.IsTrue(obstacles.All(o => o.Width <= 2.0f && o.Height <= 1.5f), "The big obstacles were made smaller.");
            Assert.IsFalse(obstacles.Any(o => o.Kind == FieldObstacleKind.Fence), "The palisade fences were removed from the obstacle generation (user request 2026-10-03).");
            Assert.IsTrue(obstacles.All(o => FieldObstacleLayoutGenerator.Distance(
                new UnityEngine.Rect(o.X - o.Width / 2, o.Y - o.Height / 2, o.Width, o.Height), UnityEngine.Vector2.zero) >= 12f - 1e-3f),
                "The 12-unit camp start circle stays free.");
            Assert.AreEqual("FIELD-004-VISUAL-GROUND", presentation.Ground.Id.ToString());
            Assert.AreEqual("FIELD-004-VISUAL-PALISADE", presentation.Fence.Id.ToString());
            // The tent was removed from the obstacles (user request 2026-10-03).
            var props = new[] { "FIELD-004-VISUAL-SUPPLY-CART",
                "FIELD-004-VISUAL-BRAZIER", "FIELD-004-VISUAL-SUPPLY-CRATE", "FIELD-004-VISUAL-TRAINING-DUMMY" };
            var selected = new System.Collections.Generic.HashSet<string>();
            for (var seed = 0; seed < 20; seed++)
            {
                var run = FieldObstacleLayoutGenerator.Generate(presentation.ObstacleLayout, side, UnityEngine.Vector2.zero,
                    seed, "FIELD-004-ENVIRONMENT");
                var visuals = run.Where(item => item.VisualId.IsValid).Select(item => item.VisualId.ToString()).Distinct().ToArray();
                CollectionAssert.IsSubsetOf(visuals, props, $"No unapproved prop is selected (seed {seed}).");
                foreach (var visual in visuals) selected.Add(visual);
            }
            CollectionAssert.AreEquivalent(props, selected.ToArray(), "Every approved camp prop is selected across the reference seeds.");
            var cart = FieldObstacleLayoutGenerator.Generate(presentation.ObstacleLayout, side, UnityEngine.Vector2.zero, 1, "FIELD-004-ENVIRONMENT")
                .Where(o => o.VisualId.IsValid && o.VisualId.ToString() == "FIELD-004-VISUAL-SUPPLY-CART").ToArray();
            Assert.IsTrue(cart.All(o => o.Width >= 2.0f - 1e-3f && o.Height >= 1.4f - 1e-3f), "The supply cart is bigger.");
        }

        [Test]
        public void TravelerSchedule_UsesTheGlobalPoolAtRankFour()
        {
            var schedules = FixtureTravelerCatalog.CreateProduction().Schedules;
            var one = schedules.Single(s => s.Id.ToString() == "FIELD-001-TRAVELERS");
            var four = schedules.Single(s => s.Id.ToString() == "FIELD-004-TRAVELERS");
            CollectionAssert.AreEqual(one.TravelerIds, four.TravelerIds, "One global pool of implemented Travelers.");
            Assert.AreEqual(4, four.FieldRank);
            Assert.AreEqual(1.3f * 1.5f, four.Scale(780f, 900f), 1e-4f, "K = (1 + 0.1·(4−1))·(1 + 0.5·1).");
        }
    }
}
