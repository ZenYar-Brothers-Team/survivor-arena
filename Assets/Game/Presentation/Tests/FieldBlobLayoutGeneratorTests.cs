using System;
using System.Collections.Generic;
using System.Linq;
using Game.Presentation.Json;
using NUnit.Framework;
using UnityEngine;

namespace Game.Presentation.Tests
{
    /// <summary>Field geometry study: per-run blob layouts with a start-screen blob, clear start and minimum gaps.</summary>
    public sealed class FieldBlobLayoutGeneratorTests
    {
        private const float Side = 120f;

        private static FieldBlobLayoutData Data(int blobs = 8) => new FieldBlobLayoutData
        {
            EdgeMargin = 5f, StartClearRadius = 11f, PlayerClearRadius = 3.5f, MinGap = 7f, PlacementAttempts = 1500,
            MaxRestarts = 8, ReferenceSeed = 5, PixelsPerUnit = 16f, OutlinePixels = 2,
            StartScreen = new FieldBlobStartData { HalfWidth = 6.5f, HalfHeight = 4.4f, Reach = 4f, Radius = 6.48f,
                Styles = new[] { FieldBlobStyle.Round, FieldBlobStyle.Angular } },
            Styles = new Dictionary<string, FieldBlobStyleData>
            {
                ["Round"] = new FieldBlobStyleData { FillColor = "#5a6a5c", OutlineColor = "#1d1f23", StretchMin = .8f, StretchMax = 1.25f,
                    OutlineVertexCount = 36, HarmonicAmplitudes = new[] { .18f, .14f, .08f }, HarmonicScaleMin = .7f, HarmonicScaleMax = 1.3f },
                ["Angular"] = new FieldBlobStyleData { FillColor = "#5a5f66", OutlineColor = "#1d1f23", StretchMin = .7f, StretchMax = 1.3f,
                    MinVertices = 5, MaxVertices = 8, RadiusJitterMin = .65f, RadiusJitterMax = 1.1f, MinAspect = .45f },
                ["Linear"] = new FieldBlobStyleData { FillColor = "#6a5a5a", OutlineColor = "#1d1f23", LengthMin = 2f, LengthMax = 2.8f,
                    WidthMin = .28f, WidthMax = .42f, BendMax = .25f, Taper = .35f, SpineSamples = 24 }
            },
            Blobs = Enumerable.Range(0, blobs).Select(i => new FieldBlobEntryData
            {
                Style = (FieldBlobStyle)(i % 3), Radius = 8f - i * .5f
            }).ToArray()
        };

        private static FieldBlobLayoutDefinition Layout(int blobs = 8) => new FieldBlobLayoutDefinition(Data(blobs));

        private static IReadOnlyList<FieldObstacleShapeDefinition> Generate(FieldBlobLayoutDefinition layout, int seed) =>
            FieldBlobLayoutGenerator.Generate(layout, Side, Vector2.zero, seed, "FIXTURE-ENVIRONMENT");

        // Library mode: authored illustrations with fixed size and orientation, outlines around the sprite pivot.
        private static FieldBlobLayoutData LibraryData()
        {
            float[][] Box(float w, float h) => new[]
            {
                new[] { -w / 2, -h / 2 }, new[] { w / 2, -h / 2 }, new[] { w / 2, h / 2 }, new[] { -w / 2, h / 2 }
            };
            var data = Data(0);
            data.Styles = null;
            data.StartScreen = new FieldBlobStartData { HalfWidth = 6.5f, HalfHeight = 4.4f, Reach = 4f, LibraryIds = new[] { "mid-a", "mid-b" } };
            data.Library = new[]
            {
                new FieldBlobLibraryItemData { Id = "big", VisualId = "V-BIG", Points = Box(24f, 18f) },
                new FieldBlobLibraryItemData { Id = "tall", VisualId = "V-TALL", Points = Box(6f, 14f) },
                new FieldBlobLibraryItemData { Id = "mid-a", VisualId = "V-MID-A", Points = Box(14f, 6f) },
                new FieldBlobLibraryItemData { Id = "mid-b", VisualId = "V-MID-B", Points = Box(15f, 7f) },
                new FieldBlobLibraryItemData { Id = "small", VisualId = "V-SMALL", Points = Box(6f, 5f) }
            };
            data.Blobs = new[] { "big", "tall", "mid-a", "mid-b", "small" }.Select(id => new FieldBlobEntryData { LibraryId = id }).ToArray();
            return data;
        }

