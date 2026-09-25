using System;
using System.Collections.Generic;
using Game.Content;
using Game.Diagnostics;
using UnityEngine;
namespace Game.Pickup
{
    /// <summary>
    /// Nearest point in the spawn-connected component of an axis-aligned arena, accounting for player footprint.
    /// Main-thread only; one instance keeps reusable query buffers and is not thread-safe.
    /// </summary>
    /// <remarks>
    /// Reachability is a breadth-first search over the grid formed by the arena bounds, the spawn anchor and every
    /// inflated obstacle edge, plus one extra column/row through the requested point. The obstacle-derived grid never
    /// changes, so its node and edge clearances are computed once in the constructor from per-row/per-column obstacle
    /// lists; a query only checks the nodes and edges on its extra lines and reuses preallocated buffers, so
    /// <see cref="TryPlace"/> allocates nothing. Visit order and tie-breaking match the original per-call grid build
    /// exactly (perf fix for the "Pickup.ReachablePlacement" PerfGuard warnings, docs/regression-map.md).
    /// </remarks>
    public sealed class BoxPickupPlacement : IPickupPlacement
    {
        private readonly Rect _bounds;
        private readonly Rect[] _obstacles;
        private readonly Vector2 _anchor;

        // Static grid: sorted unique coordinates inside the bounds.
        private readonly float[] _xs;
        private readonly float[] _ys;
        private readonly int _anchorColumn;
        private readonly int _anchorRow;
        // Obstacles whose open interior spans a static column/row (compressed lists: start offsets + indices).
        private readonly int[] _columnStart;
        private readonly int[] _columnObstacles;
        private readonly int[] _rowStart;
        private readonly int[] _rowObstacles;
        // Clearances of static nodes and of edges between neighbouring static nodes.
        private readonly bool[] _nodeFree;          // column * rows + row
        private readonly bool[] _horizontalClear;   // column * rows + row: (column, row) -> (column + 1, row)
        private readonly bool[] _verticalClear;     // column * (rows - 1) + row: (column, row) -> (column, row + 1)

        // Per-query reusable buffers sized for the static grid plus one extra column and row.
        private readonly bool[] _visited;
        private readonly int[] _queue;
        private readonly int[] _extraColumnObstacles;
        private readonly int[] _extraRowObstacles;
        private int _extraColumnCount;
        private int _extraRowCount;
        // Current query grid: coordinates per query column/row and the matching static index (-1 = the extra line
        // through the requested point, present only when that coordinate is not already a static line).
        private readonly float[] _queryX;
        private readonly float[] _queryY;
        private readonly int[] _staticColumn;
        private readonly int[] _staticRow;
        private float _extraY;
        private int _columns;
        private int _rows;

