using System;
using System.Collections.Generic;
using System.Linq;
using Game.Diagnostics;
using UnityEngine;
using Random = System.Random;

namespace Game.Presentation
{
    /// <summary>Bounded seeded sparse graph construction: MST, long-cycle cross links, ring ports and isolated book branches.</summary>
    public static class FieldRoadLayoutGenerator
    {
        public static FieldRoadLayout Generate(FieldRoadLayoutDefinition p, IReadOnlyList<FieldRoadLayout> fallbacks, int seed)
        {
            if (p == null || fallbacks == null || fallbacks.Count == 0) throw new ArgumentException("Road profile and validated fallbacks required.");
            using var guard = PerfGuard.Measure("RoadLayout.Generate", 500f);
            var rng = new Random(seed);
            for (var attempt = 0; attempt < p.LayoutAttempts; attempt++)
            {
                var roads = MakeRoads(p,rng);
                if (roads == null) continue;
                var branches = MakeBranches(p,rng,roads);
                if (branches.Count >= p.MinimumDeadEnds) return new FieldRoadLayout(p,roads,branches,seed,false);
            }
            // Approved geometry is a rare bounded-budget fallback, never the primary randomizer.
            var fallback = fallbacks[rng.Next(fallbacks.Count)];
            Debug.LogWarning($"FIELD-003 seed {seed}: bounded generation exhausted; using approved fallback {fallback.Seed}.");
            return new FieldRoadLayout(p,fallback.Roads,fallback.DeadEnds,seed,true);
        }