        [Test]
        public void Library_EveryItemAppearsOnce_AtItsAuthoredSizeAndOrientation_AndKeepsTheClearances()
        {
            var layout = new FieldBlobLayoutDefinition(LibraryData());
            Assert.AreEqual(5, layout.TotalCount, "The start-screen item is one of the listed items.");
            for (var seed = 0; seed < 60; seed++)
            {
                var shapes = Generate(layout, seed);
                CollectionAssert.AreEquivalent(new[] { "V-BIG", "V-TALL", "V-MID-A", "V-MID-B", "V-SMALL" },
                    shapes.Select(s => s.VisualId.ToString()), $"seed {seed}");
                foreach (var shape in shapes)
                {
                    var item = layout.Library.Values.Single(i => i.Visual.Id == shape.VisualId);
                    for (var i = 0; i < item.Points.Count; i++)
                        Assert.AreEqual(0f, (shape.Points[i] - shape.Origin - item.Points[i]).magnitude, 1e-3f,
                            $"seed {seed} {shape.Id}: no rotation or scaling");
                    Assert.IsTrue(shape.Points.All(p => Mathf.Abs(p.x) <= Side * .5f - layout.EdgeMargin + 1e-3f &&
                                                       Mathf.Abs(p.y) <= Side * .5f - layout.EdgeMargin + 1e-3f), $"seed {seed}");
                }
                for (var i = 1; i < shapes.Count; i++)
                    Assert.GreaterOrEqual(FieldBlobLayoutGenerator.DistanceToOutline(shapes[i].Points, Vector2.zero), layout.StartClearRadius - 1e-3f,
                        $"seed {seed} {shapes[i].Id} keeps the start circle clear");
                for (var i = 0; i < shapes.Count; i++)
                    for (var j = i + 1; j < shapes.Count; j++)
                        Assert.GreaterOrEqual(FieldBlobLayoutGenerator.PolygonDistance(shapes[i].Points, shapes[j].Points), layout.MinGap - 1e-3f,
                            $"seed {seed} {shapes[i].Id}/{shapes[j].Id}");
            }
        }

        [Test]
        public void Library_AMediumItemCoversPartOfTheStartScreen_ButNeverTheSpawn()
        {
            var layout = new FieldBlobLayoutDefinition(LibraryData());
            var screen = layout.StartScreen;
            var seen = new HashSet<string>();
            for (var seed = 0; seed < 60; seed++)
            {
                var blob = Generate(layout, seed)[0];
                seen.Add(blob.VisualId.ToString());
                Assert.IsTrue(blob.VisualId.ToString().StartsWith("V-MID"), $"seed {seed}");
                Assert.IsFalse(FieldBlobLayoutGenerator.Contains(blob.Points, Vector2.zero), $"seed {seed}");
                Assert.GreaterOrEqual(FieldBlobLayoutGenerator.DistanceToOutline(blob.Points, Vector2.zero), layout.PlayerClearRadius - 1e-3f);
                var covers = blob.Points.Any(p => Mathf.Abs(p.x) <= screen.HalfWidth && Mathf.Abs(p.y) <= screen.HalfHeight) ||
                             new[] { new Vector2(-screen.HalfWidth, -screen.HalfHeight), new Vector2(screen.HalfWidth, screen.HalfHeight) }
                                 .Any(c => FieldBlobLayoutGenerator.Contains(blob.Points, c));
                Assert.IsTrue(covers, $"seed {seed}");
            }
            CollectionAssert.AreEquivalent(new[] { "V-MID-A", "V-MID-B" }, seen, "Either medium item can open the run.");
        }

        [Test]
        public void Library_RejectsUnknownDuplicateAndMisplacedEntries()
        {
            var data = LibraryData();
            data.Blobs[0].LibraryId = "missing";
            Assert.Throws<ArgumentException>(() => new FieldBlobLayoutDefinition(data));
            data = LibraryData();
            data.Blobs[1].LibraryId = "big";
            Assert.Throws<ArgumentException>(() => new FieldBlobLayoutDefinition(data));
            data = LibraryData();
            data.Blobs[0].Radius = 3f;
            Assert.Throws<ArgumentException>(() => new FieldBlobLayoutDefinition(data));
            data = LibraryData();
            data.StartScreen.LibraryIds = new[] { "small", "tall" };
            data.Blobs = data.Blobs.Where(b => b.LibraryId != "tall").ToArray();
            Assert.Throws<ArgumentException>(() => new FieldBlobLayoutDefinition(data), "A start item must also be a listed blob.");
        }

