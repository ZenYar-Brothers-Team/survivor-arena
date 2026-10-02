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
        public IEnumerator Seals_RenderEightDifferentEffects_AndPreparationStates()
        {
            var effects = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")
                [new ContentId("FIELD-006-ENVIRONMENT")].ZoneLayout.Effects.Values
                .GroupBy(e => e.Kind).Select(g => g.First()).ToArray();
            Assert.AreEqual(8, effects.Length);
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
                    var zone = new ZonePlacement(i, effects[i], new Vector2(-24f + 16f * (i % 4), i < 4 ? 9f : -9f)); zone.SetNear(true);
                    foreach (var child in go.GetComponentsInChildren<Transform>()) child.gameObject.layer = 31;
                    zones.Add(zone); seals.Add(seal);
                }
                foreach (var sample in new[] { (time: 28f, name: "waiting"), (time: 2.5f, name: "preparing"), (time: 5f, name: "active") })
                {
                    for (var i = 0; i < seals.Count; i++)
                        seals[i].Apply(zones[i], effects[i].Kind == ZoneEffectKind.SpeedBurst && sample.name == "active" ? 4.15f : sample.time, 0f);
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
