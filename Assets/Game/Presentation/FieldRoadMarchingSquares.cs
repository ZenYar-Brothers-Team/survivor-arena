using System;
using System.Collections.Generic;
using Game.Diagnostics;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Marching squares over a <see cref="FieldRoadDistanceField"/>: closed edge polylines and a filled mesh that share one
    /// zero crossing per grid edge, so the drawn road and the player boundary coincide. A saddle cell is resolved by the
    /// average of its four corners, identically for contours and fill.
    /// </summary>
    public static class FieldRoadMarchingSquares
    {
        // Corner k of a cell is (x + dx[k], y + dy[k]), counter-clockwise from bottom-left; edge k joins corner k and k+1.
        private static readonly int[] Dx = { 0, 1, 1, 0 };
        private static readonly int[] Dy = { 0, 0, 1, 1 };

        /// <summary>
        /// Closed boundary polylines (last point equals the first) between road and grass. Points are removed only where the
        /// outline stays within <paramref name="tolerance"/> world units of the simplified line, so arcs keep their samples.
        /// </summary>
        public static List<Vector2[]> Contours(FieldRoadDistanceField field, float tolerance)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            using var guard = PerfGuard.Measure("RoadSurface.Contours", 150f);
            var links = new Dictionary<int, int[]>();
            void Add(int from, int to)
            {
                if (!links.TryGetValue(from, out var ends)) { ends = new[] { -1, -1 }; links.Add(from, ends); }
                if (ends[0] < 0) ends[0] = to;
                else if (ends[1] < 0) ends[1] = to;
                else throw new InvalidOperationException("Road boundary edge is shared by more than two segments.");
            }
            var crossings = new int[4];
            for (var y = 0; y < field.Cells; y++)
            for (var x = 0; x < field.Cells; x++)
            {
                var count = Crossings(field, x, y, crossings);
                if (count == 2) { Add(crossings[0], crossings[1]); Add(crossings[1], crossings[0]); }
                else if (count == 4)
                    for (var k = 0; k < 4; k++)
                        if (Inside(field, x, y, k) != CenterInside(field, x, y))
                        {
                            // Cut off the corner that disagrees with the cell center: segment between its two edges.
                            var a = EdgeId(field, x, y, (k + 3) % 4); var b = EdgeId(field, x, y, k);
                            Add(a, b); Add(b, a);
                        }
            }

            var contours = new List<Vector2[]>();
            var points = new List<Vector2>();
            while (links.Count > 0)
            {
                using var keys = links.Keys.GetEnumerator(); keys.MoveNext();
                var start = keys.Current; var previous = -1; var current = start;
                points.Clear();
                do
                {
                    if (!links.TryGetValue(current, out var ends) || ends[1] < 0)
                        throw new InvalidOperationException("Open road boundary.");
                    var point = EdgePoint(field, current);
                    if (points.Count == 0 || (points[points.Count - 1] - point).sqrMagnitude > 1e-10f) points.Add(point);
                    links.Remove(current);
                    var next = ends[0] != previous ? ends[0] : ends[1];
                    previous = current; current = next;
                } while (current != start);
                if ((points[0] - points[points.Count - 1]).sqrMagnitude <= 1e-10f) points.RemoveAt(points.Count - 1);
                if (points.Count < 3) throw new InvalidOperationException("Degenerate road boundary.");
                contours.Add(Simplify(points, tolerance));
            }
            return contours;
        }

        /// <summary>Appends the road area as triangles: whole interior cells merged into row quads, boundary cells clipped.</summary>
        public static void Fill(FieldRoadDistanceField field, List<Vector3> vertices, List<int> triangles)
        {
            if (field == null || vertices == null || triangles == null) throw new ArgumentNullException(nameof(field));
            using var guard = PerfGuard.Measure("RoadSurface.Fill", 150f);
            var polygon = new List<Vector2>(6);
            var crossings = new int[4];
            for (var y = 0; y < field.Cells; y++)
            {
                var run = -1;
                for (var x = 0; x <= field.Cells; x++)
                {
                    var full = x < field.Cells && field.Inside(x, y) && field.Inside(x + 1, y) && field.Inside(x + 1, y + 1) && field.Inside(x, y + 1);
                    if (full) { if (run < 0) run = x; continue; }
                    if (run >= 0)
                    {
                        polygon.Clear();
                        polygon.Add(field.Corner(run, y)); polygon.Add(field.Corner(x, y));
                        polygon.Add(field.Corner(x, y + 1)); polygon.Add(field.Corner(run, y + 1));
                        Fan(polygon, vertices, triangles);
                        run = -1;
                    }
                    if (x == field.Cells) break;
                    var count = Crossings(field, x, y, crossings);
                    if (count == 0) continue; // all four corners on grass
                    if (count == 4 && !CenterInside(field, x, y))
                    {
                        // Separated saddle: one triangle for each road corner.
                        for (var k = 0; k < 4; k++)
                        {
                            if (!Inside(field, x, y, k)) continue;
                            polygon.Clear();
                            polygon.Add(EdgePoint(field, EdgeId(field, x, y, (k + 3) % 4)));
                            polygon.Add(field.Corner(x + Dx[k], y + Dy[k]));
                            polygon.Add(EdgePoint(field, EdgeId(field, x, y, k)));
                            Fan(polygon, vertices, triangles);
                        }
                        continue;
                    }
                    // Walk the cell counter-clockwise: road corners and the crossings between them form one convex polygon.
                    polygon.Clear();
                    for (var k = 0; k < 4; k++)
                    {
                        if (Inside(field, x, y, k)) polygon.Add(field.Corner(x + Dx[k], y + Dy[k]));
                        if (Inside(field, x, y, k) != Inside(field, x, y, (k + 1) % 4)) polygon.Add(EdgePoint(field, EdgeId(field, x, y, k)));
                    }
                    Fan(polygon, vertices, triangles);
                }
            }
        }

        private static void Fan(List<Vector2> polygon, List<Vector3> vertices, List<int> triangles)
        {
            var first = vertices.Count;
            foreach (var point in polygon) vertices.Add(point);
            for (var i = 1; i < polygon.Count - 1; i++) { triangles.Add(first); triangles.Add(first + i); triangles.Add(first + i + 1); }
        }

        private static bool Inside(FieldRoadDistanceField field, int x, int y, int corner) => field.Inside(x + Dx[corner], y + Dy[corner]);

        private static bool CenterInside(FieldRoadDistanceField field, int x, int y) =>
            field[x, y] + field[x + 1, y] + field[x + 1, y + 1] + field[x, y + 1] < 0f;

        private static int Crossings(FieldRoadDistanceField field, int x, int y, int[] result)
        {
            var count = 0;
            for (var k = 0; k < 4; k++)
                if (Inside(field, x, y, k) != Inside(field, x, y, (k + 1) % 4)) result[count++] = EdgeId(field, x, y, k);
            return count;
        }

        // Horizontal edge from corner (x, y) to (x + 1, y) is 2·i, vertical edge from (x, y) to (x, y + 1) is 2·i + 1.
        private static int EdgeId(FieldRoadDistanceField field, int x, int y, int edge)
        {
            var n = field.Cells + 1;
            switch (edge)
            {
                case 0: return (y * n + x) * 2;
                case 1: return (y * n + x + 1) * 2 + 1;
                case 2: return ((y + 1) * n + x) * 2;
                default: return (y * n + x) * 2 + 1;
            }
        }

        private static Vector2 EdgePoint(FieldRoadDistanceField field, int id)
        {
            var n = field.Cells + 1;
            var corner = id / 2; var x = corner % n; var y = corner / n;
            var ox = id % 2 == 0 ? 1 : 0; var oy = 1 - ox;
            var a = field[x, y]; var b = field[x + ox, y + oy];
            var t = Mathf.Clamp01(a / (a - b));
            return Vector2.Lerp(field.Corner(x, y), field.Corner(x + ox, y + oy), t);
        }

        // Greedy run extension on the closed loop; returns a closed polyline (first point repeated at the end).
        private static Vector2[] Simplify(List<Vector2> loop, float tolerance)
        {
            var result = new List<Vector2> { loop[0] };
            var anchor = 0; var count = loop.Count;
            for (var end = 2; end <= count; end++)
            {
                var target = loop[end % count];
                for (var k = anchor + 1; k < end; k++)
                {
                    if (FieldRoadLayout.Distance(loop[k], loop[anchor], target) <= tolerance) continue;
                    anchor = end - 1; result.Add(loop[anchor]);
                    break;
                }
            }
            result.Add(loop[0]);
            return result.ToArray();
        }
    }
}