        [Test]
        public void PolygonDistance_ReportsOverlapContainmentAndGap()
        {
            Vector2[] Square(float x, float y, float s) => new[] { new Vector2(x, y), new Vector2(x + s, y), new Vector2(x + s, y + s), new Vector2(x, y + s) };
            Assert.AreEqual(0f, FieldBlobLayoutGenerator.PolygonDistance(Square(0, 0, 4), Square(2, 2, 4)), "crossing");
            Assert.AreEqual(0f, FieldBlobLayoutGenerator.PolygonDistance(Square(0, 0, 10), Square(3, 3, 2)), "containment");
            Assert.AreEqual(3f, FieldBlobLayoutGenerator.PolygonDistance(Square(0, 0, 4), Square(7, 0, 4)), 1e-4f, "side by side");
            Assert.AreEqual(Mathf.Sqrt(18f), FieldBlobLayoutGenerator.PolygonDistance(Square(0, 0, 4), Square(7, 7, 4)), 1e-4f, "diagonal");
        }

        [Test]
        public void Generate_SameSeedSameLayout_DifferentSeedsDiffer()
        {
            var layout = Layout();
            var first = Generate(layout, 11).Select(s => (s.Center.x, s.Center.y, s.Points.Count)).ToList();
            CollectionAssert.AreEqual(first, Generate(layout, 11).Select(s => (s.Center.x, s.Center.y, s.Points.Count)).ToList());
            CollectionAssert.AreNotEqual(first, Generate(layout, 12).Select(s => (s.Center.x, s.Center.y, s.Points.Count)).ToList());
        }

        [Test]
        public void Generate_PlacesEveryBlob_InsideTheArena_WithGapsAndAClearStart()
        {
            var layout = Layout();
            for (var seed = 0; seed < 40; seed++)
            {
                var shapes = Generate(layout, seed);
                Assert.AreEqual(layout.TotalCount, shapes.Count, $"seed {seed}");
                var half = Side * .5f - layout.EdgeMargin;
                foreach (var shape in shapes)
                    foreach (var point in shape.Points)
                    {
                        Assert.LessOrEqual(Mathf.Abs(point.x), half + 1e-3f, $"seed {seed} {shape.Id}");
                        Assert.LessOrEqual(Mathf.Abs(point.y), half + 1e-3f, $"seed {seed} {shape.Id}");
                    }
                for (var i = 1; i < shapes.Count; i++)
                    Assert.GreaterOrEqual(shapes[i].Center.magnitude, layout.StartClearRadius + shapes[i].BoundingRadius - 1e-3f,
                        $"seed {seed} {shapes[i].Id} keeps the start circle clear");
                for (var i = 0; i < shapes.Count; i++)
                    for (var j = i + 1; j < shapes.Count; j++)
                        Assert.GreaterOrEqual(Vector2.Distance(shapes[i].Center, shapes[j].Center),
                            shapes[i].BoundingRadius + shapes[j].BoundingRadius + layout.MinGap - 1e-3f,
                            $"seed {seed} {shapes[i].Id}/{shapes[j].Id}");
            }
        }

        [Test]
        public void Generate_FirstBlobCoversPartOfTheStartScreen_ButNeverTheSpawn()
        {
            var layout = Layout();
            var screen = layout.StartScreen;
            for (var seed = 0; seed < 60; seed++)
            {
                var blob = Generate(layout, seed)[0];
                Assert.IsFalse(FieldBlobLayoutGenerator.Contains(blob.Points, Vector2.zero), $"seed {seed}");
                Assert.GreaterOrEqual(FieldBlobLayoutGenerator.DistanceToOutline(blob.Points, Vector2.zero), layout.PlayerClearRadius - 1e-3f,
                    $"seed {seed}");
                var overlaps = blob.Points.Any(p => Mathf.Abs(p.x) <= screen.HalfWidth && Mathf.Abs(p.y) <= screen.HalfHeight) ||
                               new[] { new Vector2(-screen.HalfWidth, -screen.HalfHeight), new Vector2(screen.HalfWidth, -screen.HalfHeight),
                                   new Vector2(screen.HalfWidth, screen.HalfHeight), new Vector2(-screen.HalfWidth, screen.HalfHeight) }
                                   .Any(c => FieldBlobLayoutGenerator.Contains(blob.Points, c));
                Assert.IsTrue(overlaps, $"seed {seed} start-screen blob must be visible at the start");
                Assert.AreNotEqual(FieldBlobStyle.Linear, blob.Style);
                var min = new Vector2(blob.Points.Min(p => p.x), blob.Points.Min(p => p.y));
                var max = new Vector2(blob.Points.Max(p => p.x), blob.Points.Max(p => p.y));
                Assert.IsFalse(min.x - layout.PlayerClearRadius < 0f && max.x + layout.PlayerClearRadius > 0f &&
                               min.y - layout.PlayerClearRadius < 0f && max.y + layout.PlayerClearRadius > 0f,
                    $"seed {seed}: the bounding box used by spawn placement keeps the spawn free");
            }
        }

