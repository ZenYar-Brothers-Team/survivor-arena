using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Diagnostics;
using UnityEngine;
namespace Game.Pickup
{
    /// <summary>Nearest point in the spawn-connected component of an axis-aligned arena, accounting for player footprint.</summary>
    public sealed class BoxPickupPlacement : IPickupPlacement
    {
        private readonly Rect _bounds;
        private readonly Rect[] _obstacles;
        private readonly Vector2 _anchor;
        public BoxPickupPlacement(Rect bounds, IEnumerable<Rect> obstacles, Vector2 playerHalfSize, Vector2 anchor, float skin)
        {
            NumericValidation.ValidatePositive(skin, nameof(skin));
            NumericValidation.ValidatePositive(bounds.width, nameof(bounds));
            NumericValidation.ValidatePositive(bounds.height, nameof(bounds));
            NumericValidation.ValidateNonNegative(playerHalfSize.x, nameof(playerHalfSize));
            NumericValidation.ValidateNonNegative(playerHalfSize.y, nameof(playerHalfSize));
            var margin = playerHalfSize + Vector2.one * skin;
            _bounds = Rect.MinMaxRect(bounds.xMin + margin.x, bounds.yMin + margin.y, bounds.xMax - margin.x, bounds.yMax - margin.y);
            _obstacles = obstacles.Select(rect => Rect.MinMaxRect(rect.xMin - margin.x, rect.yMin - margin.y,
                rect.xMax + margin.x, rect.yMax + margin.y)).ToArray();
            _anchor = anchor;
            if (_bounds.width <= 0 || _bounds.height <= 0 || !Free(anchor)) throw new ArgumentException("Arena requires a reachable spawn with player clearance.");
        }
        private bool Free(Vector2 p) => p.x >= _bounds.xMin && p.x <= _bounds.xMax && p.y >= _bounds.yMin && p.y <= _bounds.yMax &&
            !_obstacles.Any(rect => p.x > rect.xMin && p.x < rect.xMax && p.y > rect.yMin && p.y < rect.yMax);
        private bool ClearSegment(Vector2 a, Vector2 b)
        {
            foreach (var rect in _obstacles)
                if (a.x == b.x ? a.x > rect.xMin && a.x < rect.xMax && Math.Max(a.y, b.y) > rect.yMin && Math.Min(a.y, b.y) < rect.yMax
                    : a.y > rect.yMin && a.y < rect.yMax && Math.Max(a.x, b.x) > rect.xMin && Math.Min(a.x, b.x) < rect.xMax) return false;
            return true;
        }
        public bool TryPlace(Vector2 requested, out Vector2 reachable)
        {
            NumericValidation.ValidateFinite(requested.x, nameof(requested)); NumericValidation.ValidateFinite(requested.y, nameof(requested));
            using var guard = PerfGuard.Measure("Pickup.ReachablePlacement", 5f);
            var xs = new List<float> { _bounds.xMin, _bounds.xMax, _anchor.x, Mathf.Clamp(requested.x, _bounds.xMin, _bounds.xMax) };
            var ys = new List<float> { _bounds.yMin, _bounds.yMax, _anchor.y, Mathf.Clamp(requested.y, _bounds.yMin, _bounds.yMax) };
            foreach (var rect in _obstacles) { xs.Add(rect.xMin); xs.Add(rect.xMax); ys.Add(rect.yMin); ys.Add(rect.yMax); }
            xs = xs.Where(x => x >= _bounds.xMin && x <= _bounds.xMax).Distinct().OrderBy(x => x).ToList();
            ys = ys.Where(y => y >= _bounds.yMin && y <= _bounds.yMax).Distinct().OrderBy(y => y).ToList();
            var visited = new bool[xs.Count * ys.Count];
            var queue = new Queue<int>();
            var first = xs.IndexOf(_anchor.x) * ys.Count + ys.IndexOf(_anchor.y);
            visited[first] = true; queue.Enqueue(first);
            reachable = _anchor; var best = float.PositiveInfinity;
            while (queue.Count > 0)
            {
                var index = queue.Dequeue(); var x = index / ys.Count; var y = index % ys.Count;
                var point = new Vector2(xs[x], ys[y]); var distance = (point - requested).sqrMagnitude;
                if (distance < best) { best = distance; reachable = point; }
                Visit(x - 1, y, point); Visit(x + 1, y, point); Visit(x, y - 1, point); Visit(x, y + 1, point);
            }
            return !float.IsPositiveInfinity(best);
            void Visit(int x, int y, Vector2 from)
            {
                if (x < 0 || x >= xs.Count || y < 0 || y >= ys.Count) return;
                var index = x * ys.Count + y;
                if (visited[index]) return;
                var to = new Vector2(xs[x], ys[y]);
                if (!Free(to) || !ClearSegment(from, to)) return;
                visited[index] = true; queue.Enqueue(index);
            }
        }
    }
}
