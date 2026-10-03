using System.Linq;
using Game.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Game.Bootstrap.Tests
{
    public sealed class FieldPlatformSurfaceTests
    {
        [Test]
        public void HolyGroundSample_ContainsOnlyWarmGround_NoCoolPlatformFragments()
        {
            var registry = RuntimeContentCatalog.CreateProduction().Registry;
            var art = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")
                ["FIELD-009-ENVIRONMENT"].PlatformLayout.Art;
            var sprite = art.Visual.Resolve(registry).Sprite;
            var path = UnityEditor.AssetDatabase.GetAssetPath(sprite.texture);
            var readable = new Texture2D(2, 2);
            try
            {
                Assert.IsTrue(readable.LoadImage(System.IO.File.ReadAllBytes(path)));
                var pixels = readable.GetPixels32(); var b = art.GroundUvBounds;
                for (var y = Mathf.CeilToInt(b.yMin * readable.height); y < Mathf.FloorToInt(b.yMax * readable.height); y++)
                for (var x = Mathf.CeilToInt(b.xMin * readable.width); x < Mathf.FloorToInt(b.xMax * readable.width); x++)
                {
                    var pixel = pixels[y * readable.width + x];
                    Assert.Greater(pixel.r - pixel.b, 7,
                        $"Holy-ground sample contains a cool platform fragment at source ({x},{y}).");
                }
            }
            finally { Object.DestroyImmediate(readable); }
        }

        [Test]
        public void ApprovedArt_SharedWorldUvAndSampleBounds_NoPhysics_AndOwnedAssetsReleased()
        {
            var parent = new GameObject("Platform art test");
            var runtime = new FieldPlatformSurfaceRuntime();
            try
            {
                var registry = RuntimeContentCatalog.CreateProduction().Registry;
                var profile = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")
                    ["FIELD-009-ENVIRONMENT"].PlatformLayout;
                var sprite = profile.Art.Visual.Resolve(registry).Sprite;
                var layout = FieldPlatformLayoutGenerator.Generate(profile, 9);
                runtime.Initialize(layout, parent.transform, sprite);
                Assert.IsEmpty(parent.GetComponentsInChildren<Collider2D>());
                foreach (var name in new[] { "Platforms", "StartPlatform" })
                {
                    var surface = parent.transform.Find("PlatformNetwork/" + name);
                    var mesh = surface.GetComponent<MeshFilter>().sharedMesh;
                    var vertices = mesh.vertices; var uv = mesh.uv;
                    Assert.AreEqual(vertices.Length, uv.Length);
                    for (var i = 0; i < vertices.Length; i++)
                        Assert.Less(Vector2.Distance(uv[i], (Vector2)vertices[i] / profile.Art.SurfaceRepeat), 1e-5f);
                    var m = surface.GetComponent<MeshRenderer>().sharedMaterial;
                    Assert.AreSame(sprite.texture, m.mainTexture);
                    var r = profile.Art.SurfaceUvBounds;
                    Assert.AreEqual(new Vector4(r.xMin, r.yMin, r.xMax, r.yMax), m.GetVector("_UvBounds"));
                    Assert.That(mesh.colors.All(c => c == Color.white));
                }
                var groundMaterial = parent.transform.Find("PlatformNetwork/Void").GetComponent<MeshRenderer>().sharedMaterial;
                var bridgeMaterial = parent.transform.Find("PlatformNetwork/Bridges").GetComponent<MeshRenderer>().sharedMaterial;
                Assert.AreEqual(1f, bridgeMaterial.GetFloat("_BridgeVeil"));
                var bounds = profile.Art.GroundUvBounds;
                Assert.AreEqual(new Vector4(bounds.xMin, bounds.yMin, bounds.xMax, bounds.yMax), groundMaterial.GetVector("_UvBounds"));
                runtime.Dispose();
                Assert.IsTrue(groundMaterial == null);
                Assert.IsTrue(bridgeMaterial == null, "Private veil material is owned and released too.");
                Assert.NotNull(sprite.texture, "Shared approved texture survives disposal.");
                Assert.IsNull(parent.transform.Find("PlatformNetwork"));
                runtime.Initialize(layout, parent.transform, sprite);
                Assert.AreEqual(1, parent.transform.childCount, "Reinitialize has exactly one network.");
            }
            finally { runtime.Dispose(); Object.DestroyImmediate(parent); }
        }

        [Test]
        public void UnionRim_LeavesBridgeMouthOpen_AndFacesProjectDownScreen()
        {
            var parent = new GameObject("Platform union test");
            var runtime = new FieldPlatformSurfaceRuntime();
            try
            {
                var registry = RuntimeContentCatalog.CreateProduction().Registry;
                var p = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")
                    ["FIELD-009-ENVIRONMENT"].PlatformLayout;
                var layout = new FieldPlatformLayout(p,
                    new[] { new FieldPlatformDisc(new Vector2(-12, 0), 6), new FieldPlatformDisc(new Vector2(12, 0), 6) },
                    new[] { new FieldPlatformBridge(0, 1, 12) }, 0, 9);
                runtime.Initialize(layout, parent.transform, p.Art.Visual.Resolve(registry).Sprite);
                var rim = parent.transform.Find("PlatformNetwork/PlatformRim").GetComponent<MeshFilter>().sharedMesh.vertices;
                Assert.IsFalse(rim.Any(v => Mathf.Abs(v.x + 6) < .5f && Mathf.Abs(v.y) < 1f),
                    "No circular lip remains across the bridge mouth.");
                var faces = parent.transform.Find("PlatformNetwork/PlatformFaces").GetComponent<MeshFilter>().sharedMesh;
                var points = faces.vertices;
                for (var i = 0; i < points.Length; i += 4)
                {
                    Assert.Less(Vector3.Distance(points[i + 2] - points[i + 1], Vector3.down * p.Art.FaceHeight), 1e-5f);
                    Assert.Less(Vector3.Distance(points[i + 3] - points[i], Vector3.down * p.Art.FaceHeight), 1e-5f);
                }
                Assert.IsTrue(faces.colors.Any(c => c.a == 0f), "Rear faces do not extrude upward or across the surface.");
                Assert.IsTrue(layout.IsWalkable(new Vector2(0, 2)));
                Assert.IsFalse(layout.IsWalkable(new Vector2(0, 4)));
            }
            finally { runtime.Dispose(); Object.DestroyImmediate(parent); }
        }

        [TestCase(0f)]
        [TestCase(45f)]
        [TestCase(90f)]
        public void VeilBridge_LocalWeaveCoordinates_NoMasonryOutsidePlazas_ContinuousSafeWidth(float degrees)
        {
            var parent = new GameObject("Bridge veil test");
            var runtime = new FieldPlatformSurfaceRuntime();
            try
            {
                var registry = RuntimeContentCatalog.CreateProduction().Registry;
                var p = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")
                    ["FIELD-009-ENVIRONMENT"].PlatformLayout;
                var axis = new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad), Mathf.Sin(degrees * Mathf.Deg2Rad));
                var side = Vector2.Perpendicular(axis);
                var discs = new[] { new FieldPlatformDisc(-axis * 12f, 6f), new FieldPlatformDisc(axis * 12f, 6f) };
                var layout = new FieldPlatformLayout(p, discs, new[] { new FieldPlatformBridge(0, 1, 12) }, 0, 9);
                runtime.Initialize(layout, parent.transform, p.Art.Visual.Resolve(registry).Sprite);
                var surface = parent.transform.Find("PlatformNetwork/Bridges");
                var mesh = surface.GetComponent<MeshFilter>().sharedMesh;
                var half = p.BridgeWidth * .5f;
                var expectedUv = new[] { new Vector2(0, -half), new Vector2(24, -half),
                    new Vector2(24, half), new Vector2(0, half) };
                Assert.AreEqual(expectedUv.Length, mesh.uv.Length);
                for (var i = 0; i < expectedUv.Length; i++)
                    Assert.Less(Vector2.Distance(expectedUv[i], mesh.uv[i]), .00001f,
                        "Rotated bridge projection keeps local UV within floating-point precision.");
                var material = surface.GetComponent<MeshRenderer>().sharedMaterial;
                Assert.AreEqual(1f, material.GetFloat("_BridgeVeil"));
                Assert.AreEqual(p.Art.BridgeVeilColor.a, material.GetColor("_VeilColor").a);
                Assert.Less(material.GetColor("_VeilColor").a, .35f, "The golden earth remains visible through D.");
                Assert.AreEqual(new Vector4(p.Art.BridgeWeaveLength, p.Art.BridgeThreadWidth, p.Art.BridgeEdgeWidth, half), material.GetVector("_Weave"));
                foreach (var name in new[] { "PlatformFaces", "PlatformRim", "PlatformInlay" })
                {
                    var points = parent.transform.Find("PlatformNetwork/" + name).GetComponent<MeshFilter>().sharedMesh.vertices;
                    Assert.IsNotEmpty(points, "Plazas retain their existing masonry.");
                    for (var i = 0; i < points.Length; i += 4)
                    {
                        var midpoint = (Vector2)(points[i] + points[i + 1]) * .5f;
                        Assert.IsTrue(discs.Any(d => Vector2.Distance(midpoint, d.Center) <= d.Radius + p.Art.ContourStep + .001f),
                            "No thick rim, gold band or lower face along the exposed bridge.");
                    }
                }
                Assert.IsTrue(layout.IsWalkable(side * (half - .01f)), "Transparency is visual; the entire width is safe.");
                Assert.IsFalse(layout.IsWalkable(side * (half + .01f)));
                Assert.IsEmpty(parent.GetComponentsInChildren<Collider2D>());
            }
            finally { runtime.Dispose(); Object.DestroyImmediate(parent); }
        }

        [Test]
        public void VeilProfile_MissingOrNonFiniteRequiredSettings_RejectsInsteadOfDefaulting()
        {
            var data = new Game.Presentation.Json.FieldPlatformArtData {
                VisualId = "FIELD-009-MATERIAL-VISUAL-TILE", GroundUvBounds = new[] { 0f, 0f, 1f, 1f },
                SurfaceUvBounds = new[] { 0f, 0f, 1f, 1f }, GroundRepeat = 8, SurfaceRepeat = 2.4f,
                ContourStep = .2f, RimWidth = .18f, InlayWidth = .035f, FaceHeight = .32f,
                RimColor = "#b8b9c5", InlayColor = "#aa925e", FaceColor = "#626680"
            };
            Assert.IsFalse(new FieldPlatformArtDefinition(data).BridgeVeil, "Legacy profiles retain stone bridges.");
            data.BridgeVeil = true;
            Assert.Throws<System.ArgumentException>(() => new FieldPlatformArtDefinition(data));
            data.BridgeWeaveLength = float.NaN; data.BridgeThreadWidth = .024f; data.BridgeEdgeWidth = .045f;
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new FieldPlatformArtDefinition(data));
        }
    }
}