        public BoxPickupPlacement(Rect bounds, IEnumerable<Rect> obstacles, Vector2 playerHalfSize, Vector2 anchor, float skin)
        {
            if (obstacles == null) throw new ArgumentNullException(nameof(obstacles));
            NumericValidation.ValidatePositive(skin, nameof(skin));
            NumericValidation.ValidatePositive(bounds.width, nameof(bounds));
            NumericValidation.ValidatePositive(bounds.height, nameof(bounds));
            NumericValidation.ValidateNonNegative(playerHalfSize.x, nameof(playerHalfSize));
            NumericValidation.ValidateNonNegative(playerHalfSize.y, nameof(playerHalfSize));
            var margin = playerHalfSize + Vector2.one * skin;
            _bounds = Rect.MinMaxRect(bounds.xMin + margin.x, bounds.yMin + margin.y, bounds.xMax - margin.x, bounds.yMax - margin.y);
            var inflated = new List<Rect>();
            foreach (var rect in obstacles)
                inflated.Add(Rect.MinMaxRect(rect.xMin - margin.x, rect.yMin - margin.y, rect.xMax + margin.x, rect.yMax + margin.y));
            _obstacles = inflated.ToArray();
            _anchor = anchor;
            if (_bounds.width <= 0 || _bounds.height <= 0 || !Free(anchor)) throw new ArgumentException("Arena requires a reachable spawn with player clearance.");

            _xs = GridCoordinates(_bounds.xMin, _bounds.xMax, anchor.x, true);
            _ys = GridCoordinates(_bounds.yMin, _bounds.yMax, anchor.y, false);
            _anchorColumn = Array.BinarySearch(_xs, anchor.x);
            _anchorRow = Array.BinarySearch(_ys, anchor.y);
            BuildSpanLists(_xs, true, out _columnStart, out _columnObstacles);
            BuildSpanLists(_ys, false, out _rowStart, out _rowObstacles);

            var columns = _xs.Length; var rows = _ys.Length;
            _nodeFree = new bool[columns * rows];
            _horizontalClear = new bool[(columns - 1) * rows];
            _verticalClear = new bool[columns * (rows - 1)];
            for (var column = 0; column < columns; column++)
            for (var row = 0; row < rows; row++)
            {
                _nodeFree[column * rows + row] = NodeFree(_ys[row], _columnObstacles, _columnStart[column], _columnStart[column + 1]);
                if (column + 1 < columns)
                    _horizontalClear[column * rows + row] = SpanClear(_xs[column], _xs[column + 1], true,
                        _rowObstacles, _rowStart[row], _rowStart[row + 1]);
                if (row + 1 < rows)
                    _verticalClear[column * (rows - 1) + row] = SpanClear(_ys[row], _ys[row + 1], false,
                        _columnObstacles, _columnStart[column], _columnStart[column + 1]);
            }

            _queryX = new float[columns + 1];
            _queryY = new float[rows + 1];
            _staticColumn = new int[columns + 1];
            _staticRow = new int[rows + 1];
            _visited = new bool[(columns + 1) * (rows + 1)];
            _queue = new int[_visited.Length];
            _extraColumnObstacles = new int[_obstacles.Length];
            _extraRowObstacles = new int[_obstacles.Length];
        }

        private bool Free(Vector2 p)
        {
            if (p.x < _bounds.xMin || p.x > _bounds.xMax || p.y < _bounds.yMin || p.y > _bounds.yMax) return false;
            foreach (var rect in _obstacles)
                if (p.x > rect.xMin && p.x < rect.xMax && p.y > rect.yMin && p.y < rect.yMax) return false;
            return true;
        }

        // A free point reached from an already reachable point stays in the spawn-connected component.
        // This avoids the grid search for ordinary small movement steps.
        public bool TryPlaceFrom(Vector2 knownReachable, Vector2 requested, out Vector2 reachable)
        {
            NumericValidation.ValidateFinite(requested.x, nameof(requested));
            NumericValidation.ValidateFinite(requested.y, nameof(requested));
            if (Free(knownReachable) && Free(requested) && ClearStraightSegment(knownReachable, requested))
            { reachable = requested; return true; }
            return TryPlace(requested, out reachable);
        }

        private bool ClearStraightSegment(Vector2 a, Vector2 b)
        {
            var delta = b - a;
            foreach (var rect in _obstacles)
            {
                var min = 0f; var max = 1f;
                if (delta.x == 0)
                { if (a.x <= rect.xMin || a.x >= rect.xMax) continue; }
                else
                {
                    var first = (rect.xMin - a.x) / delta.x;
                    var second = (rect.xMax - a.x) / delta.x;
                    min = Mathf.Max(min, Mathf.Min(first, second));
                    max = Mathf.Min(max, Mathf.Max(first, second));
                }
                if (delta.y == 0)
                { if (a.y <= rect.yMin || a.y >= rect.yMax) continue; }
                else
                {
                    var first = (rect.yMin - a.y) / delta.y;
                    var second = (rect.yMax - a.y) / delta.y;
                    min = Mathf.Max(min, Mathf.Min(first, second));
                    max = Mathf.Min(max, Mathf.Max(first, second));
                }
                if (min < max) return false;
            }
            return true;
        }

