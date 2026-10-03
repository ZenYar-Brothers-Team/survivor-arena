using System.Linq;
using Game.Content;
using Game.Traps;
using Game.Zones;
using NUnit.Framework;
using UnityEngine;

namespace Game.Bootstrap.Tests
{
    /// <summary>DECISION-0156: FIELD-004 carries the approved trap set; no other production field has traps yet.</summary>
    public sealed class ProductionField004TrapsTests
    {
        private static readonly string[] ApprovedNames =
        {
            "Копейный ряд", "Крест", "Диагональный крест", "Круг клинков", "Арбалетный столб", "Трезубец", "Спиральная",
            "Кольцо", "Баллиста", "Маятник", "Занавес", "Шипастая глыба", "Рассыпная"
        };

        private static TrapLayoutDefinition Layout()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            return catalog.FieldEnvironmentPresentations[new ContentId("FIELD-004-ENVIRONMENT")].TrapLayout;
        }

        [Test]
        public void Field004_HasTheApprovedThirteenTurretsAndBarrels()
        {
            var layout = Layout();
            Assert.IsNotNull(layout);
            CollectionAssert.AreEqual(ApprovedNames, layout.Types.Select(t => t.Name).ToArray());
            CollectionAssert.AreEqual(Enumerable.Range(1, 13).Select(i => $"TRAP-{i:000}").ToArray(),
                layout.Types.Select(t => t.Id.ToString()).ToArray());
            Assert.IsNotNull(layout.Density, "Random traps are scattered per screen.");
            Assert.AreEqual(40, layout.TotalTrapCount, "With a density the counts are only weights.");
            Assert.AreEqual(1, layout.Barrels.PerScreen, "One barrel on every screen.");
            Assert.AreEqual(1f / 3f, layout.Barrels.ExplosiveShare, 1e-3f, "Each barrel explodes with a one in three chance.");
            Assert.IsTrue(layout.Types.Any(t => t.Tier == 1) && layout.Types.Any(t => t.Tier == 2) && layout.Types.Any(t => t.Tier == 3));
        }

        [Test]
        public void Field004_SteppingFixedTurretsAlsoTurnJustAfterTheirShot_AndTheBladesAreAThirdSmaller()
        {
            var layout = Layout();
            foreach (var type in layout.Types.Where(t => t.Heading == TrapHeadingMode.Fixed && t.HeadingStepDegrees != 0f))
                Assert.Greater(type.AimResumeDelaySeconds, 0f, type.Name);
            Assert.AreEqual(0.35f / 1.5f, layout.Projectiles["blade"].Radius, 1e-3f, "Blades are one and a half times smaller.");
        }

        [Test]
        public void Field004_TheSpearRowIsPlacedFacingOneOfTheEightStandardDirections()
        {
            var row = Layout().Types.Single(t => t.Id.ToString() == "TRAP-001");
            CollectionAssert.AreEquivalent(new[] { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f }, row.RotationsDegrees);
        }

        [Test]
        public void Field004_AimedTurretsTurnJustAfterTheirShot_AndTheSpiralSweepsWithItsShots()
        {
            var layout = Layout();
            foreach (var type in layout.Types.Where(t => t.Heading == TrapHeadingMode.Aim))
                Assert.Greater(type.AimResumeDelaySeconds, 0f, type.Name + ": the head turns a moment after the shot, not with it.");
            var spiral = layout.Types.Single(t => t.Name == "Спиральная");
            Assert.Greater(spiral.LastShotDelaySeconds, 0f, "A series: the head follows the shots.");
            Assert.AreEqual(14, spiral.Shots.Count);
        }

        [Test]
        public void Field004_EveryModelIsScaledToTheSizeOfItsProjectile()
        {
            // scale = 0.55 + 0.25 x S, S = drawn projectile size: sprite length (3.2 x diameter, at least 0.5) for spear and bolt,
            // sprite diameter (1.3 x diameter) for the round kinds (user request 2026-10-03).
            var layout = Layout();
            var scales = new System.Collections.Generic.Dictionary<float, float>();
            foreach (var type in layout.Types.Where(t => t.ModelKey != "trap-012"))
            {
                var projectile = type.Shots[0].Projectile;
                var diameter = projectile.Radius * 2f;
                var directional = projectile.Visual == "spear" || projectile.Visual == "bolt";
                var drawn = directional ? Mathf.Max(.5f, diameter * 3.2f) : diameter * 1.3f;
                Assert.AreEqual(.55f + .25f * drawn, layout.Models[type.ModelKey].Scale, .01f, type.Name);
            }
            // The big-ball cannon is sized by its bore: the ball (drawn 1.3 x diameter) must not be much wider than the muzzle (~1.03 x scale).
            var ball = layout.Projectiles["spike-large"].Radius * 2f * 1.3f;
            Assert.That(ball / (1.03f * layout.Models["trap-012"].Scale), Is.InRange(.9f, 1.4f), "The huge ball fits the cannon it is fired from.");
            Assert.Greater(layout.Models["trap-012"].Scale, layout.Models["trap-011"].Scale, "Huge balls: a bigger cannon than small balls.");
            Assert.Greater(layout.Models["trap-009"].Scale, layout.Models["trap-005"].Scale, "Heavy bolt: a bigger ballista than the light one.");
        }