        private static List<Vector2[]> MakeRoads(FieldRoadLayoutDefinition p, Random rng)
        {
            var half = p.ArenaSideLength*.5f;
            var bound = half-p.RingInset;
            var ring = new List<Vector2>();
            for (var corner = 0; corner < 4; corner++)
            {
                var c = bound-p.RingCornerRadius;
                var center = new Vector2(corner < 2 ? c : -c, corner == 0 || corner == 3 ? -c : c);
                for (var sample = 0; sample < p.RingArcSamples; sample++)
                {
                    var angle = (-90 + corner*90 + sample*90f/(p.RingArcSamples-1))*Mathf.Deg2Rad;
                    ring.Add(center + new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*p.RingCornerRadius);
                }
            }
            ring.Add(ring[0]);
            for (var retry = 0; retry < p.GraphAttempts; retry++)
            {
                var count = rng.Next(p.InteriorNodeCountMin,p.InteriorNodeCountMax+1);
                var points = new List<Vector2>();
                for (var attempt = 0; attempt < p.NodePlacementAttempts && points.Count < count; attempt++)
                {
                    var q = new Vector2(Range(rng,-half+p.InteriorMargin,half-p.InteriorMargin),Range(rng,-half+p.InteriorMargin,half-p.InteriorMargin));
                    if (points.All(n => Vector2.Distance(n,q) > p.InteriorNodeGap)) points.Add(q);
                }
                if (points.Count != count) continue;
                var edges = new List<(int a,int b)>();
                var used = new bool[count]; used[0] = true;
                // Prim's tree; complete graph is tiny (13-18 nodes), no external package.
                while (edges.Count < count-1)
                {
                    var best = float.PositiveInfinity; var a = -1; var b = -1;
                    for (var i = 0; i < count; i++) if (used[i])
                        for (var j = 0; j < count; j++) if (!used[j] && (points[i]-points[j]).sqrMagnitude < best)
                        { best = (points[i]-points[j]).sqrMagnitude; a = i; b = j; }
                    edges.Add((a,b)); used[b] = true;
                }
                List<int> Neighbors(int i) => edges.Where(e => e.a == i || e.b == i).Select(e => e.a == i ? e.b : e.a).ToList();
                bool AngleClear(int i, Vector2 destination) => Neighbors(i).All(j => Vector2.Angle(destination-points[i],points[j]-points[i]) >= p.MinimumJunctionAngleDegrees);
                var valid = true;
                for (var i = 0; i < count && valid; i++)
                {
                    var neighbors = Neighbors(i);
                    for (var j = 0; j < neighbors.Count; j++) for (var k = j+1; k < neighbors.Count; k++)
                        if (Vector2.Angle(points[neighbors[j]]-points[i],points[neighbors[k]]-points[i]) < p.MinimumJunctionAngleDegrees) valid = false;
                }
                for (var i = 0; i < edges.Count && valid; i++)
                {
                    var e = edges[i];
                    if (Vector2.Distance(points[e.a],points[e.b]) < p.MinimumEdgeLength) valid = false;
                    for (var j = i+1; j < edges.Count; j++)
                    {
                        var f = edges[j];
                        if (e.a != f.a && e.a != f.b && e.b != f.a && e.b != f.b && SegmentDistance(points[e.a],points[e.b],points[f.a],points[f.b]) < p.IndependentAxisGap) valid = false;
                    }
                }
                if (!valid) continue;
                var candidates = new List<(int a,int b)>();
                for (var i = 0; i < count; i++) for (var j = i+1; j < count; j++) if (!Neighbors(i).Contains(j)) candidates.Add((i,j));
                Shuffle(rng,candidates);
                var desired = rng.Next(p.AdditionalLinkCountMin,p.AdditionalLinkCountMax+1); var added = 0;
                foreach (var e in candidates)
                {
                    if (added >= desired) break;
                    var length = Vector2.Distance(points[e.a],points[e.b]);
                    if (length > p.MaximumExtraEdgeLength || Neighbors(e.a).Count >= 3 || Neighbors(e.b).Count >= 3 || !AngleClear(e.a,points[e.b]) || !AngleClear(e.b,points[e.a])) continue;
                    if (edges.Any(f => e.a != f.a && e.a != f.b && e.b != f.a && e.b != f.b && SegmentDistance(points[e.a],points[e.b],points[f.a],points[f.b]) < p.IndependentAxisGap)) continue;
                    if (Shortest(points,edges,e.a,e.b)+length < p.MinimumCycleLength) continue;
                    edges.Add(e); added++;
                }
                var segments = edges.Select(e => (a:points[e.a],b:points[e.b])).ToList();
                var anchors = new List<Vector2>(); var owners = new List<int>();
                var leaves = Enumerable.Range(0,count).Where(i => Neighbors(i).Count == 1).ToList(); Shuffle(rng,leaves);
                var extras = Enumerable.Range(0,count).Where(i => !leaves.Contains(i) && Neighbors(i).Count < 3).ToList(); Shuffle(rng,extras);
                foreach (var i in leaves.Concat(extras))
                {
                    if (!leaves.Contains(i) && anchors.Count >= p.MinimumRingConnections) break;
                    var q = points[i]; var coordinate = half-p.AnchorCornerMargin;
                    var ports = new[] { new Vector2(-bound,Mathf.Clamp(q.y,-coordinate,coordinate)),new Vector2(bound,Mathf.Clamp(q.y,-coordinate,coordinate)),
                        new Vector2(Mathf.Clamp(q.x,-coordinate,coordinate),-bound),new Vector2(Mathf.Clamp(q.x,-coordinate,coordinate),bound) };
                    var accepted = false;
                    foreach (var port in ports.OrderBy(n => (n-q).sqrMagnitude))
                    {
                        if (!AngleClear(i,port) || anchors.Any(n => Vector2.Distance(n,port) < p.AnchorGap) ||
                            segments.Any(s => s.a != q && s.b != q && SegmentDistance(q,port,s.a,s.b) < p.IndependentAxisGap)) continue;
                        segments.Add((q,port)); anchors.Add(port); owners.Add(i); accepted = true; break;
                    }
                    if (!accepted && leaves.Contains(i)) { valid = false; break; }
                }
                if (!valid || anchors.Count < p.MinimumRingConnections || !CyclesClear(p,points,edges,anchors,owners,ring)) continue;
                var paths = new List<Vector2[]> { ring.ToArray() };
                List<Vector2> Incident(Vector2 q) => segments.Where(s => s.a == q || s.b == q).Select(s => s.a == q ? s.b : s.a).ToList();
                foreach (var s in segments)
                    paths.Add(new[] { Incident(s.a).Count == 2 ? s.a+(s.b-s.a).normalized*p.BendTrim : s.a,
                        Incident(s.b).Count == 2 ? s.b+(s.a-s.b).normalized*p.BendTrim : s.b });
                foreach (var q in points)
                {
                    var incident = Incident(q); if (incident.Count != 2) continue;
                    var a = q+(incident[0]-q).normalized*p.BendTrim; var b = q+(incident[1]-q).normalized*p.BendTrim;
                    var curve = new Vector2[p.BendSamples];
                    for (var i = 0; i < curve.Length; i++) { var t = i/(float)(curve.Length-1); curve[i] = (1-t)*(1-t)*a+2*t*(1-t)*q+t*t*b; }
                    paths.Add(curve);
                }
                return paths;
            }
            return null;
        }

