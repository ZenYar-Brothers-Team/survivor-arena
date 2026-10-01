using System.Collections.Generic;
using UnityEngine;

namespace Game.Zones
{
    /// <summary>Small polygon helpers for keeping zones clear of obstacle outlines.</summary>
    public static class ZoneGeometry
    {
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

        /// <summary>Clearance from a disc to a polygon: negative when the disc center is inside the polygon.</summary>
        public static float DiscClearance(IReadOnlyList<Vector2> polygon, Vector2 center, float radius) =>
            Contains(polygon, center) ? -1f : DistanceToOutline(polygon, center) - radius;
    }
}
