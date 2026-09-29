using System.Linq;
using Game.Content;
using Game.Enemy;
using Game.Field;
using Game.Presentation;
using Game.Traveler;
using NUnit.Framework;

namespace Game.Bootstrap.Tests
{
    /// <summary>FIELD-002 slice (DECISION-0063): field, timeline, bosses and Traveler schedule resolve from production data.</summary>
    public sealed class ProductionField002ContentTests
    {
        private static RuntimeContentCatalog Catalog => RuntimeContentCatalog.CreateProduction();

        private static ResolvedFieldConfiguration Field002()
        {
            var catalog = Catalog;
            return catalog.Fields.Roster.AllFields.Single(f => f.Id.ToString() == "FIELD-002").Resolve(catalog.Registry);
        }

        [Test]
        public void Field002_ResolvesItsOwnEncountersTimelineAndSchedule()
        {
            var configuration = Field002();
            Assert.AreEqual("FIELD-002-TIMELINE", configuration.Timeline.Id.ToString());
            Assert.AreEqual("FIELD-002-ENVIRONMENT", configuration.Environment.Id.ToString());
            CollectionAssert.AreEquivalent(new[] { "BOSS-002", "MIDBOSS-002" }, configuration.Bosses.Select(b => b.Id.ToString()));
            Assert.AreEqual(2, ((TravelerScheduleDefinition)configuration.Travelers).FieldRank, "Traveler K uses r = 2.");
            Assert.AreEqual(1, configuration.Field.Difficulty);
            Assert.IsTrue(configuration.Field.Thumbnail.HasValue);
            Assert.AreEqual("FIELD-002-VISUAL-BACKGROUND", configuration.Field.Thumbnail.Value.Id.ToString());
            Assert.AreEqual(SpriteRole.Background, configuration.Field.Thumbnail.Value.Resolve(Catalog.Registry).Role);
        }

        [Test]
        public void EnemyPool_AddsThreeNewTypes_AndTheFirstWaveAlreadyShowsOne()
        {
            var configuration = Field002();
            CollectionAssert.AreEquivalent(new[] { "ENEMY-001", "ENEMY-002", "ENEMY-003", "ENEMY-004", "ENEMY-005", "ENEMY-007",
                "ENEMY-006", "ENEMY-008", "ENEMY-009" }, configuration.Enemies.Select(e => e.Id.ToString()));
            var first = configuration.Timeline.Phases[0];
            Assert.IsTrue(first.Composition.Any(c => c.Enemy.Id.ToString() == "ENEMY-008" && c.Weight > 0),
                "DECISION-0063: a new enemy type is visible from the first wave.");
            foreach (var phase in configuration.Timeline.Phases)
            {
                Assert.AreEqual(1.2f, phase.Modifiers.HealthMultiplier, 1e-5f, phase.Id.ToString());
                Assert.AreEqual(1.1f, phase.Modifiers.ContactDamageMultiplier, 1e-5f, phase.Id.ToString());
                Assert.AreEqual(1f, phase.Modifiers.SpeedMultiplier, 1e-5f, phase.Id.ToString());
            }
        }

        [TestCase("FIELD-002", 1.1f)]
        [TestCase("FIELD-003", 1.2f)]
        public void Timeline_FollowsTheField001Rhythm_WithShorterRestsAndDenserSpawns(string fieldId, float density)
        {
            // DECISION-0092: same 16 phases and modes as FIELD-001; 15-s rests; spawn intervals divided by the field density.
            var catalog = Catalog;
            var reference = catalog.Fields.Roster.AllFields.Single(f => f.Id.ToString() == "FIELD-001").Resolve(catalog.Registry).Timeline;
            var timeline = catalog.Fields.Roster.AllFields.Single(f => f.Id.ToString() == fieldId).Resolve(catalog.Registry).Timeline;
            Assert.AreEqual(16, reference.Phases.Count);
            Assert.AreEqual(reference.Phases.Count, timeline.Phases.Count);
            Assert.AreEqual(reference.TotalDurationSeconds, timeline.TotalDurationSeconds, 1e-3f);
            for (var i = 0; i < timeline.Phases.Count; i++)
            {
                var phase = timeline.Phases[i];
                var baseline = reference.Phases[i];
                Assert.AreEqual(baseline.SpawnMode, phase.SpawnMode, phase.Id.ToString());
                if (baseline.Tag == WavePhaseTag.Rest)
                    Assert.AreEqual(15f, phase.DurationSeconds, 1e-3f, phase.Id.ToString());
                if (phase.SpawnMode == WaveSpawnMode.Burst)
                    Assert.Greater(phase.Burst.Count, baseline.Burst.Count, phase.Id.ToString());
                else
                    Assert.LessOrEqual(phase.SpawnIntervalSeconds, baseline.SpawnIntervalSeconds / density + 1e-3f, phase.Id.ToString());
                var types = phase.Composition.Count(c => c.Weight > 0);
                Assert.That(types, Is.InRange(2, 7), phase.Id.ToString());
            }
        }

