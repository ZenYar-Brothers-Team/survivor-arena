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
