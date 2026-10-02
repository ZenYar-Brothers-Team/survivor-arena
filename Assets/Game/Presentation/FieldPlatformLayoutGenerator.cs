using System;
using System.Collections.Generic;
using System.Linq;
using Game.Diagnostics;
using UnityEngine;
using Random = System.Random;

namespace Game.Presentation
{
    /// <summary>
    /// Per-run circular-platform network (docs/prototypes/field009-platforms): random non-overlapping round platforms, a short
    /// spanning tree of straight bridges plus extra bridges (loops), then every small dead end gets a second bridge or is
    /// removed. Only a platform of at least <c>DeadEndMinimumRadius</c> may keep a single bridge.
    /// </summary>
    public static class FieldPlatformLayoutGenerator
    {
        private readonly struct Edge
        {
            public readonly float Gap;
            public readonly int A;
            public readonly int B;
            public Edge(float gap, int a, int b) { Gap = gap; A = a; B = b; }
        }

        public static FieldPlatformLayout Generate(FieldPlatformLayoutDefinition profile, int seed)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            using var guard = PerfGuard.Measure("PlatformLayout.Generate", 200f);
            for (var attempt = 0; attempt < profile.LayoutAttempts; attempt++)
            {
                var random = new Random(unchecked(seed * 100 + attempt));
                var platforms = Place(profile, random);
                if (platforms.Count < 2) continue;
                var edges = Build(profile, platforms, random);
                var start = NearestToCenter(platforms);
                var alive = FixDeadEnds(profile, platforms, ref edges, start);
                var reached = Reach(platforms.Count, edges, start);
                var kept = Enumerable.Range(0, platforms.Count).Where(i => alive.Contains(i) && reached.Contains(i)).ToList();
                if (kept.Count < 2 || kept.Count < profile.KeptPlatformFraction * platforms.Count) continue;
                var map = new Dictionary<int, int>();
                foreach (var index in kept) map[index] = map.Count;
                // Each Book is a coin flip per platform; the start platform never holds one.
                var bookRandom = new Random(unchecked(seed * 100 + attempt + 7919));
                return new FieldPlatformLayout(profile, kept.Select(i => platforms[i].WithBook(i != start && bookRandom.NextDouble() < profile.BookChance)),
                    edges.Where(e => map.ContainsKey(e.A) && map.ContainsKey(e.B)).Select(e => new FieldPlatformBridge(map[e.A], map[e.B], e.Gap)),
                    map[start], seed);
            }
            throw new InvalidOperationException("Platform layout did not fit; check the platform profile.");
        }

        private static List<FieldPlatformDisc> Place(FieldPlatformLayoutDefinition p, Random random)
        {
            var result = new List<FieldPlatformDisc>();
            var half = p.ArenaSideLength * .5f;
            for (var tries = 0; tries < p.PlacementAttempts && result.Count < p.PlatformCount; tries++)
            {
                var radius = random.NextDouble() < p.GiantChance
                    ? Lerp(p.GiantRadiusMin, p.MaximumRadius, random)
                    : p.MinimumRadius + (p.GiantRadiusMin - p.MinimumRadius) * Mathf.Pow((float)random.NextDouble(), p.RadiusSkew);
                var reach = half - radius - p.EdgeMargin;
                var center = new Vector2(Lerp(-reach, reach, random), Lerp(-reach, reach, random));
                var fits = true;
                for (var i = 0; i < result.Count && fits; i++)
                    fits = Vector2.Distance(center, result[i].Center) >= radius + result[i].Radius + p.MinimumPlatformGap;
                if (fits) result.Add(new FieldPlatformDisc(center, radius));
            }
            return result;
        }

        private static float Lerp(float a, float b, Random random) => a + (b - a) * (float)random.NextDouble();

        private static float Gap(IReadOnlyList<FieldPlatformDisc> d, int i, int j) =>
            Vector2.Distance(d[i].Center, d[j].Center) - d[i].Radius - d[j].Radius;

        /// <summary>A bridge must not touch a third platform.</summary>
        private static bool Clear(FieldPlatformLayoutDefinition p, IReadOnlyList<FieldPlatformDisc> d, int i, int j)
        {
            var a = d[i].Center;
            var edge = d[j].Center - a;
            for (var k = 0; k < d.Count; k++)
            {
                if (k == i || k == j) continue;
                var t = Mathf.Clamp01(Vector2.Dot(d[k].Center - a, edge) / edge.sqrMagnitude);
                if (Vector2.Distance(d[k].Center, a + edge * t) < d[k].Radius + p.BridgeWidth * .5f + p.BridgeClearance) return false;
            }
            return true;
        }

