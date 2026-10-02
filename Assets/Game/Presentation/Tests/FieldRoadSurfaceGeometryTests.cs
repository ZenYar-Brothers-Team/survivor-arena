using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public sealed class FieldRoadSurfaceGeometryTests
    {
        private const float Tolerance = .01f;

        private static IReadOnlyList<FieldRoadLayout> Layouts() =>
            FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")["FIELD-003-ENVIRONMENT"].RoadFallbackLayouts;

        [Test]
        public void Contours_FollowTheAnalyticOutline_WithoutCellSteps()
        {
            foreach (var layout in Layouts())
            {
                var contours = FieldRoadMarchingSquares.Contours(FieldRoadDistanceField.Walkable(layout), Tolerance);
                Assert.IsNotEmpty(contours);
                foreach (var contour in contours)
                {
                    Assert.AreEqual(contour[0], contour[contour.Length - 1], "Boundary loops are closed.");
                    for (var i = 1; i < contour.Length; i++)
                    {
                        // Cell steps deviate up to half a cell diagonal (~0.35); the interpolated edge stays on the outline.
                        // Where two roads meet, the cell containing the sharp grass corner is cut by a chord (slightly rounded).
                        var middle = (contour[i - 1] + contour[i]) * .5f;
                        foreach (var q in new[] { contour[i], middle })
                            Assert.Less(Mathf.Abs(Analytic(layout, q, true)), Corner(layout, q) ? .3f : .05f, $"seed {layout.Seed} point {q}");
                    }
                }
            }
        }

        [Test]
        public void RoundEnds_AreCircularAroundEachBook()
        {
            foreach (var layout in Layouts())
            {
                var p = layout.Profile;
                var points = FieldRoadMarchingSquares.Contours(FieldRoadDistanceField.Walkable(layout), Tolerance).SelectMany(c => c).ToArray();
                foreach (var branch in layout.DeadEnds)
                {
                    var axis = (branch.EndCenter - branch.Entrance).normalized;
                    // The far half of the round end is pure circle: no corridor or other road reaches it.
                    var rim = points.Where(q => Vector2.Distance(q, branch.EndCenter) < p.DeadEndEndRadius + 1f &&
                                                Vector2.Dot(q - branch.EndCenter, axis) > 0f).ToArray();
                    Assert.Greater(rim.Length, 20, $"seed {layout.Seed}: the far half circle keeps its samples.");
                    foreach (var q in rim) Assert.AreEqual(p.DeadEndEndRadius, Vector2.Distance(q, branch.EndCenter), .03f);
                }
            }
        }

        [Test]
        public void Fill_CoversExactlyTheAreaInsideTheContours()
        {
            var layout = Layouts()[0];
            var field = FieldRoadDistanceField.Walkable(layout);
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            FieldRoadMarchingSquares.Fill(field, vertices, triangles);
            var meshArea = 0f;
            for (var t = 0; t < triangles.Count; t += 3)
            {
                Vector2 a = vertices[triangles[t]], b = vertices[triangles[t + 1]], c = vertices[triangles[t + 2]];
                meshArea += Mathf.Abs((b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y)) * .5f;
                var centroid = (a + b + c) / 3f;
                Assert.Less(Analytic(layout, centroid, true), Corner(layout, centroid) ? .3f : .05f, "Every drawn triangle is road.");
            }
            // The walkable network is connected: one outer boundary, every other loop is a grass hole.
            var areas = FieldRoadMarchingSquares.Contours(field, Tolerance).Select(c => Mathf.Abs(Shoelace(c))).OrderByDescending(a => a).ToArray();
            var enclosed = areas[0] - areas.Skip(1).Sum();
            Assert.AreEqual(enclosed, meshArea, enclosed * .002f);
        }

        [Test]
        public void BranchField_IsDeadEndsClippedToTheMouthOverlapOfMainRoads()
        {
            var layout = Layouts()[0]; var p = layout.Profile;
            Assert.AreEqual(1f, p.DeadEndMouthOverlap, "User revision: the broken surface reaches 0.1H onto the main road.");
            var field = FieldRoadDistanceField.Branches(layout);
            for (var y = 0; y <= field.Cells; y += 3)
            for (var x = 0; x <= field.Cells; x += 3)
            {
                var q = field.Corner(x, y);
                var main = layout.MainDistance(q) - p.MainRoadWidth * .5f;
                var expected = Mathf.Max(FlatMouthBranches(layout, q), -(main + p.DeadEndMouthOverlap));
                if (Mathf.Abs(expected) > .01f) Assert.AreEqual(expected < 0f, field.Inside(x, y), $"corner {q}");
            }
            foreach (var branch in layout.DeadEnds)
            {
                // The main road keeps its junction: its axis at the mouth is main surface, the edge strip shows the entrance.
                var axis = (branch.EndCenter - branch.Entrance).normalized;
                Assert.Greater(Value(field, branch.Entrance), 0f, "Main road axis at a mouth stays main road.");
                var edge = branch.Entrance + axis * (p.MainRoadWidth * .5f / Mathf.Max(.5f, Vector2.Dot(axis, MainNormal(layout, branch.Entrance, axis))));
                Assert.Less(Value(field, edge - axis * (p.DeadEndMouthOverlap * .5f)), 0f, "The dead end starts slightly inside the main road.");
            }
        }

        // Dead ends whose corridor starts flat at the entrance (no cap behind it), plus round ends.
        private static float FlatMouthBranches(FieldRoadLayout layout, Vector2 q)
        {
            var p = layout.Profile; var best = float.PositiveInfinity;
            foreach (var branch in layout.DeadEnds)
            {
                var axis = (branch.EndCenter - branch.Entrance).normalized;
                var corridor = Mathf.Max(FieldRoadLayout.Distance(q, branch.Entrance, branch.EndCenter) - p.DeadEndWidth * .5f,
                    -Vector2.Dot(q - branch.Entrance, axis));
                best = Mathf.Min(best, Mathf.Min(corridor, Vector2.Distance(q, branch.EndCenter) - p.DeadEndEndRadius));
            }
            return best;
        }

        [Test]
        public void BranchField_NeverShowsOnTheFarSideOfTheMainRoad()
        {
            foreach (var layout in Layouts())
            {
                var p = layout.Profile; var field = FieldRoadDistanceField.Branches(layout);
                foreach (var branch in layout.DeadEnds)
                {
                    var axis = (branch.EndCenter - branch.Entrance).normalized;
                    // Behind the entrance, across the main road: main surface up to the far edge.
                    for (var t = .25f; t < p.MainRoadWidth * .5f; t += .25f)
                        Assert.Greater(Value(field, branch.Entrance - axis * t), 0f, $"seed {layout.Seed} behind mouth at {t}");
                }
            }
        }

        // Bilinear sample of the field at a world point.
        private static float Value(FieldRoadDistanceField field, Vector2 q)
        {
            var fx = (q.x - field.Origin) / field.Step; var fy = (q.y - field.Origin) / field.Step;
            var x = Mathf.FloorToInt(fx); var y = Mathf.FloorToInt(fy); var tx = fx - x; var ty = fy - y;
            return Mathf.Lerp(Mathf.Lerp(field[x, y], field[x + 1, y], tx), Mathf.Lerp(field[x, y + 1], field[x + 1, y + 1], tx), ty);
        }

        // Unit normal of the main road segment nearest to a point, oriented along the given direction.
        private static Vector2 MainNormal(FieldRoadLayout layout, Vector2 q, Vector2 direction)
        {
            var best = float.PositiveInfinity; var normal = direction;
            foreach (var path in layout.Roads)
                for (var i = 1; i < path.Count; i++)
                {
                    var d = FieldRoadLayout.Distance(q, path[i - 1], path[i]);
                    if (d >= best) continue;
                    best = d; var t = (path[i] - path[i - 1]).normalized; normal = new Vector2(-t.y, t.x);
                }
            return Vector2.Dot(normal, direction) < 0f ? -normal : normal;
        }

        // Near a junction: two distinct pieces (a whole road path, a corridor or a round end) both reach the point's vicinity.
        private static bool Corner(FieldRoadLayout layout, Vector2 q)
        {
            var p = layout.Profile; var near = 0;
            foreach (var path in layout.Roads)
            {
                var best = float.PositiveInfinity;
                for (var i = 1; i < path.Count; i++) best = Mathf.Min(best, FieldRoadLayout.Distance(q, path[i - 1], path[i]) - p.MainRoadWidth * .5f);
                if (best < .75f) near++;
            }
            foreach (var branch in layout.DeadEnds)
            {
                if (FieldRoadLayout.Distance(q, branch.Entrance, branch.EndCenter) - p.DeadEndWidth * .5f < .75f) near++;
                if (Vector2.Distance(q, branch.EndCenter) - p.DeadEndEndRadius < .75f) near++;
            }
            return near >= 2;
        }

        // Brute-force union of capsules, independent of the field's bounding-box painting.
        private static float Analytic(FieldRoadLayout layout, Vector2 q, bool includeMain)
        {
            var p = layout.Profile; var best = float.PositiveInfinity;
            if (includeMain)
                foreach (var path in layout.Roads)
                    for (var i = 1; i < path.Count; i++) best = Mathf.Min(best, FieldRoadLayout.Distance(q, path[i - 1], path[i]) - p.MainRoadWidth * .5f);
            foreach (var branch in layout.DeadEnds)
            {
                best = Mathf.Min(best, FieldRoadLayout.Distance(q, branch.Entrance, branch.EndCenter) - p.DeadEndWidth * .5f);
                best = Mathf.Min(best, Vector2.Distance(q, branch.EndCenter) - p.DeadEndEndRadius);
            }
            return best;
        }

        private static float Shoelace(Vector2[] loop)
        {
            var sum = 0f;
            for (var i = 1; i < loop.Length; i++) sum += loop[i - 1].x * loop[i].y - loop[i].x * loop[i - 1].y;
            return sum * .5f;
        }
    }
}
