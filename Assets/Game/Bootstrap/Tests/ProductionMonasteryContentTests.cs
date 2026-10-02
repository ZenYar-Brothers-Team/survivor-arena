using System.Linq;
using Game.Content;
using Game.Presentation;
using Game.Zones;
using NUnit.Framework;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace Game.Bootstrap.Tests
{
    public sealed class ProductionMonasteryContentTests
    {
        [Test]
        public void Field007_ApprovedPreview_HasAssignedGroundAndBalancedThirtySixAltars()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            var field = catalog.Fields.Roster.AllFields.Single(f => f.Id.ToString() == "FIELD-007");
            var config = field.Resolve(catalog.Registry);
            Assert.AreEqual("FIELD-001-TIMELINE", config.Timeline.Id.ToString());
            var presentation = catalog.FieldEnvironmentPresentations[config.Environment.Id];
            Assert.AreEqual("FIELD-007-VISUAL-GROUND", presentation.Ground.Id.ToString());
            Assert.AreEqual(120f, presentation.ArenaSideLength);
            var obstacleCells = Mathf.FloorToInt((presentation.ArenaSideLength.Value - 2f * presentation.ObstacleLayout.EdgeMargin) / presentation.ObstacleLayout.CellSize);
            Assert.AreEqual(obstacleCells * obstacleCells * presentation.ObstacleLayout.PatternsPerCell, presentation.InteriorObstacleCount);
            var layout = presentation.ZoneLayout;
            Assert.AreEqual(36, layout.Zones.Count);
            Assert.AreEqual(3, layout.MaxPerScreen);
            Assert.AreEqual(12, layout.Zones.Count(e => e.Kind == ZoneEffectKind.Shrine));
            Assert.AreEqual(12, layout.Zones.Count(e => e.Kind != ZoneEffectKind.Shrine && e.Polarity == ZoneAltarPolarity.Positive));
            Assert.AreEqual(12, layout.Zones.Count(e => e.Polarity == ZoneAltarPolarity.Negative));
            Assert.IsTrue(layout.Zones.All(e => e.Kind != ZoneEffectKind.Charge && e.Polarity != ZoneAltarPolarity.Neutral));
            Assert.IsTrue(layout.Zones.Where(e => e.Kind != ZoneEffectKind.Shrine).All(e => e.Lifetime == ZoneLifetimeMode.Cycling));
            Assert.IsTrue(layout.Zones.Where(e => e.Polarity == ZoneAltarPolarity.Negative).All(e => !e.HarmsEnemies));
            foreach (var group in layout.Zones.GroupBy(e => e.Kind == ZoneEffectKind.Shrine ? 0 : e.Polarity == ZoneAltarPolarity.Positive ? 1 : 2))
            {
                var counts = group.GroupBy(e => e.Id).Select(e => e.Count()).ToArray();
                Assert.LessOrEqual(counts.Max() - counts.Min(), 1);
            }
            foreach (var reference in presentation.AltarPresentation.GetReferencedContent())
                catalog.Registry.Get<SpriteDefinition>(reference.Id).RequireRole(SpriteRole.Prop);
        }

        [Test]
        public void RandomRadii_AreSeededAndSharedByClearanceContainmentAndStrikeCircles()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            var presentation = catalog.FieldEnvironmentPresentations[new ContentId("FIELD-007-ENVIRONMENT")];
            var layout = presentation.ZoneLayout;
            var screen = new Vector2(24, 14);
            var zones = ZoneLayoutGenerator.Generate(layout, presentation.ArenaSideLength.Value, Vector2.zero, null, 7, screen);
            var same = ZoneLayoutGenerator.Generate(layout, presentation.ArenaSideLength.Value, Vector2.zero, null, 7, screen);
            Assert.AreEqual(36, zones.Count);
            Assert.Greater(zones.Select(z => z.Radius).Distinct().Count(), 9);
            for (var i = 0; i < zones.Count; i++)
            {
                var zone = zones[i];
                Assert.AreEqual(zone.Effect.Radius / 3f, zone.Effect.MinRadius);
                Assert.That(zone.Radius, Is.InRange(zone.Effect.MinRadius, zone.Effect.Radius));
                Assert.AreEqual(zone.Radius, same[i].Radius);
                Assert.IsTrue(zone.Contains(zone.Center + Vector2.right * (zone.Radius - .001f)));
                Assert.IsFalse(zone.Contains(zone.Center + Vector2.right * (zone.Radius + .001f)));
                for (var j = i + 1; j < zones.Count; j++)
                    Assert.GreaterOrEqual(Vector2.Distance(zone.Center, zones[j].Center), zone.Radius + zones[j].Radius + layout.MinGap);
                if (zone.Effect.Kind != ZoneEffectKind.Strike) continue;
                for (var volley = 0; volley < 20; volley++)
                    for (var circle = 0; circle < zone.Effect.StrikeCount; circle++)
                        Assert.LessOrEqual(Vector2.Distance(zone.Center, zone.Effect.StrikeCenter(zone.Center, 7, i, volley, circle, zone.Radius)),
                            zone.Radius - zone.Effect.StrikeRadius + .001f);
            }
        }

        [Test]
        public void CompactArena_RandomLayoutsKeepAllAltarsAndSlidingScreenCap()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            var presentation = catalog.FieldEnvironmentPresentations[new ContentId("FIELD-007-ENVIRONMENT")];
            var screen = new Vector2(24, 14);
            var window = screen + Vector2.one * (2f * presentation.ZoneLayout.ScreenPadding);
            for (var seed = 0; seed < 32; seed++)
            {
                var zones = ZoneLayoutGenerator.Generate(presentation.ZoneLayout, presentation.ArenaSideLength.Value,
                    Vector2.zero, null, seed, screen);
                Assert.AreEqual(36, zones.Count, "Seed " + seed);
                foreach (var x in zones)
                    foreach (var y in zones)
                        Assert.LessOrEqual(zones.Count(zone => zone.Center.x >= x.Center.x &&
                            zone.Center.x <= x.Center.x + window.x && zone.Center.y >= y.Center.y &&
                            zone.Center.y <= y.Center.y + window.y), 3, "Sliding screen, seed " + seed);
            }
        }

        [Test]
        public void AltarState_GroundClockAndCrownLightFollowActivityAndPause()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            var presentation = catalog.FieldEnvironmentPresentations[new ContentId("FIELD-007-ENVIRONMENT")];
            var effect = presentation.ZoneLayout.Effects.Values.First(e => e.Kind == ZoneEffectKind.Haste);
            var root = new GameObject("Altar state test");
            try
            {
                var zone = new ZonePlacement(0, effect, Vector2.zero, 0f, effect.MinRadius);
                zone.SetNear(true);
                var view = root.AddComponent<AltarPresentationRuntime>();
                view.Initialize(zone, presentation.AltarPresentation, catalog.Registry);
                var restTime = (effect.PulseVisibleSeconds + effect.PulsePeriodSeconds) * .5f;
                view.Apply(restTime);
                Assert.IsFalse(view.IsActive); Assert.AreEqual(.5f, view.Progress, .001f);
                var light = root.GetComponentsInChildren<MeshRenderer>().Single();
                Assert.IsFalse(light.enabled);
                var ring = root.GetComponentsInChildren<LineRenderer>().Single(line => line.name == "FoundationState");
                Assert.AreEqual(presentation.AltarPresentation.StateRingRadius * presentation.AltarPresentation.StateRingAspect +
                    presentation.AltarPresentation.AltarContactOffsetY, ring.GetPosition(0).y, .001f);
                var activeTime = effect.PulsePeriodSeconds + effect.PulseVisibleSeconds * .5f;
                view.Apply(activeTime);
                Assert.IsTrue(view.IsActive); Assert.IsTrue(light.enabled);
                Assert.AreEqual(.5f, view.Progress, .001f);
                var mesh = light.GetComponent<MeshFilter>().sharedMesh;
                var alpha = mesh.colors[0].a;
                view.Apply(activeTime);
                Assert.AreEqual(alpha, mesh.colors[0].a, "Pause freezes transition flash.");
                view.Apply(activeTime + presentation.AltarPresentation.FlashSeconds);
                Assert.Less(mesh.colors[0].a, alpha, "Transition settles into steady light.");
                var bodyColor = view.Body.color;
                view.Apply(effect.PulsePeriodSeconds + restTime);
                Assert.AreEqual(bodyColor, view.Body.color, "Foundation polarity remains stable.");
                view.Apply(effect.PulsePeriodSeconds + restTime + presentation.AltarPresentation.FlashSeconds);
                Assert.IsFalse(light.enabled);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void AltarContact_BlocksOnlyPlayerAtFoundationAndIsIndependentOfEffectRadius()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            var profile = catalog.FieldEnvironmentPresentations[new ContentId("FIELD-007-ENVIRONMENT")].AltarPresentation;
            foreach (var shrine in new[] { false, true })
            {
                var root = new GameObject("Altar contact");
                try
                {
                    var collider = AltarObstacleFactory.Attach(root, profile, shrine);
                    Assert.IsFalse(collider.isTrigger);
                    Assert.AreEqual(shrine ? profile.ShrineContactRadius : profile.AltarContactRadius, collider.radius);
                    Assert.AreEqual(shrine ? profile.ShrineContactOffsetY : profile.AltarContactOffsetY, collider.offset.y);
                    Assert.AreEqual(~(1 << LayerMask.NameToLayer("Player")), collider.excludeLayers.value);
                    Assert.AreEqual(1, root.GetComponents<Collider2D>().Length);
                }
                finally { Object.DestroyImmediate(root); }
            }
        }

        [TestCase(false, true)]
        [TestCase(true, true)]
        [TestCase(false, false)]
        [TestCase(true, false)]
        public void AltarContact_WalkingBody_PlayerStopsAndEnemiesPass(bool shrine, bool isPlayer)
        {
            var previousMode = Physics2D.simulationMode;
            var previousScenes = EditorSceneManager.GetSceneManagerSetup();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Altar physical test");
            try
            {
                Physics2D.simulationMode = SimulationMode2D.Script;
                var catalog = RuntimeContentCatalog.CreateProduction();
                var profile = catalog.FieldEnvironmentPresentations[new ContentId("FIELD-007-ENVIRONMENT")].AltarPresentation;
                var contact = AltarObstacleFactory.Attach(root, profile, shrine);
                var mover = new GameObject("Walking body"); mover.transform.SetParent(root.transform, false);
                mover.layer = isPlayer ? LayerMask.NameToLayer("Player") : 0;
                mover.transform.localPosition = new Vector3(-2f, contact.offset.y, 0f);
                mover.AddComponent<CircleCollider2D>().radius = .2f;
                var body = mover.AddComponent<Rigidbody2D>();
                body.gravityScale = 0; body.constraints = RigidbodyConstraints2D.FreezeRotation;
                Physics2D.SyncTransforms();
                for (var i = 0; i < 60; i++)
                {
                    body.linearVelocity = Vector2.right * 4f;
                    Physics2D.Simulate(.02f);
                }
                if (isPlayer) Assert.LessOrEqual(body.position.x, -contact.radius - .2f + .025f,
                    "Continuous walking must stop before the foundation, allowing the physics contact tolerance.");
                else Assert.Greater(body.position.x, 2f, "Player-only contact does not obstruct enemies.");
            }
            finally
            {
                Object.DestroyImmediate(root);
                Physics2D.simulationMode = previousMode;
                if (previousScenes.Any(scene => scene.isLoaded))
                    EditorSceneManager.RestoreSceneManagerSetup(previousScenes);
            }
        }

        [Test]
        public void AltarView_NegativeAndShrine_UseOnlyPolaritySpritesAndPausedStateIsStable()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            var presentation = catalog.FieldEnvironmentPresentations[new ContentId("FIELD-007-ENVIRONMENT")];
            foreach (var effect in presentation.ZoneLayout.Effects.Values)
            {
                var obj = new GameObject("Altar test");
                try
                {
                    var zone = new ZonePlacement(0, effect, new Vector2(3, 4), 4f, effect.MinRadius);
                    zone.SetNear(true);
                    var view = obj.AddComponent<AltarPresentationRuntime>();
                    view.Initialize(zone, presentation.AltarPresentation, catalog.Registry);
                    view.Apply(0f);
                    var boundary = obj.GetComponentsInChildren<LineRenderer>().Single(line => line.name == "Boundary");
                    Assert.AreEqual(zone.Radius, boundary.GetPosition(0).y, .001f,
                        "Boundary uses the per-instance radius, not the definition's maximum.");
                    var expected = effect.Kind == ZoneEffectKind.Shrine ? presentation.AltarPresentation.Shrine :
                        effect.Polarity == ZoneAltarPolarity.Positive ? presentation.AltarPresentation.Positive : presentation.AltarPresentation.Negative;
                    Assert.AreSame(expected.Resolve(catalog.Registry).Sprite, view.Body.sprite);
                    Assert.AreEqual(0, obj.GetComponentsInChildren<Collider2D>().Length, "Presentation never blocks movement.");
                    zone.SetCharge(.5f); view.Apply(0f);
                    if (effect.Kind == ZoneEffectKind.Shrine) Assert.AreEqual(.5f, view.Progress);
                    var color = view.Body.color; var position = view.transform.position; var progress = view.Progress;
                    view.Apply(0f);
                    Assert.AreEqual(color, view.Body.color); Assert.AreEqual(position, view.transform.position); Assert.AreEqual(progress, view.Progress);
                    view.Shutdown();
                    Assert.IsNull(view.Body); Assert.AreEqual(0, obj.transform.childCount);
                    view.Initialize(zone, presentation.AltarPresentation, catalog.Registry);
                    zone.SetNear(false); view.Apply(0f); Assert.IsFalse(view.Body.enabled);
                }
                finally { Object.DestroyImmediate(obj); }
            }
        }
    }
}