        [Test]
        public void Field004_UsesTheAgreedRadiusAndRageRules()
        {
            var layout = Layout();
            Assert.AreEqual(1f, layout.RadiusScreenWidths, "Activation, rage and cutoff share one radius: one screen width.");
            Assert.AreEqual(3f, layout.Rage.MaxMultiplier);
            Assert.AreEqual(60f, layout.Rage.SecondsToMax);
        }

        [Test]
        public void Field004_ProjectilesStayWithinTheFourArtKeys_AndSizesFollowTheAgreedClasses()
        {
            var layout = Layout();
            CollectionAssert.IsSubsetOf(layout.Projectiles.Values.Select(p => p.Visual).Distinct().ToArray(),
                new[] { "spear", "bolt", "blade", "spikeball" });
            Assert.AreEqual(4, layout.Projectiles.Values.Select(p => p.Visual).Distinct().Count());
            // Directional sprites are fast and never spin; round ones spin in the picture plane and carry the slow classes.
            foreach (var projectile in layout.Projectiles.Values)
            {
                if (projectile.Visual == "spear" || projectile.Visual == "bolt") Assert.AreEqual(0f, projectile.SpinDegreesPerSecond);
                if (projectile.Visual == "spikeball") Assert.Greater(projectile.SpinDegreesPerSecond, 0f);
            }
            Assert.Greater(layout.Projectiles["spike-large"].Radius, layout.Projectiles["spear"].Radius);
            Assert.Less(layout.Projectiles["spike-small"].Speed, layout.Projectiles["spear"].Speed);
        }

        [Test]
        public void Field004_RingIsDense_AndTheCurtainLeavesRoomToSlipThrough()
        {
            var layout = Layout();
            var ring = layout.Types.Single(t => t.Name == "Кольцо");
            Assert.AreEqual(16, ring.Shots.Count, "Half of the first 32: the ring is thinner.");
            var reach = ring.Shots[0].Projectile.MaxDistance;
            Assert.Greater(reach, 0f, "The ring is short-ranged: it fades close to the turret.");
            var chord = 2f * reach * Mathf.Sin(Mathf.PI / ring.Shots.Count);
            Assert.Less(chord - 2f * ring.Shots[0].Projectile.Radius, 2f * layout.PlayerHitRadius,
                "No gap in the ring is wide enough for the player.");
            var curtain = layout.Types.Single(t => t.Name == "Занавес");
            Assert.AreEqual(4, curtain.Shots.Count);
            var gap = curtain.Shots[1].Lateral - curtain.Shots[0].Lateral - 2f * curtain.Shots[0].Projectile.Radius;
            Assert.Greater(gap, 2f * layout.PlayerHitRadius, "A gap in the curtain is wide enough for the player.");
        }

