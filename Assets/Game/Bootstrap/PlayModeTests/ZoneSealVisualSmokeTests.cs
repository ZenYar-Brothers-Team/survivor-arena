using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Presentation;
using Game.Zones;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Bootstrap.PlayModeTests
{
    /// <summary>Captures actual Unity seal rendering; preview layers are isolated from gameplay and destroyed afterwards.</summary>
    public sealed class ZoneSealVisualSmokeTests
    {
        [UnityTest]
        public IEnumerator RasterGlyphs_RenderApprovedPixelsBesideReference_ThroughAnimatedCycles()
        {
            Assert.AreNotEqual(UnityEngine.Rendering.GraphicsDeviceType.Null, SystemInfo.graphicsDeviceType,
                "This regression requires --graphics; renderer state alone cannot detect a wrong sampled texture.");
            var effects = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")
                [new ContentId("FIELD-006-ENVIRONMENT")].ZoneLayout.Effects;
            var profile = ZoneSealPresentationProfile.Load();
            var root = new GameObject("Raster texture regression");
            var target = new RenderTexture(512, 512, 24);
            var readback = new Texture2D(512, 512, TextureFormat.RGB24, false);
            var referenceMaterial = new Material(Shader.Find("Sprites/Default"));
            var referenceRimMaterial = new Material(Shader.Find("Sprites/Default")) { mainTexture = profile.RimSprite.texture };
            var previousTarget = RenderTexture.active;
            try
            {
                target.Create();
                var camera = root.AddComponent<Camera>(); camera.orthographic = true;
                camera.orthographicSize = 4f; camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
                camera.cullingMask = 1 << 31; camera.targetTexture = target;
                var actual = new GameObject("Actual seal"); actual.transform.SetParent(root.transform, false);
                var seal = actual.AddComponent<ZoneSealPresentationRuntime>();
                var referenceRoot = new GameObject("Reference"); referenceRoot.transform.SetParent(root.transform, false);
                referenceRoot.transform.position = new Vector3(3f, 0f, 0f);
                referenceRoot.transform.localScale = new Vector3(2f, 1.6f, 1f);
                var reference = new GameObject("Fixed approved raster").AddComponent<SpriteRenderer>();
                reference.transform.SetParent(referenceRoot.transform, false); reference.gameObject.layer = 31;
                reference.sharedMaterial = referenceMaterial;
                var referenceRim = new GameObject("Fixed approved rim").AddComponent<SpriteRenderer>();
                referenceRim.transform.SetParent(referenceRoot.transform, false); referenceRim.gameObject.layer = 31;
                referenceRim.sprite = profile.RimSprite; referenceRim.sharedMaterial = referenceRimMaterial;
                referenceRim.sortingOrder = profile.SortingOrder;
                var neighborEffect = effects.Values.First(e => e.Kind == ZoneEffectKind.ArcanePower);
                var neighbors = new ZoneSealPresentationRuntime[2];
                var neighborZones = new ZonePlacement[2];
                for (var i = 0; i < neighbors.Length; i++)
                {
                    var neighbor = new GameObject("Overlapping arcane motion " + i);
                    neighbor.transform.SetParent(root.transform, false);
                    neighbors[i] = neighbor.AddComponent<ZoneSealPresentationRuntime>();
                    neighbors[i].Initialize(neighborEffect.Kind, profile);
                    foreach (var child in neighbor.GetComponentsInChildren<Transform>()) child.gameObject.layer = 31;
                    neighborZones[i] = new ZonePlacement(100, neighborEffect, new Vector2(-1.5f + i * 6f, 0f), radius: 3f);
                    neighborZones[i].SetNear(true);
                }
                foreach (var suffix in new[] { "EXPERIENCE", "PORTAL", "KNOCKBACK" })
                {
                    var effect = effects[new ContentId("FIELD-006-ZONE-" + suffix)];
                    seal.Initialize(effect.Kind, profile);
                    // PlayMode destruction of the previous seal's children completes at frame end.
                    yield return null;
                    foreach (var child in actual.GetComponentsInChildren<Transform>()) child.gameObject.layer = 31;
                    reference.sprite = suffix == "EXPERIENCE" ? profile.ExperienceGlyphSprite :
                        suffix == "PORTAL" ? profile.PortalGlyphSprite : profile.KnockbackGlyphSprite;
                    referenceMaterial.mainTexture = reference.sprite.texture;
                    reference.transform.localScale = Vector3.one * profile.GlyphScale * profile.RasterGlyphDiameterFraction /
                        Mathf.Max(reference.sprite.bounds.size.x, reference.sprite.bounds.size.y);
                    var zone = new ZonePlacement(0, effect, new Vector2(-3f, 0f), radius: 2f); zone.SetNear(true);
                    // Show/hide, flash, faint warning, active light and multiple complete cycles change sprite batches.
                    foreach (var time in new[] { 0f, .2f, .8f, 2f, 4.9f, 5f, 5.15f, 5.4f, 6f, 12f, 18.9f,
                                 21f, 28f, 30f, 32f, 34.9f, 35.15f, 35.5f, 39f, 40f, 42f, 45.15f })
                    {
                        seal.Apply(zone, time, 0f);
                        var glyph = actual.transform.Find("Glyph/Approved glyph").GetComponent<SpriteRenderer>();
                        reference.enabled = seal.IsShowing; reference.color = glyph.color;
                        var rim = actual.transform.Find("Rim/Approved outline").GetComponent<SpriteRenderer>();
                        referenceRim.enabled = seal.IsShowing; referenceRim.color = rim.color;
                        referenceRim.transform.localScale = rim.transform.localScale;
                        referenceRim.transform.localRotation = rim.transform.parent.localRotation;
                        // The same neighboring arcs cross both centers. The fixed reference glyph is on top;
                        // the actual glyph must remain above other zones' animated decorations too.
                        for (var i = 0; i < neighbors.Length; i++)
                        {
                            neighbors[i].Apply(neighborZones[i], time, 0f);
                            foreach (var renderer in neighbors[i].GetComponentsInChildren<Renderer>())
                                if (renderer.gameObject.name != "Motion") renderer.enabled = false;
                        }
                        yield return null; yield return null;
                        RenderTexture.active = target;
                        readback.ReadPixels(new Rect(0, 0, 512, 512), 0, 0); readback.Apply();
                        var pixels = readback.GetPixels32(); var maximumDifference = 0;
                        // Equal world scale on the left/right; center crops exclude the rotating outer contour.
                        for (var y = 208; y < 304; y++)
                            for (var x = 16; x < 112; x++)
                            {
                                var a = pixels[y * 512 + x]; var b = pixels[y * 512 + x + 384];
                                maximumDifference = Mathf.Max(maximumDifference,
                                    Mathf.Abs(a.r - b.r), Mathf.Abs(a.g - b.g), Mathf.Abs(a.b - b.b));
                            }
                        if (maximumDifference > 2)
                            UiFoundationSmokeTests.Capture(target, "academy-raster-failure-" + suffix.ToLowerInvariant());
                        Assert.LessOrEqual(maximumDifference, 2, $"{suffix} at {time}s must keep approved pixels above neighboring arcs.");
                    }
                    UiFoundationSmokeTests.Capture(target, "academy-raster-reference-" + suffix.ToLowerInvariant());
                }
            }
            finally
            {
                RenderTexture.active = previousTarget; root.GetComponent<Camera>().targetTexture = null;
                Object.DestroyImmediate(root); Object.DestroyImmediate(referenceMaterial); Object.DestroyImmediate(referenceRimMaterial);
                Object.DestroyImmediate(readback);
                target.Release(); Object.DestroyImmediate(target);
            }
        }

        [UnityTest]
        public IEnumerator Seals_RenderAllNineAcademyEffects_AndPreparationStates()
        {
            var effects = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")
                [new ContentId("FIELD-006-ENVIRONMENT")].ZoneLayout.Effects.Values
                .GroupBy(e => e.Kind).Select(g => g.First()).ToArray();
            Assert.AreEqual(9, effects.Length);
            var root = new GameObject("Seal capture");
            var seals = new List<ZoneSealPresentationRuntime>();
            var zones = new List<ZonePlacement>();
            RenderTexture target = null;
            try
            {
                var camera = root.AddComponent<Camera>();
                camera.orthographic = true; camera.orthographicSize = 19f;
                camera.transform.position = new Vector3(0, 0, -10);
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.14f, .16f, .19f);
                camera.cullingMask = 1 << 31;
                if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
                { target = new RenderTexture(1600, 900, 24); target.Create(); camera.targetTexture = target; }
                var profile = ZoneSealPresentationProfile.Load();
                for (var i = 0; i < effects.Length; i++)
                {
                    var go = new GameObject(effects[i].Kind.ToString()); go.transform.SetParent(root.transform, false);
                    var seal = go.AddComponent<ZoneSealPresentationRuntime>(); seal.Initialize(effects[i].Kind, profile);
                    var zone = new ZonePlacement(i, effects[i], new Vector2(-20f + 20f * (i % 3), 12f - 12f * (i / 3))); zone.SetNear(true);
                    foreach (var child in go.GetComponentsInChildren<Transform>()) child.gameObject.layer = 31;
                    zones.Add(zone); seals.Add(seal);
                }
                foreach (var sample in new[] { (time: 28f, name: "waiting"), (time: 2.5f, name: "preparing"), (time: 5f, name: "active") })
                {
                    for (var i = 0; i < seals.Count; i++)
                        seals[i].Apply(zones[i], effects[i].Lifetime == ZoneLifetimeMode.Burst && sample.name == "active"
                            ? effects[i].TelegraphSeconds + .15f : sample.time, 0f);
                    yield return null; yield return null;
                    if (target != null) UiFoundationSmokeTests.Capture(target, "academy-seals-" + sample.name);
                }
                Assert.IsTrue(seals.All(s => s.IsShowing));
            }
            finally
            {
                root.GetComponent<Camera>().targetTexture = null;
                Object.DestroyImmediate(root);
                if (target != null) { target.Release(); Object.DestroyImmediate(target); }
            }
        }
    }
}
