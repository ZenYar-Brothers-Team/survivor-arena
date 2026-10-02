using System;
using Game.Diagnostics;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Signed distance to a road surface sampled on the corners of a <see cref="FieldRoadLayoutDefinition.SurfaceStep"/> grid
    /// (negative on the road). Contours, meshes and connectivity are interpolated from it, so the edge follows the analytic
    /// road and round-end outline instead of cell steps (DECISION-0137 revision: smooth, non-snagging edge).
    /// </summary>
    public sealed class FieldRoadDistanceField
    {
        private readonly float[] _values;

        /// <summary>Cells per side; corners per side are <c>Cells + 1</c>.</summary>
        public int Cells { get; }
        public float Step { get; }
        /// <summary>World x and y of corner (0, 0).</summary>
        public float Origin { get; }

        private FieldRoadDistanceField(int cells, float step, float origin)
        {
            Cells = cells; Step = step; Origin = origin;
            _values = new float[(cells + 1) * (cells + 1)];
        }

        public float this[int x, int y] => _values[y * (Cells + 1) + x];
        public bool Inside(int x, int y) => this[x, y] < 0f;
        public Vector2 Corner(int x, int y) => new Vector2(Origin + x * Step, Origin + y * Step);

        /// <summary>Everything the player may walk on: main roads, dead-end corridors and their round ends.</summary>
        public static FieldRoadDistanceField Walkable(FieldRoadLayout layout) => Build(layout, true, true, false);

        /// <summary>
        /// The dead-end surface drawn over the main roads: corridors and round ends, clipped so that it reaches only
        /// <see cref="FieldRoadLayoutDefinition.DeadEndMouthOverlap"/> past a main road edge (user revision 2026-10-02: the
        /// broken surface belongs to the branch, the main road keeps the junction).
        /// </summary>
        public static FieldRoadDistanceField Branches(FieldRoadLayout layout)
        {
            var branches = Build(layout, false, true, true);
            var main = Build(layout, true, false, false);
            var overlap = layout.Profile.DeadEndMouthOverlap;
            // Intersection with the outside of the main road shrunk by the overlap: max(branch, -(main + overlap)).
            for (var i = 0; i < branches._values.Length; i++)
                branches._values[i] = Mathf.Max(branches._values[i], -(main._values[i] + overlap));
            return branches;
        }

        // flatMouth: a corridor starts flat at its entrance instead of a round cap reaching back across the main road.
        private static FieldRoadDistanceField Build(FieldRoadLayout layout, bool includeMain, bool includeBranches, bool flatMouth)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            using var guard = PerfGuard.Measure("RoadLayout.DistanceField", 250f);
            var p = layout.Profile;
            var field = new FieldRoadDistanceField(Mathf.CeilToInt(p.ArenaSideLength / p.SurfaceStep), p.SurfaceStep,
                -p.ArenaSideLength * .5f);
            // Far corners only need the right sign; every corner next to an edge lies inside some primitive's margin.
            var far = 4f * p.SurfaceStep;
            for (var i = 0; i < field._values.Length; i++) field._values[i] = far;
            if (includeMain)
                foreach (var path in layout.Roads)
                    for (var i = 1; i < path.Count; i++) field.Paint(path[i-1], path[i], p.MainRoadWidth * .5f);
            if (includeBranches)
                foreach (var branch in layout.DeadEnds)
                {
                    field.Paint(branch.Entrance, branch.EndCenter, p.DeadEndWidth * .5f, flatMouth);
                    field.Paint(branch.EndCenter, branch.EndCenter, p.DeadEndEndRadius);
                }
            return field;
        }

        // Distance to a capsule (segment of radius r), evaluated only inside its bounding rectangle plus two cells.
        private void Paint(Vector2 a, Vector2 b, float radius, bool flatStart = false)
        {
            var axis = (b - a).normalized;
            var margin = radius + 2f * Step;
            var min = Vector2.Min(a, b) - Vector2.one * margin;
            var max = Vector2.Max(a, b) + Vector2.one * margin;
            var x0 = Mathf.Clamp(Mathf.FloorToInt((min.x - Origin) / Step), 0, Cells);
            var x1 = Mathf.Clamp(Mathf.CeilToInt((max.x - Origin) / Step), 0, Cells);
            var y0 = Mathf.Clamp(Mathf.FloorToInt((min.y - Origin) / Step), 0, Cells);
            var y1 = Mathf.Clamp(Mathf.CeilToInt((max.y - Origin) / Step), 0, Cells);
            for (var y = y0; y <= y1; y++)
            for (var x = x0; x <= x1; x++)
            {
                var index = y * (Cells + 1) + x;
                var value = FieldRoadLayout.Distance(Corner(x, y), a, b) - radius;
                if (flatStart) value = Mathf.Max(value, -Vector2.Dot(Corner(x, y) - a, axis));
                if (value < _values[index]) _values[index] = value;
            }
        }
    }
}