        private static bool AngleOk(IReadOnlyList<FieldPlatformDisc> d, List<Edge> edges, int i, int j, float minimumDegrees)
        {
            foreach (var edge in edges)
                foreach (var (shared, fresh) in new[] { (i, j), (j, i) })
                {
                    if (edge.A != shared && edge.B != shared) continue;
                    var other = edge.A == shared ? edge.B : edge.A;
                    if (Vector2.Angle(d[fresh].Center - d[shared].Center, d[other].Center - d[shared].Center) < minimumDegrees)
                        return false;
                }
            return true;
        }

        private static List<Edge> Candidates(FieldPlatformLayoutDefinition p, IReadOnlyList<FieldPlatformDisc> d)
        {
            var result = new List<Edge>();
            for (var i = 0; i < d.Count; i++)
                for (var j = i + 1; j < d.Count; j++)
                {
                    var gap = Gap(d, i, j);
                    // Two too-small platforms never follow one another.
                    if (d[i].Radius < p.SmallRadiusMax && d[j].Radius < p.SmallRadiusMax) continue;
                    if (gap <= p.MaximumBridgeLength && Clear(p, d, i, j)) result.Add(new Edge(gap, i, j));
                }
            return result;
        }

        private static List<Edge> Build(FieldPlatformLayoutDefinition p, IReadOnlyList<FieldPlatformDisc> d, Random random)
        {
            var ranked = Candidates(p, d)
                .Select(e => (edge: e, key: e.Gap * Lerp(p.EdgeWeightJitterMin, p.EdgeWeightJitterMax, random)))
                .OrderBy(e => e.key).Select(e => e.edge).ToList();
            var parent = Enumerable.Range(0, d.Count).ToArray();
            int Find(int x) { while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; } return x; }
            var tree = new List<Edge>();
            foreach (var e in ranked)
            {
                var a = Find(e.A);
                var b = Find(e.B);
                if (a == b || !AngleOk(d, tree, e.A, e.B, p.TreeMinimumAngleDegrees)) continue;
                parent[a] = b;
                tree.Add(e);
            }
            var extra = new List<Edge>();
            foreach (var e in ranked)
                if (!tree.Contains(e) && AngleOk(d, tree.Concat(extra).ToList(), e.A, e.B, p.ExtraMinimumAngleDegrees)) extra.Add(e);
            // Fisher-Yates keeps the extra loops organic and seed-determined.
            for (var i = extra.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (extra[i], extra[j]) = (extra[j], extra[i]);
            }
            tree.AddRange(extra.Take(p.ExtraBridgeCount));
            return tree;
        }

        private static int NearestToCenter(IReadOnlyList<FieldPlatformDisc> d)
        {
            var best = 0;
            for (var i = 1; i < d.Count; i++)
                if (d[i].Center.sqrMagnitude < d[best].Center.sqrMagnitude) best = i;
            return best;
        }

        /// <summary>Gives every small dead end a second bridge, else deletes it; repeats until none is left.</summary>
        private static HashSet<int> FixDeadEnds(FieldPlatformLayoutDefinition p, IReadOnlyList<FieldPlatformDisc> d,
            ref List<Edge> edges, int start)
        {
            var current = edges;
            var alive = new HashSet<int>(Enumerable.Range(0, d.Count));
            var candidates = Candidates(p, d).OrderBy(e => e.Gap).ToList();
            while (true)
            {
                var degree = new int[d.Count];
                foreach (var e in current) { degree[e.A]++; degree[e.B]++; }
                var leaf = alive.Where(i => i != start && degree[i] <= 1 && d[i].Radius < p.DeadEndMinimumRadius)
                    .DefaultIfEmpty(-1).First();
                if (leaf < 0) break;
                var fix = candidates.Where(e => (e.A == leaf || e.B == leaf) && alive.Contains(e.A) && alive.Contains(e.B) &&
                                                !current.Any(x => x.A == e.A && x.B == e.B) &&
                                                AngleOk(d, current, e.A, e.B, p.ExtraMinimumAngleDegrees))
                    .Select(e => (Edge?)e).FirstOrDefault();
                if (fix.HasValue) { current.Add(fix.Value); continue; }
                alive.Remove(leaf);
                current = current.Where(e => e.A != leaf && e.B != leaf).ToList();
            }
            edges = current;
            return alive;
        }

        private static HashSet<int> Reach(int count, IReadOnlyList<Edge> edges, int start)
        {
            var seen = new HashSet<int> { start };
            var queue = new Queue<int>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                foreach (var e in edges)
                {
                    var v = e.A == u ? e.B : e.B == u ? e.A : -1;
                    if (v >= 0 && seen.Add(v)) queue.Enqueue(v);
                }
            }
            return seen;
        }
    }
}