        private static bool CyclesClear(FieldRoadLayoutDefinition p, List<Vector2> points, List<(int a,int b)> edges,
            List<Vector2> anchors, List<int> owners, List<Vector2> ring)
        {
            var nodes = new List<Vector2>(points); nodes.AddRange(anchors);
            var weighted = edges.Select(e => (e.a,e.b,w:Vector2.Distance(points[e.a],points[e.b]))).ToList();
            var ports = new List<(float position,int node)>(); var ringLength = 0f;
            for (var i = 1; i < ring.Count; i++) ringLength += Vector2.Distance(ring[i-1],ring[i]);
            for (var i = 0; i < anchors.Count; i++)
            {
                weighted.Add((owners[i],points.Count+i,Vector2.Distance(points[owners[i]],anchors[i])));
                var cumulative = 0f; var best = float.PositiveInfinity; var position = 0f;
                for (var j = 1; j < ring.Count; j++)
                {
                    var d = FieldRoadLayout.Distance(anchors[i],ring[j-1],ring[j]);
                    if (d < best) { best = d; position = cumulative+Vector2.Distance(ring[j-1],FieldRoadLayout.Closest(anchors[i],ring[j-1],ring[j])); }
                    cumulative += Vector2.Distance(ring[j-1],ring[j]);
                }
                ports.Add((position,points.Count+i));
            }
            ports.Sort((a,b) => a.position.CompareTo(b.position));
            for (var i = 0; i < ports.Count; i++)
            {
                var a = ports[i]; var b = ports[(i+1)%ports.Count]; var arc = b.position-a.position;
                if (arc <= 0) arc += ringLength;
                if (ShortestWeighted(nodes.Count,weighted,a.node,b.node)+arc < p.MinimumCycleLength) return false;
                weighted.Add((a.node,b.node,arc));
            }
            return true;
        }

