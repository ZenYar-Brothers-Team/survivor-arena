using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Scatters a field's blob obstacles for one run: one blob partly covers the start screen without touching the
    /// player, the rest take random free spots with a clear start circle, edge margin and minimum gap. The same seed and
    /// arena always give the same layout. A seed that cannot fit every blob is retried with a derived seed.
    /// </summary>
    public static class FieldBlobLayoutGenerator
    {
        private const int RestartSeedStep = 7919;

        public static IReadOnlyList<FieldObstacleShapeDefinition> Generate(FieldBlobLayoutDefinition layout, float sideLength,
            Vector2 start, int seed, string idPrefix)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            NumericValidation.ValidatePositive(sideLength, nameof(sideLength));
            if (string.IsNullOrWhiteSpace(idPrefix)) throw new ArgumentException("Obstacle id prefix is required.", nameof(idPrefix));
            for (var restart = 0; restart <= layout.MaxRestarts; restart++)
            {
                var result = TryGenerate(layout, sideLength, start, unchecked(seed + restart * RestartSeedStep), idPrefix);
                if (result != null) return result;
            }
            throw new InvalidOperationException("Blob layout does not fit the arena; reduce blob sizes or count.");
        }

        private static IReadOnlyList<FieldObstacleShapeDefinition> TryGenerate(FieldBlobLayoutDefinition layout, float sideLength,
            Vector2 start, int seed, string idPrefix)
        {
            var random = new System.Random(seed);
            var half = sideLength * .5f - layout.EdgeMargin;
            var placed = new List<FieldObstacleShapeDefinition>();
            if (layout.StartScreen != null)
            {
                var startShape = PlaceStartBlob(layout, start, random, idPrefix);
                if (startShape == null) return null;
                placed.Add(startShape);
            }
            foreach (var entry in layout.Blobs)
            {
                FieldObstacleShapeDefinition shape = null;
                for (var attempt = 0; attempt < layout.PlacementAttempts && shape == null; attempt++)
                {
                    var points = Build(layout.Styles[entry.Style], entry.Radius, random);
                    if (points == null) continue;
                    var radius = Radius(points);
                    var lo = -half + radius;
                    var hi = half - radius;
                    if (hi < lo) continue;
                    var offset = new Vector2(Range(random, lo, hi), Range(random, lo, hi));
                    if ((start + offset).magnitude < layout.StartClearRadius + radius) continue;
                    var candidate = new FieldObstacleShapeDefinition($"{idPrefix}-B{placed.Count + 1:00}", entry.Style,
                        points.Select(point => point + offset));
                    if (placed.Any(other => Vector2.Distance(other.Center, candidate.Center) <
                                            other.BoundingRadius + candidate.BoundingRadius + layout.MinGap)) continue;
                    shape = candidate;
                }
                if (shape == null) return null;
                placed.Add(shape);
            }
            return placed.AsReadOnly();
        }

        private static FieldObstacleShapeDefinition PlaceStartBlob(FieldBlobLayoutDefinition layout, Vector2 start,
            System.Random random, string idPrefix)
        {
            var config = layout.StartScreen;
            var style = config.Styles[random.Next(config.Styles.Count)];
            var corners = new[]
            {
                new Vector2(-config.HalfWidth, -config.HalfHeight), new Vector2(config.HalfWidth, -config.HalfHeight),
                new Vector2(config.HalfWidth, config.HalfHeight), new Vector2(-config.HalfWidth, config.HalfHeight)
            };
            for (var attempt = 0; attempt < layout.PlacementAttempts * 5; attempt++)
            {
                var points = Build(layout.Styles[style], config.Radius, random);
                if (points == null) continue;
                var offset = new Vector2(Range(random, -config.HalfWidth - config.Reach, config.HalfWidth + config.Reach),
                    Range(random, -config.HalfHeight - config.Reach, config.HalfHeight + config.Reach));
                var world = points.Select(point => start + offset + point).ToList();
                if (Contains(world, start) || DistanceToOutline(world, start) < layout.PlayerClearRadius) continue;
                // Spawn placement treats obstacles as axis-aligned boxes, so the box (not only the outline) must keep the spawn free.
                var min = new Vector2(world.Min(p => p.x), world.Min(p => p.y)) - Vector2.one * layout.PlayerClearRadius;
                var max = new Vector2(world.Max(p => p.x), world.Max(p => p.y)) + Vector2.one * layout.PlayerClearRadius;
                if (start.x > min.x && start.x < max.x && start.y > min.y && start.y < max.y) continue;
                var covers = world.Any(point => Mathf.Abs(point.x - start.x) <= config.HalfWidth &&
                                                Mathf.Abs(point.y - start.y) <= config.HalfHeight) ||
                             corners.Any(corner => Contains(world, start + corner));
                if (!covers) continue;
                return new FieldObstacleShapeDefinition($"{idPrefix}-B01", style, world);
            }
            return null;
        }

        /// <summary>Silhouette points around (0, 0) for a style and radius; null when the draw is rejected.</summary>
        public static List<Vector2> Build(FieldBlobStyleDefinition style, float radius, System.Random random)
        {
            List<Vector2> points;
            switch (style.Style)
            {
                case FieldBlobStyle.Round: points = Round(style, radius, random); break;
                case FieldBlobStyle.Angular: points = Angular(style, radius, random); break;
                default: points = Linear(style, radius, random); break;
            }
            if (points == null || points.Count < 3) return null;
            var center = Vector2.zero;
            foreach (var point in points) center += point;
            center /= points.Count;
            for (var i = 0; i < points.Count; i++) points[i] -= center;
            return points;
        }

        private static List<Vector2> Round(FieldBlobStyleDefinition style, float radius, System.Random random)
        {
            var amplitudes = style.HarmonicAmplitudes;
            var scale = new float[3];
            var phase = new float[3];
            for (var i = 0; i < 3; i++)
            {
                scale[i] = Range(random, style.HarmonicScaleMin, style.HarmonicScaleMax);
                phase[i] = Range(random, 0f, 2f * Mathf.PI);
            }
            var sx = Range(random, style.StretchMin, style.StretchMax);
            var sy = Range(random, style.StretchMin, style.StretchMax);
            var points = new List<Vector2>(style.OutlineVertexCount);
            for (var i = 0; i < style.OutlineVertexCount; i++)
            {
                var angle = 2f * Mathf.PI * i / style.OutlineVertexCount;
                var factor = 1f + amplitudes[0] * Mathf.Sin(2f * angle + phase[0]) * scale[0] +
                             amplitudes[1] * Mathf.Sin(3f * angle + phase[1]) * scale[1] +
                             amplitudes[2] * Mathf.Sin(5f * angle + phase[2]) * scale[2];
                points.Add(new Vector2(radius * factor * Mathf.Cos(angle) * sx, radius * factor * Mathf.Sin(angle) * sy));
            }
            return points;
        }

        private static List<Vector2> Angular(FieldBlobStyleDefinition style, float radius, System.Random random)
        {
            var count = random.Next(style.MinVertices, style.MaxVertices + 1);
            var angles = new float[count];
            for (var i = 0; i < count; i++) angles[i] = Range(random, 0f, 2f * Mathf.PI);
            Array.Sort(angles);
            var sx = Range(random, style.StretchMin, style.StretchMax);
            var sy = Range(random, style.StretchMin, style.StretchMax);
            var raw = new List<Vector2>(count);
            foreach (var angle in angles)
            {
                var length = radius * Range(random, style.RadiusJitterMin, style.RadiusJitterMax);
                raw.Add(new Vector2(length * Mathf.Cos(angle) * sx, length * Mathf.Sin(angle) * sy));
            }
            var hull = ConvexHull(raw);
            if (hull.Count < 3) return null;
            // Reject slivers: sqrt of the covariance eigenvalue ratio is the aspect (1 = round, 0 = a line).
            return Aspect(hull) < style.MinAspect ? null : hull;
        }

        private static List<Vector2> Linear(FieldBlobStyleDefinition style, float radius, System.Random random)
        {
            var length = radius * Range(random, style.LengthMin, style.LengthMax);
            var width = radius * Range(random, style.WidthMin, style.WidthMax);
            var rotation = Range(random, 0f, Mathf.PI);
            var bend = Range(random, -style.BendMax, style.BendMax) * length;
            var top = new List<Vector2>(style.SpineSamples);
            var bottom = new List<Vector2>(style.SpineSamples);
            for (var i = 0; i < style.SpineSamples; i++)
            {
                var t = -1f + 2f * i / (style.SpineSamples - 1);
                var spine = new Vector2(t * length * .5f, bend * (1f - t * t));
                var tangent = new Vector2(length * .5f, -2f * bend * t).normalized;
                var normal = new Vector2(-tangent.y, tangent.x) * (width * (1f - style.Taper * t * t));
                top.Add(spine + normal);
                bottom.Add(spine - normal);
            }
            bottom.Reverse();
            top.AddRange(bottom);
            var cos = Mathf.Cos(rotation);
            var sin = Mathf.Sin(rotation);
            for (var i = 0; i < top.Count; i++) top[i] = new Vector2(top[i].x * cos - top[i].y * sin, top[i].x * sin + top[i].y * cos);
            return top;
        }

        /// <summary>Counter-clockwise convex hull (monotone chain).</summary>
        public static List<Vector2> ConvexHull(IEnumerable<Vector2> source)
        {
            var points = source.Distinct().OrderBy(p => p.x).ThenBy(p => p.y).ToList();
            if (points.Count < 3) return points;
            var hull = new List<Vector2>();
            foreach (var point in points)
            {
                while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], point) <= 0f) hull.RemoveAt(hull.Count - 1);
                hull.Add(point);
            }
            var lower = hull.Count + 1;
            for (var i = points.Count - 2; i >= 0; i--)
            {
                while (hull.Count >= lower && Cross(hull[hull.Count - 2], hull[hull.Count - 1], points[i]) <= 0f) hull.RemoveAt(hull.Count - 1);
                hull.Add(points[i]);
            }
            hull.RemoveAt(hull.Count - 1);
            return hull;
        }

        /// <summary>Ratio of the shorter to the longer principal axis of the point cloud, in [0, 1].</summary>
        public static float Aspect(IReadOnlyList<Vector2> points)
        {
            var mean = Vector2.zero;
            foreach (var point in points) mean += point;
            mean /= points.Count;
            float xx = 0f, xy = 0f, yy = 0f;
            foreach (var point in points)
            {
                var d = point - mean;
                xx += d.x * d.x; xy += d.x * d.y; yy += d.y * d.y;
            }
            var trace = xx + yy;
            var root = Mathf.Sqrt(Mathf.Max(0f, (xx - yy) * (xx - yy) * .25f + xy * xy));
            var major = trace * .5f + root;
            var minor = trace * .5f - root;
            return major <= 1e-6f ? 0f : Mathf.Sqrt(Mathf.Max(0f, minor) / major);
        }

        /// <summary>Even-odd point-in-polygon test.</summary>
        public static bool Contains(IReadOnlyList<Vector2> polygon, Vector2 point)
        {
            var inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                var a = polygon[i];
                var b = polygon[j];
                if ((a.y > point.y) != (b.y > point.y) &&
                    point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                    inside = !inside;
            }
            return inside;
        }

        /// <summary>Shortest distance from a point to the polygon outline.</summary>
        public static float DistanceToOutline(IReadOnlyList<Vector2> polygon, Vector2 point)
        {
            var best = float.MaxValue;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                var a = polygon[j];
                var edge = polygon[i] - a;
                var t = Mathf.Clamp01(Vector2.Dot(point - a, edge) / Mathf.Max(1e-9f, edge.sqrMagnitude));
                best = Mathf.Min(best, Vector2.Distance(point, a + edge * t));
            }
            return best;
        }

        private static float Radius(IReadOnlyList<Vector2> points) => points.Max(point => point.magnitude);

        private static float Cross(Vector2 o, Vector2 a, Vector2 b) => (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);

        private static float Range(System.Random random, float min, float max) =>
            max <= min ? min : min + (float)random.NextDouble() * (max - min);
    }
}