        [Test]
        public void Bosses_DashAndFireRingsOnDashEnd_WithTheFinalBossTeleport()
        {
            var bosses = Catalog.Bosses;
            var final = bosses.Single(b => b.Id.ToString() == "BOSS-002");
            Assert.AreEqual(WaveHookKind.FinalBoss, final.Hook);
            Assert.AreEqual(6000f, final.Body.MaxHealth);
            Assert.AreEqual(EnemyMovementKind.TelegraphedDash, final.Body.Movement.Kind);
            Assert.AreEqual(8, final.Body.DashVolley.Attack.ProjectileCount);
            Assert.AreEqual("BOSS-002-VISUAL-PROJECTILE", final.Body.DashVolley.Attack.ProjectileVisual.Id.ToString());
            Assert.AreEqual(16f, final.Body.DashVolley.Attack.Damage);
            Assert.AreEqual(2, final.Body.DashVolley.RepeatEveryNthDash);
            Assert.AreEqual(0.5f, final.Body.DashVolley.RepeatBelowHealthFraction, 1e-5f);
            Assert.IsNotNull(final.Teleport, "Same teleport rule as BOSS-001.");
            var mid = bosses.Single(b => b.Id.ToString() == "MIDBOSS-002");
            Assert.AreEqual(WaveHookKind.MidBoss, mid.Hook);
            Assert.AreEqual(1600f, mid.Body.MaxHealth);
            Assert.AreEqual(6, mid.Body.DashVolley.Attack.ProjectileCount);
            Assert.AreEqual("BOSS-002-VISUAL-PROJECTILE", mid.Body.DashVolley.Attack.ProjectileVisual.Id.ToString());
            Assert.AreEqual(0, mid.Body.DashVolley.RepeatEveryNthDash);
        }

        [Test]
        public void Presentation_GeneratesRowsOfRocksAndColumnsPerRun_WithAFreeStart()
        {
            var presentation = FixtureFieldEnvironmentPresentationCatalog.Load(RuntimeContentCatalog.ProductionFieldPresentationPath)
                .Values.Single(p => p.Id.ToString() == "FIELD-002-PRESENTATION");
            // DECISION-0068: rows of 3–5 rocks or columns are placed every run, one row per 38.4-unit cell (5×5).
            var obstacles = FieldObstacleLayoutGenerator.Generate(presentation.ObstacleLayout, 200f, UnityEngine.Vector2.zero,
                presentation.ObstacleLayout.ReferenceSeed, "FIELD-002-ENVIRONMENT");
            Assert.That(obstacles.Count, Is.InRange(75, 125), "25 rows of 3–5 pieces.");
            Assert.IsTrue(obstacles.All(o => FieldObstacleLayoutGenerator.Distance(
                    new UnityEngine.Rect(o.X - o.Width / 2, o.Y - o.Height / 2, o.Width, o.Height), UnityEngine.Vector2.zero) >= 8f - 1e-3f),
                "Start circle of 8 units stays free.");
            Assert.AreEqual("FIELD-002-VISUAL-GROUND", presentation.Ground.Id.ToString());
            Assert.AreEqual("FIELD-002-VISUAL-BOULDER", presentation.Obstacle.Id.ToString());
            Assert.AreEqual("FIELD-002-VISUAL-COLUMN", presentation.Column.Id.ToString());
            Assert.AreEqual("FIELD-002-VISUAL-SHRINE", presentation.Shrine.Id.ToString());
            Assert.Greater(presentation.ShrineChance, 0f);
            Assert.IsTrue(presentation.ObstacleLayout.UsesKind(FieldObstacleKind.Column));
            Assert.IsTrue(presentation.ObstacleLayout.UsesKind(FieldObstacleKind.Stump));
            var props = new[] { "FIELD-002-VISUAL-BOULDER", "FIELD-002-VISUAL-ROADSIDE-MILESTONE", "FIELD-002-VISUAL-ROADSIDE-BENCH",
                "FIELD-002-VISUAL-BROKEN-WAGON", "FIELD-002-VISUAL-ROAD-BARRICADE" };
            for (var seed = 0; seed < 20; seed++)
            {
                var run = FieldObstacleLayoutGenerator.Generate(presentation.ObstacleLayout, 200f, UnityEngine.Vector2.zero,
                    seed, "FIELD-002-ENVIRONMENT");
                CollectionAssert.AreEquivalent(props, run.Where(item => item.VisualId.IsValid)
                    .Select(item => item.VisualId.ToString()).Distinct().ToArray(),
                    $"DECISION-0073: every approved FIELD-002 rock-row prop appears in each run (seed {seed}).");
            }
        }

        [Test]
        public void TravelerSchedule_UsesTheGlobalPoolAtRankTwo()
        {
            var schedules = FixtureTravelerCatalog.CreateProduction().Schedules;
            var one = schedules.Single(s => s.Id.ToString() == "FIELD-001-TRAVELERS");
            var two = schedules.Single(s => s.Id.ToString() == "FIELD-002-TRAVELERS");
            CollectionAssert.AreEqual(one.TravelerIds, two.TravelerIds, "One global pool of implemented Travelers.");
            Assert.AreEqual(2, two.FieldRank);
            Assert.AreEqual(1.1f * 1.5f, two.Scale(780f, 900f), 1e-4f, "K = (1 + 0.1·(2−1))·(1 + 0.5·1).");
        }
    }
}