        private static List<FieldRoadDeadEnd> MakeBranches(FieldRoadLayoutDefinition p, Random rng, List<Vector2[]> roads)
        {
            var segments = roads.SelectMany(path => Enumerable.Range(1,path.Length-1).Select(i => (a:path[i-1],b:path[i],length:Vector2.Distance(path[i-1],path[i])))).ToArray();
            var total = segments.Sum(s => s.length);
            var ends = new Dictionary<Vector2,int>();
            foreach (var path in roads.Skip(1)) foreach (var point in new[] { path[0],path[path.Length-1] })
                ends[point] = ends.TryGetValue(point,out var count) ? count+1 : 1;
            var junctions = ends.Where(e => e.Value == 1 || e.Value >= 3).Select(e => e.Key).ToArray();
            var branches = new List<FieldRoadDeadEnd>(); var half = p.ArenaSideLength*.5f;
            for (var attempt = 0; attempt < p.BookPlacementAttempts; attempt++)
            {
                var remaining = Range(rng,0,total); var index = 0;
                while (index < segments.Length-1 && remaining > segments[index].length) remaining -= segments[index++].length;
                var s = segments[index]; var entrance = Vector2.Lerp(s.a,s.b,(float)rng.NextDouble());
                if (junctions.Any(j => Vector2.Distance(j,entrance) < p.BookEntranceJunctionGap) || branches.Any(b => Vector2.Distance(b.Entrance,entrance) < p.BookEntranceGap)) continue;
                var tangent = (s.b-s.a).normalized;
                var normal = new Vector2(-tangent.y,tangent.x)*(rng.Next(2) == 0 ? -1 : 1);
                var tilt = Range(rng,-p.BranchTiltDegrees,p.BranchTiltDegrees)*Mathf.Deg2Rad;
                normal = new Vector2(normal.x*Mathf.Cos(tilt)-normal.y*Mathf.Sin(tilt),normal.x*Mathf.Sin(tilt)+normal.y*Mathf.Cos(tilt));
                var length = Range(rng,p.DeadEndLengthMin,p.DeadEndLengthMax);
                var center = entrance+normal*(p.MainRoadWidth*.5f/Mathf.Cos(tilt)+length-p.DeadEndEndRadius);
                var margin = p.DeadEndEndRadius+p.FieldPadding;
                if (Mathf.Abs(center.x)+margin > half || Mathf.Abs(center.y)+margin > half) continue;
                // Continuous branch-branch gap including bulb/corridor combinations.
                if (branches.Any(b => SegmentDistance(entrance,center,b.Entrance,b.EndCenter) < p.DeadEndWidth+p.IndependentSurfaceGap ||
                    Vector2.Distance(center,b.EndCenter) < 2*p.DeadEndEndRadius+p.IndependentSurfaceGap ||
                    FieldRoadLayout.Distance(center,b.Entrance,b.EndCenter) < p.DeadEndEndRadius+p.DeadEndWidth*.5f+p.IndependentSurfaceGap ||
                    FieldRoadLayout.Distance(b.EndCenter,entrance,center) < p.DeadEndEndRadius+p.DeadEndWidth*.5f+p.IndependentSurfaceGap)) continue;
                // Grid inspection matches the accepted study. Outside the mouth reserve three units of grass against other axes.
                var min = Vector2.Min(entrance,center)-Vector2.one*p.DeadEndEndRadius;
                var max = Vector2.Max(entrance,center)+Vector2.one*p.DeadEndEndRadius;
                var clear = true;
                for (var y = min.y; y <= max.y && clear; y += p.SurfaceStep)
                for (var x = min.x; x <= max.x && clear; x += p.SurfaceStep)
                {
                    var q = new Vector2(x,y);
                    if (FieldRoadLayout.Distance(q,entrance,center) > p.DeadEndWidth*.5f && Vector2.Distance(q,center) > p.DeadEndEndRadius) continue;
                    if (Mathf.Abs(q.x) > half-p.FieldPadding || Mathf.Abs(q.y) > half-p.FieldPadding) { clear = false; break; }
                    if (Vector2.Distance(q,entrance) <= p.ParentOverlapRadius) continue;
                    foreach (var road in segments)
                        if (FieldRoadLayout.Distance(q,road.a,road.b) < p.MainRoadWidth*.5f+p.IndependentSurfaceGap) { clear = false; break; }
                }
                if (clear) branches.Add(new FieldRoadDeadEnd(entrance,center,length));
            }
            return branches;
        }

        private static float Shortest(List<Vector2> points, List<(int a,int b)> edges, int start, int end) =>
            ShortestWeighted(points.Count,edges.Select(e => (e.a,e.b,Vector2.Distance(points[e.a],points[e.b]))).ToList(),start,end);
        private static float ShortestWeighted(int count, List<(int a,int b,float w)> edges, int start, int end)
        {
            var costs = Enumerable.Repeat(float.PositiveInfinity,count).ToArray(); var visited = new bool[count]; costs[start] = 0;
            for (var step = 0; step < count; step++)
            {
                var current = -1;
                for (var i = 0; i < count; i++) if (!visited[i] && (current < 0 || costs[i] < costs[current])) current = i;
                if (current == end) return costs[current];
                if (current < 0 || float.IsPositiveInfinity(costs[current])) break;
                visited[current] = true;
                foreach (var e in edges)
                    if (e.a == current || e.b == current) { var other = e.a == current ? e.b : e.a; costs[other] = Mathf.Min(costs[other],costs[current]+e.w); }
            }
            return float.PositiveInfinity;
        }
        private static float Cross(Vector2 a, Vector2 b) => a.x*b.y-a.y*b.x;
        private static float SegmentDistance(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            var denominator = Cross(b-a,d-c);
            if (Mathf.Abs(denominator) > 1e-6f)
            {
                var t = Cross(c-a,d-c)/denominator; var u = Cross(c-a,b-a)/denominator;
                if (t >= 0 && t <= 1 && u >= 0 && u <= 1) return 0;
            }
            return Mathf.Min(FieldRoadLayout.Distance(c,a,b),FieldRoadLayout.Distance(d,a,b),FieldRoadLayout.Distance(a,c,d),FieldRoadLayout.Distance(b,c,d));
        }
        private static float Range(Random rng, float a, float b) => a+(b-a)*(float)rng.NextDouble();
        private static void Shuffle<T>(Random rng,List<T> items)
        { for (var i = items.Count-1; i > 0; i--) { var j = rng.Next(i+1); (items[i],items[j]) = (items[j],items[i]); } }
    }
}