        /// <summary>
        /// Projects <paramref name="requested"/> to the nearest grid point reachable from the spawn anchor.
        /// Allocation-free; cost is proportional to the static grid size, not to the obstacle count per node.
        /// </summary>
        public bool TryPlace(Vector2 requested, out Vector2 reachable)
        {
            NumericValidation.ValidateFinite(requested.x, nameof(requested)); NumericValidation.ValidateFinite(requested.y, nameof(requested));
            using var guard = PerfGuard.Measure("Pickup.ReachablePlacement", 5f);
            PrepareQueryGrid(Mathf.Clamp(requested.x, _bounds.xMin, _bounds.xMax), Mathf.Clamp(requested.y, _bounds.yMin, _bounds.yMax));
            Array.Clear(_visited, 0, _columns * _rows);
            var head = 0; var tail = 0;
            var first = IndexOf(_staticColumn, _columns, _anchorColumn) * _rows + IndexOf(_staticRow, _rows, _anchorRow);
            _visited[first] = true; _queue[tail++] = first;
            reachable = _anchor; var best = float.PositiveInfinity;
            while (head < tail)
            {
                var index = _queue[head++]; var x = index / _rows; var y = index % _rows;
                var point = new Vector2(_queryX[x], _queryY[y]); var distance = (point - requested).sqrMagnitude;
                if (distance < best) { best = distance; reachable = point; }
                // Same neighbour order as the original search keeps equal-distance ties identical.
                if (x > 0) Visit(x - 1, y, x, y, ref tail);
                if (x + 1 < _columns) Visit(x + 1, y, x, y, ref tail);
                if (y > 0) Visit(x, y - 1, x, y, ref tail);
                if (y + 1 < _rows) Visit(x, y + 1, x, y, ref tail);
            }
            return !float.IsPositiveInfinity(best);
        }

        private void Visit(int x, int y, int fromX, int fromY, ref int tail)
        {
            var index = x * _rows + y;
            if (_visited[index] || !QueryNodeFree(x, y) || !QueryEdgeClear(fromX, fromY, x, y)) return;
            _visited[index] = true; _queue[tail++] = index;
        }

        private void PrepareQueryGrid(float x, float y)
        {
            _columns = FillQueryAxis(_xs, x, _queryX, _staticColumn);
            _extraColumnCount = _columns > _xs.Length ? CollectSpanning(x, true, _extraColumnObstacles) : 0;
            _rows = FillQueryAxis(_ys, y, _queryY, _staticRow);
            _extraRowCount = _rows > _ys.Length ? CollectSpanning(y, false, _extraRowObstacles) : 0;
            _extraY = y;
        }

        // Static lines plus the requested coordinate inserted in sorted order when it is not already a line.
        private static int FillQueryAxis(float[] lines, float value, float[] coordinates, int[] staticIndex)
        {
            var found = Array.BinarySearch(lines, value);
            var insert = found >= 0 ? -1 : ~found;
            var count = 0;
            for (var line = 0; line <= lines.Length; line++)
            {
                if (line == insert) { coordinates[count] = value; staticIndex[count++] = -1; }
                if (line < lines.Length) { coordinates[count] = lines[line]; staticIndex[count++] = line; }
            }
            return count;
        }

        private static int IndexOf(int[] staticIndex, int count, int value)
        {
            for (var i = 0; i < count; i++) if (staticIndex[i] == value) return i;
            throw new InvalidOperationException("Static grid line is missing from the query grid.");
        }

        private bool QueryNodeFree(int x, int y)
        {
            var column = _staticColumn[x];
            if (column < 0) return NodeFree(_queryY[y], _extraColumnObstacles, 0, _extraColumnCount);
            var row = _staticRow[y];
            if (row < 0) return NodeFree(_extraY, _columnObstacles, _columnStart[column], _columnStart[column + 1]);
            return _nodeFree[column * _ys.Length + row];
        }

