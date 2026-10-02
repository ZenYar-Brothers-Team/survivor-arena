using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>One immutable, centered road network; its surface is sampled by <see cref="FieldRoadDistanceField"/>.</summary>
    public sealed class FieldRoadLayout
    {
        public IReadOnlyList<IReadOnlyList<Vector2>> Roads { get; }
        public IReadOnlyList<FieldRoadDeadEnd> DeadEnds { get; }
        public int Seed { get; }
        public bool UsedFallback { get; }
        /// <summary>Road graphs the generator built and checked for this result, including rejected ones; 0 for supplied geometry.</summary>
        public int GraphAttempts { get; }
        /// <summary>Complete networks tried before enough book branches fitted (1..LayoutAttempts); 0 for supplied geometry.</summary>
        public int LayoutAttempts { get; }
        /// <summary>Point on a main road's centerline nearest to the arena center, including when the center lies on grass.</summary>
        public Vector2 SpawnPosition { get; }
        public FieldRoadLayoutDefinition Profile { get; }
        public FieldRoadLayout(FieldRoadLayoutDefinition profile, IEnumerable<IEnumerable<Vector2>> roads,
            IEnumerable<FieldRoadDeadEnd> deadEnds, int seed, bool usedFallback, int graphAttempts = 0, int layoutAttempts = 0)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            Roads = roads.Select(p => (IReadOnlyList<Vector2>)Array.AsReadOnly(p.ToArray())).ToArray();
            DeadEnds = Array.AsReadOnly(deadEnds.ToArray());
            if (Roads.Count < 2 || Roads.Any(p => p.Count < 2) || DeadEnds.Count < profile.MinimumDeadEnds)
                throw new ArgumentException("Incomplete road network.");
            foreach (var path in Roads)
            foreach (var point in path)
            {
                NumericValidation.ValidateFinite(point.x, "road x"); NumericValidation.ValidateFinite(point.y, "road y");
                if (Mathf.Abs(point.x) > profile.ArenaSideLength * .5f || Mathf.Abs(point.y) > profile.ArenaSideLength * .5f)
                    throw new ArgumentException("Road is outside the arena.");
            }
            foreach (var branch in DeadEnds)
                if (branch.Length < profile.DeadEndLengthMin || branch.Length > profile.DeadEndLengthMax ||
                    MainDistance(branch.Entrance) > profile.SurfaceStep ||
                    Mathf.Abs(branch.EndCenter.x) + profile.DeadEndEndRadius > profile.ArenaSideLength * .5f - profile.FieldPadding ||
                    Mathf.Abs(branch.EndCenter.y) + profile.DeadEndEndRadius > profile.ArenaSideLength * .5f - profile.FieldPadding)
                    throw new ArgumentException("Invalid or disconnected road branch.");
            Seed = seed; UsedFallback = usedFallback;
            NumericValidation.ValidateNonNegative(graphAttempts, nameof(graphAttempts));
            NumericValidation.ValidateNonNegative(layoutAttempts, nameof(layoutAttempts));
            GraphAttempts = graphAttempts; LayoutAttempts = layoutAttempts;
            // Closest point on a main axis to the arena center. Safe for the actual body radius, checked by composition.
            var closest = Roads[0][0];
            foreach (var path in Roads)
                for (var i = 1; i < path.Count; i++)
                {
                    var point = Closest(Vector2.zero, path[i-1], path[i]);
                    if (point.sqrMagnitude < closest.sqrMagnitude) closest = point;
                }
            SpawnPosition = closest;
        }

        public static FieldRoadLayout FromReference(FieldRoadLayoutDefinition profile, FieldRoadReferenceData data, int seed)
        {
            if (data?.RoadCenterlines == null || data.DeadEnds == null || !data.Seed.HasValue)
                throw new ArgumentException("Road fallback geometry is required.");
            var offset = Vector2.one * (profile.ArenaSideLength * .5f);
            Vector2 Point(float[] p)
            {
                if (p == null || p.Length != 2) throw new ArgumentException("Expected a planar reference point.");
                return new Vector2(p[0], p[1]) - offset;
            }
            return new FieldRoadLayout(profile, data.RoadCenterlines.Select(p => p.Select(Point)), data.DeadEnds.Select(b =>
            {
                if (Point(b.BookPosition) != Point(b.EndCenter)) throw new ArgumentException("Reference book must be centered.");
                return new FieldRoadDeadEnd(Point(b.Entrance), Point(b.EndCenter), b.Length ?? throw new ArgumentException("Branch length required."));
            }), seed, true);
        }

        public float MainDistance(Vector2 p)
        {
            var best = float.PositiveInfinity;
            foreach (var path in Roads)
                for (var i = 1; i < path.Count; i++) best = Mathf.Min(best, Distance(p, path[i-1], path[i]));
            return best;
        }
        public static Vector2 Closest(Vector2 p, Vector2 a, Vector2 b)
        {
            var d = b-a;
            return a + d * (d.sqrMagnitude == 0 ? 0 : Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude));
        }
        public static float Distance(Vector2 p, Vector2 a, Vector2 b) => Vector2.Distance(p, Closest(p,a,b));
    }
}
