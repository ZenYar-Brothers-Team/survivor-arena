using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public sealed class FieldRoadLayoutTests
    {
        private static FieldEnvironmentPresentationDefinition Definition() =>
            FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")["FIELD-003-ENVIRONMENT"];
        [Test]
        public void ApprovedReferences_AllSixPreserveGeometryAndConnectedBooks()
        {
            var d = Definition();
            CollectionAssert.AreEqual(new[] {13,14,15,13,13,14}, d.RoadFallbackLayouts.Select(l => l.DeadEnds.Count));
            foreach (var layout in d.RoadFallbackLayouts) AssertConnected(layout);
            Assert.AreEqual(200, d.ArenaSideLength);
            Assert.IsNull(d.ObstacleLayout);
            Assert.AreEqual("FIELD-001-VISUAL-GROUND", d.Ground.Id.ToString());
        }
        [Test]
        public void ApprovedReferences_MatchTheCurrentGeneratorForTheirSeeds()
        {
            // Fallbacks are exported from this generator and profile; a rule change must regenerate them (revision 2026-10-02).
            var d = Definition();
            foreach (var reference in d.RoadFallbackLayouts)
            {
                var generated = FieldRoadLayoutGenerator.Generate(d.RoadLayout,d.RoadFallbackLayouts,reference.Seed);
                Assert.IsFalse(generated.UsedFallback, $"seed {reference.Seed}");
                AssertNear(generated.Roads.SelectMany(p => p), reference.Roads.SelectMany(p => p), reference.Seed);
                AssertNear(generated.DeadEnds.SelectMany(b => new[] {b.Entrance,b.EndCenter}),
                    reference.DeadEnds.SelectMany(b => new[] {b.Entrance,b.EndCenter}), reference.Seed);
            }
        }
        [Test]
        public void PerimeterRing_KeepsTwoUnitsOfGrassToTheBorder()
        {
            var d = Definition(); var p = d.RoadLayout;
            var layout = FieldRoadLayoutGenerator.Generate(p,d.RoadFallbackLayouts,42);
            var outer = layout.Roads[0].Max(q => Mathf.Max(Mathf.Abs(q.x),Mathf.Abs(q.y))) + p.MainRoadWidth*.5f;
            Assert.AreEqual(p.ArenaSideLength*.5f-2f, outer, 1e-3f, "User revision: ring pressed to the border with a 2-unit gap.");
            foreach (var branch in layout.DeadEnds)
                Assert.LessOrEqual(Mathf.Max(Mathf.Abs(branch.EndCenter.x),Mathf.Abs(branch.EndCenter.y))+p.DeadEndEndRadius,
                    p.ArenaSideLength*.5f-p.FieldPadding+1e-3f);
        }
        [Test]
        public void SeededGeneration_32SeedsConnectedDiverseAndBounded()
        {
            var d = Definition(); var fingerprints = new HashSet<string>(); var fallbackCount = 0;
            var watch = Stopwatch.StartNew();
            for (var seed = 0; seed < 32; seed++)
            {
                var before = watch.Elapsed.TotalMilliseconds;
                var layout = FieldRoadLayoutGenerator.Generate(d.RoadLayout,d.RoadFallbackLayouts,seed);
                if (layout.UsedFallback) fallbackCount++;
                else
                {
                    Assert.That(layout.LayoutAttempts, Is.InRange(1,d.RoadLayout.LayoutAttempts));
                    Assert.That(layout.GraphAttempts, Is.InRange(layout.LayoutAttempts,d.RoadLayout.GraphAttempts*d.RoadLayout.LayoutAttempts));
                }
                AssertConnected(layout);
                Assert.GreaterOrEqual(layout.DeadEnds.Count,10);
                fingerprints.Add(string.Join(";",layout.Roads.SelectMany(p => p).Select(p => p.ToString("F3"))));
                TestContext.WriteLine($"seed={seed} books={layout.DeadEnds.Count} graphs={layout.GraphAttempts} layouts={layout.LayoutAttempts} fallback={layout.UsedFallback} ms={watch.Elapsed.TotalMilliseconds-before:F1}");
            }
            Assert.LessOrEqual(fallbackCount, 3, "The generator must build networks rather than routinely selecting the six fallbacks.");
            Assert.GreaterOrEqual(fingerprints.Count,29);
            Assert.Less(watch.Elapsed.TotalSeconds,30, "32 complete layouts including connectivity validation should finish promptly.");
        }
        [Test]
        public void SeededGeneration_ReplayIsExact()
        {
            var d = Definition();
            var a = FieldRoadLayoutGenerator.Generate(d.RoadLayout,d.RoadFallbackLayouts,42);
            var b = FieldRoadLayoutGenerator.Generate(d.RoadLayout,d.RoadFallbackLayouts,42);
            CollectionAssert.AreEqual(a.Roads.SelectMany(p => p),b.Roads.SelectMany(p => p));
            CollectionAssert.AreEqual(a.DeadEnds.Select(p => p.EndCenter),b.DeadEnds.Select(p => p.EndCenter));
        }
        [TestCase(15f)]
        [TestCase(-12f)]
        [TestCase(0f)]
        public void SpawnPosition_NearestRoadAxis_UsesSegmentInteriorEvenWhenCenterIsGrass(float nearestAxisX)
        {
            var profile = Definition().RoadLayout;
            var roads = new[]
            {
                new[] { new Vector2(nearestAxisX,-60),new Vector2(nearestAxisX,60) },
                new[] { new Vector2(-30,-60),new Vector2(-30,60) },
                new[] { new Vector2(nearestAxisX,60),new Vector2(-30,60) }
            };
            var branches = Enumerable.Range(0,profile.MinimumDeadEnds).Select(i =>
            {
                var entrance = new Vector2(nearestAxisX,-54+i*12);
                return new FieldRoadDeadEnd(entrance,entrance+Vector2.right*35,37);
            });
            var layout = new FieldRoadLayout(profile,roads,branches,42,false);
            Assert.AreEqual(new Vector2(nearestAxisX,0),layout.SpawnPosition,
                "Start belongs on the closest centerline, rather than the grass origin or a road endpoint.");
            Assert.AreEqual(0,layout.MainDistance(layout.SpawnPosition));
            if (nearestAxisX != 0) Assert.Greater(layout.MainDistance(Vector2.zero),profile.MainRoadWidth*.5f,
                "This fixture's arena center must actually be on grass.");
        }
        private static void AssertNear(IEnumerable<Vector2> actual, IEnumerable<Vector2> expected, int seed)
        {
            var a = actual.ToArray(); var e = expected.ToArray();
            Assert.AreEqual(e.Length, a.Length, $"seed {seed}");
            for (var i = 0; i < a.Length; i++) Assert.Less(Vector2.Distance(a[i],e[i]), 1e-3f, $"seed {seed} point {i}");
        }
        private static void AssertConnected(FieldRoadLayout layout)
        {
            // Road corners of the shared distance field, 4-connected: the same surface the boundary and meshes are built from.
            var field = FieldRoadDistanceField.Walkable(layout); var n = field.Cells+1;
            int Index(Vector2 q) => Mathf.RoundToInt((q.y-field.Origin)/field.Step)*n+Mathf.RoundToInt((q.x-field.Origin)/field.Step);
            bool Road(int i) => field.Inside(i%n,i/n);
            var visited = new bool[n*n]; var queue = new Queue<int>();
            var first = Index(layout.SpawnPosition); Assert.IsTrue(Road(first)); visited[first] = true; queue.Enqueue(first);
            while (queue.Count > 0)
            {
                var i = queue.Dequeue();
                void Visit(int other)
                { if (other < 0 || other >= visited.Length || visited[other] || !Road(other)) return; visited[other] = true; queue.Enqueue(other); }
                if (i%n > 0) Visit(i-1); if (i%n < n-1) Visit(i+1); Visit(i-n); Visit(i+n);
            }
            foreach (var branch in layout.DeadEnds) Assert.IsTrue(visited[Index(branch.EndCenter)], "Book is unreachable from spawn.");
            for (var i = 0; i < visited.Length; i++) if (Road(i)) Assert.IsTrue(visited[i], "Disconnected walkable island.");
        }
    }
}