        [Test]
        public void Generate_AngularBlobsAreConvexAndNotSlivers()
        {
            var style = Layout().Styles[FieldBlobStyle.Angular];
            var random = new System.Random(3);
            var built = 0;
            for (var i = 0; i < 200; i++)
            {
                var points = FieldBlobLayoutGenerator.Build(style, 5f, random);
                if (points == null) continue;
                built++;
                Assert.GreaterOrEqual(FieldBlobLayoutGenerator.Aspect(points), style.MinAspect - 1e-4f);
                for (var k = 0; k < points.Count; k++)
                {
                    var o = points[k];
                    var a = points[(k + 1) % points.Count];
                    var b = points[(k + 2) % points.Count];
                    Assert.Greater((a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x), 0f, "counter-clockwise convex turn");
                }
            }
            Assert.Greater(built, 100, "most angular draws are accepted");
        }

        [Test]
        public void Generate_LinearBlobIsALongThinBand()
        {
            var style = Layout().Styles[FieldBlobStyle.Linear];
            var points = FieldBlobLayoutGenerator.Build(style, 6f, new System.Random(1));
            Assert.AreEqual(style.SpineSamples * 2, points.Count);
            Assert.Less(FieldBlobLayoutGenerator.Aspect(points), .5f, "a band is much longer than wide");
        }

        [Test]
        public void Generate_ImpossibleLayout_Throws()
        {
            var data = Data(30);
            foreach (var blob in data.Blobs) blob.Radius = 20f;
            Assert.Throws<InvalidOperationException>(() => Generate(new FieldBlobLayoutDefinition(data), 1));
        }

        [Test]
        public void Definition_RejectsMissingStyleAndBadValues()
        {
            var data = Data();
            data.Styles.Remove("Linear");
            Assert.Throws<ArgumentException>(() => new FieldBlobLayoutDefinition(data));
            data = Data();
            data.Styles["Round"].FillColor = "not-a-color";
            Assert.Throws<ArgumentException>(() => new FieldBlobLayoutDefinition(data));
            data = Data();
            data.StartScreen.Styles = new[] { FieldBlobStyle.Linear };
            Assert.Throws<ArgumentException>(() => new FieldBlobLayoutDefinition(data));
            data = Data();
            data.Blobs = new FieldBlobEntryData[0];
            Assert.Throws<ArgumentException>(() => new FieldBlobLayoutDefinition(data));
            data = Data();
            data.MinGap = null;
            Assert.Throws<ArgumentException>(() => new FieldBlobLayoutDefinition(data));
        }

        [Test]
        public void ShapeSprite_PivotIsTheShapeCenter_AndCoversTheSilhouette()
        {
            var layout = Layout();
            var shape = Generate(layout, 4)[1];
            var style = layout.Styles[shape.Style];
            var sprite = FieldObstacleShapeSprite.Create(shape, style.Fill, style.Outline, layout.PixelsPerUnit, layout.OutlinePixels);
            try
            {
                var local = shape.Points.Select(p => p - shape.Center).ToList();
                Assert.AreEqual(local.Min(p => p.x), -sprite.pivot.x / sprite.pixelsPerUnit, 3f / layout.PixelsPerUnit);
                Assert.AreEqual(local.Min(p => p.y), -sprite.pivot.y / sprite.pixelsPerUnit, 3f / layout.PixelsPerUnit);
                var extent = new Vector2(local.Max(p => p.x) - local.Min(p => p.x), local.Max(p => p.y) - local.Min(p => p.y));
                Assert.AreEqual(extent.x, sprite.bounds.size.x, 6f / layout.PixelsPerUnit, "the sprite covers the silhouette plus padding");
                Assert.AreEqual(extent.y, sprite.bounds.size.y, 6f / layout.PixelsPerUnit);
                Assert.AreEqual(shape.Id, sprite.texture.name);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sprite.texture);
                UnityEngine.Object.DestroyImmediate(sprite);
            }
        }
    }
}