        [Test]
        public void Field004_ScattersTrapsByScreen_OnTheRealArena()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            var presentation = catalog.FieldEnvironmentPresentations[new ContentId("FIELD-004-ENVIRONMENT")];
            var density = presentation.TrapLayout.Density;
            Assert.AreEqual(17.8f, density.CellWidth);
            Assert.AreEqual(10f, density.CellHeight);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, density.CountWeights.Select(w => w.Count).ToArray());
            CollectionAssert.AreEqual(new[] { 7, 2, 1 }, density.CountWeights.Select(w => w.Weight).ToArray(), "70% one trap, 20% two, 10% three.");
            var set = TrapPlacementGenerator.Generate(presentation.TrapLayout, presentation.ArenaSideLength.Value, Vector2.zero, null,
                presentation.TrapLayout.ReferenceSeed);
            Assert.Greater(set.Traps.Count, 55, "A trap or more on every screen of the 100-unit arena.");
            Assert.That(set.Barrels.Count, Is.InRange(54, 60), "One barrel in each of the 6 x 10 screen cells.");
        }

        [Test]
        public void EveryTrapType_HasAModel_WithAMatchingPrefabInTheLibrary()
        {
            var library = TrapPrefabLibrary.Load();
            Assert.IsNotNull(library, "Resources/" + TrapPrefabLibrary.ResourcePath);
            var layout = Layout();
            foreach (var type in layout.Types)
            {
                Assert.IsNotNull(type.ModelKey, type.Id + " has a 3D model.");
                var prefab = library.Find(type.ModelKey);
                Assert.IsNotNull(prefab, type.ModelKey + " resolves to a prefab.");
                Assert.IsNotNull(prefab.transform.Find("RotatingHead"), type.ModelKey);
                Assert.IsNotNull(prefab.transform.Find("StationaryBase"), type.ModelKey);
                var renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
                Assert.IsTrue(renderers.All(r => r.sharedMaterials.All(m => m != null && m.shader.name == "ArtPrototypes/TrapCrossMatte")),
                    type.ModelKey + ": no Blender import material (default shader) is left on the prefab.");
                Assert.GreaterOrEqual(renderers.Count(r => r.name.EndsWith("_Contour")), 5, type.ModelKey + ": parts have real contour shells.");
            }
            Assert.IsNull(library.Find("missing"));
        }

        [Test]
        public void Field004_SpritesCoverEveryProjectileKeyAndTheBarrel()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            var layout = Layout();
            foreach (var pair in layout.Sprites)
                Assert.IsNotNull(catalog.Registry.Get<Game.Presentation.SpriteDefinition>(new ContentId(pair.Value)).Sprite, pair.Key);
            CollectionAssert.AreEquivalent(new[] { "spear", "bolt", "blade", "spikeball", "barrel" }, layout.Sprites.Keys);
        }

        [Test]
        public void Field004_ScattersTrapsPerScreen_TheStartIsOpenToTraps_AndEveryScreenGetsABarrel()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            var presentation = catalog.FieldEnvironmentPresentations[new ContentId("FIELD-004-ENVIRONMENT")];
            var layout = presentation.TrapLayout;
            Assert.AreEqual(100f, presentation.ArenaSideLength, "The fourth map is 100 x 100.");
            Assert.AreEqual(13, layout.Types.Count);
            Assert.AreEqual(0, layout.StartTraps.Count, "The temporary start-screen review set was removed.");
            Assert.AreEqual(0, layout.StartBarrels.Count);
            Assert.AreEqual(0f, layout.StartClearRadius, "The start zone is open to traps.");
            var set = TrapPlacementGenerator.Generate(layout, presentation.ArenaSideLength.Value, Vector2.zero, null, layout.ReferenceSeed);
            Assert.IsTrue(set.Traps.Any(t => Mathf.Abs(t.Center.x) < 8.9f && Mathf.Abs(t.Center.y) < 5f), "The first screen has traps too.");
            Assert.Greater(set.Traps.Count, 50, "A trap or two on every screen of the 100-unit map.");
            var byType = set.Traps.GroupBy(t => t.Type.Id.ToString()).ToDictionary(g => g.Key, g => g.Count());
            Assert.AreEqual(13, byType.Count, "Every trap type appears in the scatter: " + string.Join(", ", byType.Select(p => p.Key + "=" + p.Value)));
            Assert.Less(byType.Values.Max(), set.Traps.Count * .3f, "No single type dominates the scatter.");
            Assert.That(set.Barrels.Count, Is.InRange(54, 60), "One barrel in each of the 6 x 10 screen cells.");
            var cellWidth = 100f / 6f; var cellHeight = 10f;
            var perCell = set.Barrels.GroupBy(b => (Mathf.FloorToInt((b.Center.x + 50f) / cellWidth), Mathf.FloorToInt((b.Center.y + 50f) / cellHeight)));
            Assert.IsTrue(perCell.All(g => g.Count() == 1), "Never more than one barrel on a screen.");
            var share = set.Barrels.Count(b => b.Explosive) / (float)set.Barrels.Count;
            Assert.That(share, Is.InRange(.15f, .55f), "Roughly one barrel in three is explosive (a chance, not a quota).");
        }

        [Test]
        public void OtherProductionFields_HaveNoTraps()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            // The traps belong to the fourth map only (user request 2026-10-03).
            foreach (var pair in catalog.FieldEnvironmentPresentations)
                if (pair.Key.ToString() != "FIELD-004-ENVIRONMENT")
                    Assert.IsNull(pair.Value.TrapLayout, $"{pair.Key} must not have traps (DECISION-0156).");
        }
    }
}
