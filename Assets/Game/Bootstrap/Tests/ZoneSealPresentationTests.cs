using Game.Content;
using Game.Presentation;
using Game.Zones;
using Game.Zones.Json;
using NUnit.Framework;
using UnityEngine;

namespace Game.Bootstrap.Tests
{
    public sealed class ZoneSealPresentationTests
    {
        [Test]
        public void EnemyAdapter_HealsAndProtects_TeleportHasPerLifeCooldown_DisposeClearsBuffs()
        {
            var root = new GameObject("Enemy adapter test");
            try
            {
                var run = root.AddComponent<Game.Run.RunController>();
                if (!run.IsInitialized) run.Initialize(); run.Model.Start();
                var target = new GameObject("Target"); target.transform.SetParent(root.transform); target.transform.position = Vector2.right * 10f;
                var enemy = Game.Enemy.EnemyFactory.Spawn(new Game.Enemy.EnemyDefinition("T-ZONE-ENEMY", 100f, 1f, 3f, 0f, 1f),
                    Vector2.zero, target.transform, run, root.transform);
                enemy.TakeDamage(20f);
                using var source = new EnemyZoneSource(); source.Refresh();
                source.SetArea(0, .4f, .5f, 3f, .8f, .5f);
                Assert.AreEqual(81.5f, enemy.Health.CurrentHealth, .001f);
                Assert.AreEqual(.5f, enemy.ZoneInfluence.ActionBonus);
                Assert.AreEqual(2f, enemy.TakeDamage(10f), .001f);
                Assert.IsTrue(source.Teleport(0, Vector2.up * 10f, 3f, 1f));
                Assert.IsFalse(source.Teleport(0, Vector2.down * 10f, 3f, 2f));
                Assert.AreEqual(Vector2.up * 10f, enemy.Position);
                source.SpeedBurst(0, .6f, 8f); source.Refresh();
                Assert.AreEqual(0f, enemy.ZoneInfluence.ActionBonus);
                Assert.AreEqual(8f, enemy.ZoneInfluence.BuffRemaining);
                source.Dispose(); Assert.AreEqual(0f, enemy.ZoneInfluence.BuffRemaining);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void Portal_KeepsLargeEntryAreaAndUprightDoorway_IndependentOfRadius()
        {
            var fields = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation");
            var effect = fields[new ContentId("FIELD-006-ENVIRONMENT")].ZoneLayout.Effects[new ContentId("FIELD-006-ZONE-PORTAL")];
            var go = new GameObject("Portal size");
            try
            {
                var profile = ZoneSealPresentationProfile.Load();
                var seal = go.AddComponent<ZoneSealPresentationRuntime>(); seal.Initialize(effect.Kind, profile);
                foreach (var radius in new[] { effect.MinRadius, effect.Radius })
                {
                    var zone = new ZonePlacement(0, effect, Vector2.zero, radius: radius); zone.SetNear(true); seal.Apply(zone, 6f, 3f);
                    Assert.IsTrue(seal.IsActive, "A personal cooldown does not switch the shared doorway off for other actors.");
                    var portal = go.transform.Find("Portal");
                    Assert.AreEqual(profile.PortalHeight / profile.PortalSprite.bounds.size.y, portal.lossyScale.x, .001f);
                    Assert.AreEqual(portal.lossyScale.x, portal.lossyScale.y, .001f, "Doorway is never flattened onto the floor.");
                    Assert.IsTrue(portal.GetComponent<SpriteRenderer>().enabled);
                    Assert.AreEqual(new Vector3(radius, radius * .8f, 1f), go.transform.localScale);
                }
                seal.Shutdown(); Assert.AreEqual(0, go.transform.childCount);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void RasterRim_ScalesWithZone_KeepsReadableSymbolAndHidesOnWindowExit()
        {
            var effect = new ZoneEffectDefinition(new ZoneEffectData { Id = "T-ART", Kind = ZoneEffectKind.Haste,
                Radius = 6f, MinRadius = 2f, VerticalScale = .8f, Color = "#6df0c2", Lifetime = ZoneLifetimeMode.Pulsing,
                PulsePeriodSeconds = 30f, PulseVisibleSeconds = 20f, PulseFadeSeconds = 3f,
                PulsePrepareSeconds = 5f, PulseIdleVisibility = .14f, PlayerMovementBonus = .4f,
                RelocatesBetweenCycles = true });
            var zone = new ZonePlacement(0, effect, Vector2.zero, radius: 2f); zone.SetNear(true);
            var go = new GameObject("Approved rim");
            try
            {
                var profile = ZoneSealPresentationProfile.Load();
                var seal = go.AddComponent<ZoneSealPresentationRuntime>(); seal.Initialize(effect.Kind, profile);
                seal.Apply(zone, 5f, 0f);
                var rim = go.transform.Find("Rim"); var art = rim.GetComponentInChildren<SpriteRenderer>();
                Assert.AreSame(profile.RimSprite, art.sprite);
                Assert.IsTrue(art.enabled);
                Assert.AreEqual("Sprites/Default", art.sharedMaterial.shader.name, "Artwork is explicitly unlit in isolated review and gameplay.");
                Assert.IsFalse(rim.GetComponent<MeshRenderer>().enabled, "No smooth circular boundary is drawn over the art.");
                Assert.AreEqual(new Vector2(2f, 2f), (Vector2)art.sprite.bounds.size);
                var glyph = go.transform.Find("Glyph");
                Assert.IsTrue(glyph.GetComponent<MeshRenderer>().enabled);
                Assert.AreEqual(Vector3.one * profile.GlyphScale, glyph.localScale);
                Assert.AreEqual(Quaternion.identity, glyph.localRotation);
                foreach (var vertex in glyph.GetComponent<MeshFilter>().sharedMesh.vertices)
                    Assert.IsTrue(zone.Contains(glyph.TransformPoint(vertex)), "Enlarged symbol remains within the actual footprint.");
                zone.SetNear(false); seal.Apply(zone, 5f, 0f);
                Assert.IsFalse(art.enabled);
                Assert.IsFalse(glyph.GetComponent<MeshRenderer>().enabled);
                seal.Shutdown(); Assert.AreEqual(0, go.transform.childCount);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Seal_UsesOccurrenceRadiusForRenderingAndContainment()
        {
            var effect = new ZoneEffectDefinition(new ZoneEffectData { Id = "T-SMALL", Kind = ZoneEffectKind.Haste,
                Radius = 6f, MinRadius = 2f, VerticalScale = .8f, Color = "#6df0c2", Lifetime = ZoneLifetimeMode.Pulsing,
                PulsePeriodSeconds = 30f, PulseVisibleSeconds = 20f, PulseFadeSeconds = 3f,
                PulsePrepareSeconds = 5f, PulseIdleVisibility = .14f, PlayerMovementBonus = .4f });
            var zone = new ZonePlacement(0, effect, Vector2.zero, radius: 2f); zone.SetNear(true);
            var go = new GameObject("Small seal");
            try
            {
                var seal = go.AddComponent<ZoneSealPresentationRuntime>(); seal.Initialize(effect.Kind, ZoneSealPresentationProfile.Load());
                seal.Apply(zone, 5f, 0f);
                Assert.AreEqual(new Vector3(2f, 1.6f, 1f), go.transform.localScale);
                var rim = go.transform.Find("Rim");
                foreach (var vertex in rim.GetComponent<MeshFilter>().sharedMesh.vertices)
                    Assert.IsTrue(zone.Contains(rim.TransformPoint(vertex)));
                Assert.IsFalse(zone.Contains(Vector2.right * 2.01f));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void OneShotFlash_BurstFiringAndPortalUseBrightenSeal_ThenFadeWithoutMovingBoundary()
        {
            var fields = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation");
            var effects = fields[new ContentId("FIELD-006-ENVIRONMENT")].ZoneLayout.Effects;
            foreach (var id in new[] { "FIELD-006-ZONE-SPEED-BURST", "FIELD-006-ZONE-PORTAL" })
            {
                var effect = effects[new ContentId(id)];
                var zone = new ZonePlacement(0, effect, Vector2.zero); zone.SetNear(true);
                var go = new GameObject("Flash test");
                try
                {
                    var profile = ZoneSealPresentationProfile.Load();
                    var seal = go.AddComponent<ZoneSealPresentationRuntime>(); seal.Initialize(effect.Kind, profile);
                    var fire = effect.Kind == ZoneEffectKind.SpeedBurst ? effect.TelegraphSeconds : 6f;
                    var cooldown = effect.Kind == ZoneEffectKind.Portal ? 3f : 0f;
                    var rim = go.transform.Find("Rim").GetComponent<MeshRenderer>();
                    var block = new MaterialPropertyBlock();
                    seal.Apply(zone, fire + profile.ApplicationFlashSeconds, cooldown);
                    rim.GetPropertyBlock(block); var faded = block.GetColor("_Color");
                    if (effect.Kind == ZoneEffectKind.Portal) zone.RecordApplication(fire);
                    seal.Apply(zone, fire, cooldown);
                    rim.GetPropertyBlock(block); var lit = block.GetColor("_Color");
                    Assert.Greater(lit.r + lit.g + lit.b, faded.r + faded.g + faded.b, "Only firing/use produces the bright flash.");
                    Assert.AreEqual(new Vector3(effect.Radius, effect.Radius * .8f, 1f), go.transform.localScale);
                    seal.Apply(zone, fire, cooldown); rim.GetPropertyBlock(block);
                    Assert.AreEqual(lit, block.GetColor("_Color"), "Paused run time holds flash intensity.");
                    seal.Apply(zone, fire + profile.ApplicationFlashSeconds, cooldown); rim.GetPropertyBlock(block);
                    Assert.AreEqual(faded, block.GetColor("_Color"), "The flash ends after its configured duration.");
                }
                finally { Object.DestroyImmediate(go); }
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Rim_RotatesOnlyForRelocatingZones_KeepsEllipseAndFreezesWithClock(bool relocates)
        {
            var effect = new ZoneEffectDefinition(new ZoneEffectData { Id = "T-RIM", Kind = ZoneEffectKind.Haste,
                Radius = 6f, VerticalScale = .8f, Color = "#6df0c2", Lifetime = ZoneLifetimeMode.Pulsing,
                PulsePeriodSeconds = 30f, PulseVisibleSeconds = 20f, PulseFadeSeconds = 3f,
                PulsePrepareSeconds = 5f, PulseIdleVisibility = .14f, PlayerMovementBonus = .4f,
                RelocatesBetweenCycles = relocates });
            var zone = new ZonePlacement(0, effect, Vector2.zero); zone.SetNear(true);
            var go = new GameObject("Rim test");
            try
            {
                var profile = ZoneSealPresentationProfile.Load();
                var seal = go.AddComponent<ZoneSealPresentationRuntime>(); seal.Initialize(effect.Kind, profile);
                var rim = go.transform.Find("Rim");
                foreach (var time in new[] { 2.5f, 5f, 25f })
                {
                    seal.Apply(zone, time, 0f);
                    Assert.Less(Quaternion.Angle(Quaternion.Euler(0f, 0f,
                        relocates ? time * profile.RelocatingRimDegreesPerSecond : 0f), rim.localRotation), .01f);
                    foreach (var vertex in rim.GetComponent<MeshFilter>().sharedMesh.vertices)
                        Assert.IsTrue(effect.Contains(zone.Center, rim.TransformPoint(vertex)), "Rim stays in the gameplay ellipse.");
                    var pose = rim.localRotation;
                    seal.Apply(zone, time, 0f);
                    Assert.AreEqual(pose, rim.localRotation, "Pause holds the contour.");
                    Assert.AreEqual(Quaternion.identity, go.transform.Find("Glyph").localRotation, "The central icon remains readable.");
                }
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Seals_AllEightGlyphs_HaveGeometryWithinTheRealRadius()
        {
            var builder = new ZoneSealMeshBuilder(ZoneSealPresentationProfile.Load().StrokeFraction);
            foreach (var kind in new[] { ZoneEffectKind.Slow, ZoneEffectKind.Haste, ZoneEffectKind.Regeneration,
                ZoneEffectKind.ArcanePower, ZoneEffectKind.Rift, ZoneEffectKind.Portal, ZoneEffectKind.SpeedBurst, ZoneEffectKind.Protection })
            {
                foreach (var mesh in new[] { builder.Boundary(kind), builder.Glyph(kind), builder.Motion(kind, .64f) })
                {
                    try
                    {
                        Assert.Greater(mesh.vertexCount, 0, kind.ToString());
                        foreach (var vertex in mesh.vertices) Assert.LessOrEqual(vertex.magnitude, 1f, kind.ToString());
                    }
                    finally { Object.DestroyImmediate(mesh); }
                }
            }
        }

        [Test]
        public void Seal_StopsWhileRestingAndOutsideWindow_RepeatedTimeFreezesAndShutdownClears()
        {
            var effect = new ZoneEffectDefinition(new ZoneEffectData { Id = "T-SEAL", Kind = ZoneEffectKind.Haste,
                Radius = 6f, VerticalScale = .8f, Color = "#6df0c2", Lifetime = ZoneLifetimeMode.Pulsing, PulsePeriodSeconds = 30f,
                PulseVisibleSeconds = 20f, PulseFadeSeconds = 3f, PulsePrepareSeconds = 5f, PulseIdleVisibility = .14f,
                PlayerMovementBonus = .4f });
            var zone = new ZonePlacement(0, effect, new Vector2(10f, 10f)); zone.SetNear(true);
            var go = new GameObject("Seal test");
            try
            {
                var seal = go.AddComponent<ZoneSealPresentationRuntime>(); seal.Initialize(effect.Kind, ZoneSealPresentationProfile.Load());
                seal.Apply(zone, 2.5f, 0f);
                Assert.IsTrue(seal.IsShowing); Assert.IsFalse(seal.IsActive);
                seal.Apply(zone, 5f, 0f); Assert.IsTrue(seal.IsActive);
                Assert.AreEqual(new Vector3(6f, 4.8f, 1f), go.transform.localScale);
                var motion = go.transform.Find("Motion"); var pose = motion.localRotation;
                foreach (var vertex in motion.GetComponent<MeshFilter>().sharedMesh.vertices)
                {
                    var world = motion.TransformPoint(vertex);
                    Assert.IsTrue(effect.Contains(zone.Center, world), "Rotating ink stays within the ground ellipse.");
                }
                seal.Apply(zone, 5f, 0f); Assert.AreEqual(pose, motion.localRotation, "Pause holds the same run time.");
                seal.Apply(zone, 25f, 0f); Assert.IsTrue(seal.IsShowing); Assert.IsFalse(seal.IsActive);
                Assert.IsFalse(motion.GetComponent<MeshRenderer>().enabled, "Waiting seal has no moving layer.");
                zone.SetNear(false); seal.Apply(zone, 5f, 0f);
                Assert.IsFalse(seal.IsShowing);
                foreach (var renderer in go.GetComponentsInChildren<MeshRenderer>()) Assert.IsFalse(renderer.enabled);
                seal.Shutdown(); Assert.AreEqual(0, go.transform.childCount);
                seal.Initialize(effect.Kind, ZoneSealPresentationProfile.Load());
                Assert.AreEqual(3, go.transform.childCount, "Reinitialization creates one set of layers.");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void ProductionStudy_AllPulsingZonesPrepare_AltarCyclesKeepLegacyTiming()
        {
            var fields = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation");
            foreach (var effect in fields[new ContentId("FIELD-DEV-ZONES-ENVIRONMENT")].ZoneLayout.Effects.Values)
            {
                Assert.AreEqual(.8f, effect.VerticalScale);
                if (effect.Lifetime == ZoneLifetimeMode.Pulsing)
                { Assert.AreEqual(5f, effect.PulsePrepareSeconds); Assert.AreEqual(.14f, effect.PulseIdleVisibility); }
            }
            foreach (var effect in fields[new ContentId("FIELD-DEV-ALTARS-ENVIRONMENT")].ZoneLayout.Effects.Values)
            { Assert.IsFalse(effect.HasPreparation); Assert.AreEqual(1f, effect.VerticalScale); }
        }
    }
}