        private bool QueryEdgeClear(int fromX, int fromY, int toX, int toY)
        {
            if (fromY == toY)
            {
                var low = Math.Min(fromX, toX);
                var row = _staticRow[fromY];
                if (row < 0)
                    return SpanClear(_queryX[low], _queryX[low + 1], true, _extraRowObstacles, 0, _extraRowCount);
                var left = _staticColumn[low];
                if (left < 0 || _staticColumn[low + 1] < 0)
                    return SpanClear(_queryX[low], _queryX[low + 1], true, _rowObstacles, _rowStart[row], _rowStart[row + 1]);
                return _horizontalClear[left * _ys.Length + row];
            }
            var bottom = Math.Min(fromY, toY);
            var column = _staticColumn[fromX];
            if (column < 0)
                return SpanClear(_queryY[bottom], _queryY[bottom + 1], false, _extraColumnObstacles, 0, _extraColumnCount);
            var lower = _staticRow[bottom];
            if (lower < 0 || _staticRow[bottom + 1] < 0)
                return SpanClear(_queryY[bottom], _queryY[bottom + 1], false, _columnObstacles, _columnStart[column], _columnStart[column + 1]);
            return _verticalClear[column * (_ys.Length - 1) + lower];
        }

        // Node at height y on a vertical line whose spanning obstacles are listed: blocked when y is inside one (open interval).
        private bool NodeFree(float y, int[] columnList, int start, int end)
        {
            for (var i = start; i < end; i++)
            {
                var rect = _obstacles[columnList[i]];
                if (y > rect.yMin && y < rect.yMax) return false;
            }
            return true;
        }

        // Axis segment [a, b] on a line spanned by the listed obstacles is blocked when it overlaps one's open interval.
        private bool SpanClear(float a, float b, bool horizontal, int[] list, int start, int end)
        {
            var low = Math.Min(a, b); var high = Math.Max(a, b);
            for (var i = start; i < end; i++)
            {
                var rect = _obstacles[list[i]];
                if (horizontal ? high > rect.xMin && low < rect.xMax : high > rect.yMin && low < rect.yMax) return false;
            }
            return true;
        }

        // Obstacles whose open interior contains the line x = value (vertical) or y = value (horizontal).
        private int CollectSpanning(float value, bool vertical, int[] into)
        {
            var count = 0;
            for (var i = 0; i < _obstacles.Length; i++)
            {
                var rect = _obstacles[i];
                if (vertical ? value > rect.xMin && value < rect.xMax : value > rect.yMin && value < rect.yMax) into[count++] = i;
            }
            return count;
        }

        private void BuildSpanLists(float[] lines, bool vertical, out int[] start, out int[] indices)
        {
            start = new int[lines.Length + 1];
            var buffer = new int[_obstacles.Length];
            var collected = new List<int>();
            for (var line = 0; line < lines.Length; line++)
            {
                start[line] = collected.Count;
                var count = CollectSpanning(lines[line], vertical, buffer);
                for (var i = 0; i < count; i++) collected.Add(buffer[i]);
            }
            start[lines.Length] = collected.Count;
            indices = collected.ToArray();
        }

        // Bounds, anchor and every inflated obstacle edge that lies inside the bounds; sorted, exact duplicates removed.
        private float[] GridCoordinates(float min, float max, float anchor, bool horizontal)
        {
            var values = new List<float>(_obstacles.Length * 2 + 3) { min, max, anchor };
            foreach (var rect in _obstacles)
            {
                values.Add(horizontal ? rect.xMin : rect.yMin);
                values.Add(horizontal ? rect.xMax : rect.yMax);
            }
            values.Sort();
            var unique = new List<float>(values.Count);
            foreach (var value in values)
                if (value >= min && value <= max && (unique.Count == 0 || unique[unique.Count - 1] != value)) unique.Add(value);
            return unique.ToArray();
        }
    }
}
