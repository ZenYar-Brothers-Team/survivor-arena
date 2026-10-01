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
            Assert.AreEqual(1f, configuration.Timeline.SpawnOppositeBias);
            Assert.IsNull(configuration.Timeline.OpeningIntensity, "Only FIELD-001 gets the reduced opening rate.");
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
                "ENEMY-006", "ENEMY-009", "ENEMY-010" }, configuration.Enemies.Select(e => e.Id.ToString()));
            var first = configuration.Timeline.Phases[0];
            Assert.IsTrue(first.Composition.Any(c => c.Enemy.Id.ToString() == "ENEMY-010" && c.Weight > 0),
                "DECISION-0063/0136: a new enemy type (the guard shooter) is visible from the first wave.");
            foreach (var phase in configuration.Timeline.Phases)
            {
                Assert.AreEqual(1.15f, phase.Modifiers.HealthMultiplier, 1e-5f, phase.Id.ToString());
                Assert.AreEqual(1.08f, phase.Modifiers.ContactDamageMultiplier, 1e-5f, phase.Id.ToString());
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
            Assert.IsNull(timeline.OpeningIntensity, "The opening rate change is exclusive to FIELD-001.");
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
        public void Presentation_GeneratesTwentyBlobsEveryRun_InAMoreCompactArena()
        {
            // Map transfer (DECISION-0136): FIELD-002 keeps its art and uses the illustrated blobs of the former test field.
            var presentation = FixtureFieldEnvironmentPresentationCatalog.Load(RuntimeContentCatalog.ProductionFieldPresentationPath)
                .Values.Single(p => p.Id.ToString() == "FIELD-002-PRESENTATION");
            Assert.AreEqual(100f, presentation.ArenaSideLength);
            Assert.IsNull(presentation.ObstacleLayout);
            Assert.AreEqual(20, presentation.BlobLayout.TotalCount);
            Assert.AreEqual("FIELD-003-VISUAL-GROUND", presentation.Ground.Id.ToString());
            var layouts = new System.Collections.Generic.HashSet<string>();
            for (var seed = 0; seed < 100; seed++)
            {
                var shapes = FieldBlobLayoutGenerator.Generate(presentation.BlobLayout, presentation.ArenaSideLength.Value,
                    UnityEngine.Vector2.zero, seed, "FIELD-002-ENVIRONMENT");
                Assert.AreEqual(20, shapes.Count, $"seed {seed}");
                layouts.Add(string.Join("|", shapes.Select(s => s.Center.ToString("F2"))));
            }
            Assert.Greater(layouts.Count, 95, "A fresh seed gives a fresh arrangement.");
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
