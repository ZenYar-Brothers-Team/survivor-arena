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
            FixtureWaveTimelineCatalog.FromJson(JsonContentFile.ReadText(RuntimeContentCatalog.ProductionWaveTimelinePath));

        [Test]
        public void Timeline_Covers900Seconds_WithBossHooksAndAllSixOrdinaries()
        {
            var timeline = Timeline();
            Assert.AreEqual("FIELD-001-TIMELINE", timeline.Id.ToString());
            Assert.AreEqual(16, timeline.Phases.Count);
            Assert.AreEqual(900f, timeline.TotalDurationSeconds, 1e-3f);
            Assert.AreEqual(10f, timeline.SpawnRadius);
            Assert.AreEqual(1f, timeline.SpawnOppositeBias);
            Assert.AreEqual(30f, timeline.OpeningIntensity.DurationSeconds);
            Assert.AreEqual(0.6f, timeline.OpeningIntensity.RateMultiplier);
            Assert.AreEqual(20f, timeline.OpeningSpawn.DurationSeconds, "DECISION-0057: screen-edge opening spawn.");
            Assert.AreEqual(1f, timeline.OpeningSpawn.ScreenMargin);
            Assert.AreEqual(450f, timeline.Hooks.Single(h => h.Kind == WaveHookKind.MidBoss).TimeSeconds);
            Assert.AreEqual(810f, timeline.Hooks.Single(h => h.Kind == WaveHookKind.FinalBoss).TimeSeconds);
            var ids = timeline.Phases.SelectMany(p => p.Composition).Select(c => c.Enemy.Id.ToString()).Distinct().OrderBy(i => i);
            CollectionAssert.AreEqual(new[] { "ENEMY-001", "ENEMY-002", "ENEMY-003", "ENEMY-004", "ENEMY-005", "ENEMY-007" }, ids);
            Assert.AreEqual(300, timeline.MaxAliveEnemies);
            Assert.IsTrue(timeline.Phases.All(p => p.Composition.Count >= 2 && p.Composition.Count <= 4),
                "FIELD-001 phases use focused two-to-four enemy compositions.");
            Assert.IsTrue(timeline.Phases.Skip(4).Where(p => p.Tag != WavePhaseTag.Rest && p.SpawnMode == WaveSpawnMode.Continuous)
                .All(p => p.DurationSeconds >= 70f && p.DurationSeconds <= 90f), "Later combat waves last 70–90 seconds.");
            Assert.IsTrue(timeline.Phases.Where(p => p.Tag == WavePhaseTag.Rest).All(p => p.DurationSeconds == 20f),
                "Respites stay short.");
            CollectionAssert.AreEqual(new[] { 12, 20 },
                timeline.Phases.Where(p => p.SpawnMode == WaveSpawnMode.Burst).Select(p => p.Burst.Count).ToArray());
            Assert.IsFalse(timeline.Phases.Take(8).Any(p => p.Composition.Any(c => c.Enemy.Id.ToString() == "ENEMY-005")),
                "Archer enters only after the opening sequence.");
        }

        [Test]
        public void BossesAndTravelers_ResolveApprovedBodyArt()
        {
            // DECISION-0057: bosses and Travelers were spawned without their body art (placeholder squares).
            var catalog = RuntimeContentCatalog.CreateProduction();
            // All twenty approved boss bodies now have per-ID art; later fields still gate live encounter review.
            var bound = catalog.Bosses.ToArray();
            Assert.AreEqual(20, bound.Length, "All final and mid bosses have approved body art.");
            var bodies = bound.Select(boss => boss.Body)
                .Concat(catalog.Travelers.Definitions.Values.Select(traveler => traveler.Body)).ToArray();
            Assert.AreEqual(30, bodies.Length, "Twenty bosses and all ten Travelers (DECISION-0088).");
            foreach (var body in bodies)
            {
                var visual = EnemyBodyVisual.Resolve(body, catalog.Registry);
                Assert.IsNotNull(visual.Sprite, $"{body.Id} body sprite");
                Assert.IsNotNull(visual.Motion, $"{body.Id} motion profile");
                StringAssert.EndsWith("-body", visual.Sprite.name, $"{body.Id} uses its approved body image");
            }

            // Projectile attacks share one approved visual family per field pair.
            var bossAttacks = bound
                .SelectMany(boss => boss.OwnedAttacks
                    .Where(attack => attack.Attack != null)
                    .Select(attack => (boss, attack)))
                .ToArray();
            Assert.Greater(bossAttacks.Length, 4, "Production bosses include the late projectile families.");
            foreach (var (boss, attack) in bossAttacks)
            {
                var expectedOwner = boss.Id.ToString().Replace("MIDBOSS", "BOSS");
                Assert.AreEqual($"{expectedOwner}-VISUAL-PROJECTILE", attack.Attack.ProjectileVisual.Id.ToString(), boss.Id.ToString());
                var projectile = attack.Attack.ProjectileVisual.Resolve(catalog.Registry);
                Assert.AreEqual(SpriteRole.Projectile, projectile.Role, $"{attack.Id} projectile role");
                Assert.AreEqual($"{expectedOwner.ToLowerInvariant()}-projectile", projectile.Sprite.name, $"{attack.Id} projectile image");
            }
        }

        [Test]
        public void Field001_UsesTheProductionEncounterReferences()
        {
            var catalog = FixtureFieldCatalog.FromJson(JsonContentFile.ReadText(RuntimeContentCatalog.ProductionFieldsPath));
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
        public void Presentation_GeneratesDenseMixedObstaclesPerRun_WithFreeStart()
        {
            // DECISION-0069: 12×12 cells, two compact obstacles in each, with clear inter-cell passages.
            var presentation = FixtureFieldEnvironmentPresentationCatalog.Load(RuntimeContentCatalog.ProductionFieldPresentationPath).Values
                .Single(p => p.Id.ToString() == "FIELD-001-PRESENTATION");
            Assert.AreEqual(0, presentation.ExplicitObstacles.Count);
            Assert.IsNotNull(presentation.ObstacleLayout);
            foreach (var seed in new[] { presentation.ObstacleLayout.ReferenceSeed, 1, 2, 3 })
            {
                var obstacles = FieldObstacleLayoutGenerator.Generate(presentation.ObstacleLayout, 200f, UnityEngine.Vector2.zero, seed,
                    "FIELD-001-ENVIRONMENT");
                Assert.AreEqual(288, obstacles.Count, $"seed {seed}");
                CollectionAssert.IsSubsetOf(new[] { FieldObstacleKind.Stump, FieldObstacleKind.Fence,
                    FieldObstacleKind.Barrel, FieldObstacleKind.Rock }, obstacles.Select(item => item.Kind).Distinct().ToArray());
                Assert.AreEqual(4, obstacles.Select(item => item.Kind).Distinct().Count(), "All four thumbnail-based prop types appear.");
                CollectionAssert.IsSubsetOf(new[] { "FIELD-001-VISUAL-HAY-BALES", "FIELD-001-VISUAL-VILLAGE-HANDCART" },
                    obstacles.Where(item => item.VisualId.IsValid).Select(item => item.VisualId.ToString()).Distinct().ToArray());
                Assert.AreEqual(2, obstacles.Where(item => item.VisualId.IsValid).Select(item => item.VisualId).Distinct().Count(),
                    "Both approved FIELD-001 additions participate in the layout.");
                foreach (var obstacle in obstacles)
                {
                    Assert.LessOrEqual(System.Math.Abs(obstacle.X) + obstacle.Width / 2, 99f);
                    Assert.LessOrEqual(System.Math.Abs(obstacle.Y) + obstacle.Height / 2, 99f);
                    Assert.Greater(obstacle.X * obstacle.X + obstacle.Y * obstacle.Y, 36f, "Start area stays free.");
                    Assert.IsTrue(obstacle.Kind != FieldObstacleKind.Fence || obstacle.Width > obstacle.Height,
                        "Fences remain horizontal (DECISION-0046).");
                }
            }
        }

        [Test]
        public void Presentation_AlwaysShowsTwoSeparatedObstaclesOnTheOpeningScreen()
        {
            var presentation = FixtureFieldEnvironmentPresentationCatalog.Load(RuntimeContentCatalog.ProductionFieldPresentationPath).Values
                .Single(p => p.Id.ToString() == "FIELD-001-PRESENTATION");
            var screen = presentation.ObstacleLayout.StartScreen;
            Assert.IsNotNull(screen);
            for (var seed = 0; seed < 100; seed++)
            {
                var obstacles = FieldObstacleLayoutGenerator.Generate(presentation.ObstacleLayout, 200f,
                    UnityEngine.Vector2.zero, seed, "FIELD-001-ENVIRONMENT");
                var visible = obstacles.Where(item =>
                    System.Math.Abs(item.X) + item.Width * .5f <= screen.HalfWidth &&
                    System.Math.Abs(item.Y) + item.Height * .5f <= screen.HalfHeight).ToArray();
                Assert.GreaterOrEqual(visible.Length, 2, $"seed {seed}: two obstacles must be fully in view");
                Assert.IsTrue(visible.Any(item => item.X < 0 && System.Math.Abs(item.X) >= screen.MinAbsX),
                    $"seed {seed}: left-hand opening obstacle");
                Assert.IsTrue(visible.Any(item => item.X > 0 && System.Math.Abs(item.X) >= screen.MinAbsX),
                    $"seed {seed}: right-hand opening obstacle");
                foreach (var item in visible)
                {
                    var rect = new UnityEngine.Rect(item.X - item.Width * .5f, item.Y - item.Height * .5f,
                        item.Width, item.Height);
                    Assert.GreaterOrEqual(FieldObstacleLayoutGenerator.Distance(rect, UnityEngine.Vector2.zero), 6f - 1e-3f,
                        $"seed {seed}: obstacle touches the player start");
                }
            }
        }

        [Test]
        public void ProductionComposition_ContainsProductionDefinitions_AndResolves()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            Assert.IsTrue(catalog.IsProduction);
            Assert.AreEqual(65, catalog.BuildEntries.Count,
                "16 skills + 14 passives + 35 sets; profile access filters locked ones from the draft (DECISION-0060/0061).");
            Assert.IsFalse(catalog.BuildEntries.Any(e => e.Id.ToString().StartsWith("FIXTURE-")));
            CollectionAssert.AreEqual(new[] { "SET-013-ATTACK", "SET-015-ATTACK", "SET-016-ATTACK", "SET-017-ATTACK",
                "SET-018-ATTACK", "SET-019-ATTACK", "SET-020-ATTACK", "SET-021-ATTACK", "SET-022-ATTACK" }, catalog.SetAttackTemplates.Select(t => t.Id.ToString()));
            Assert.AreEqual("CHAR-001", catalog.RunSetup.StartingCharacterId.ToString());
            Assert.AreEqual(0.5f, catalog.RunSetup.Draft.SetDraftChance);
            Assert.AreEqual(45f, catalog.RunSetup.Experience.BaseDropLifetimeSeconds,
                "DECISION-0057: dropped XP disappears after 45 s (playtest 2026-09-25_5233a664 OBS-04).");
            CollectionAssert.AreEqual(new[] { "FIELD-001", "FIELD-002", "FIELD-003", "FIELD-004", "FIELD-006", "FIELD-007", "FIELD-009", "FIELD-DEV-ZONES", "FIELD-DEV-ALTARS" },
                catalog.Fields.Roster.AllFields.Select(f => f.Id.ToString()));
            var configuration = catalog.Fields.Roster.AllFields.Single(f => f.Id.ToString() == "FIELD-001").Resolve(catalog.Registry);
            Assert.AreEqual(SpriteRole.Background,
                configuration.Field.Thumbnail.Value.Resolve(catalog.Registry).Role);
            Assert.AreEqual("FIELD-001-TIMELINE", configuration.Timeline.Id.ToString());
            Assert.IsTrue(catalog.FieldEnvironmentPresentations.ContainsKey(configuration.Environment.Id));
        }
    }
}
