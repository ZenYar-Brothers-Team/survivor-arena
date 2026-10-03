using System;
using UnityEngine;

namespace Game.ScreenEvents
{
    /// <summary>
    /// Fairness check of a screen event (DECISION-0157): can a player who starts at <c>player</c> and walks at <c>speed</c> inside the
    /// visible rectangle get through the whole event without touching a single hazard? The visible area is cut into square cells and a
    /// set of reachable cells is advanced in time steps of one cell's travel; a cell stays in the set only while no hazard covers it.
    /// Diagonal steps alternate with straight ones, so the approximation is within about 20% of straight-line speed, which the content's
    /// fairness speed (below the slowest character) is chosen to absorb. Pure function of its arguments; used by the runtime to re-roll
    /// an unfair layout and by tests to prove every layout can be survived.
    /// </summary>
    public static class ScreenEventSurvival
    {
        private const int MaxCells = 6000;
        private const int MaxChecksPerStep = 8;

        public static bool CanSurvive(ScreenEventInstance instance, Rect view, Vector2 player, float playerRadius, float speed, float cellSize)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            Game.Content.NumericValidation.ValidatePositive(speed, nameof(speed));
            Game.Content.NumericValidation.ValidatePositive(cellSize, nameof(cellSize));
            var columns = Mathf.Max(1, Mathf.CeilToInt(view.width / cellSize));
            var rows = Mathf.Max(1, Mathf.CeilToInt(view.height / cellSize));
            if (columns * rows > MaxCells)
            {
                // A very large view: coarser cells keep the cost bounded.
                var scale = Mathf.Sqrt((float)columns * rows / MaxCells);
                cellSize *= scale;
                columns = Mathf.Max(1, Mathf.CeilToInt(view.width / cellSize));
                rows = Mathf.Max(1, Mathf.CeilToInt(view.height / cellSize));
            }
            var current = new bool[columns * rows];
            var next = new bool[columns * rows];
            var startColumn = Mathf.Clamp(Mathf.FloorToInt((player.x - view.xMin) / cellSize), 0, columns - 1);
            var startRow = Mathf.Clamp(Mathf.FloorToInt((player.y - view.yMin) / cellSize), 0, rows - 1);
            current[startRow * columns + startColumn] = true;
            var step = cellSize / speed;
            // A fast hazard must not slip through a thin band between two time samples: each step is checked at enough
            // sub-times that no hazard moves further than one player body per check.
            var fastest = 0f;
            foreach (var hazard in instance.Hazards)
                if (hazard.Kind != ScreenHazardKind.Burst) fastest = Mathf.Max(fastest, hazard.Speed);
            var checks = Mathf.Clamp(Mathf.CeilToInt(fastest * step / (2f * playerRadius)), 1, MaxChecksPerStep);
            var stepIndex = 0;
            for (var time = step; time <= instance.Duration + step; time += step, stepIndex++)
            {
                var diagonal = (stepIndex & 1) == 0;
                var any = false;
                Array.Clear(next, 0, next.Length);
                for (var row = 0; row < rows; row++)
                for (var column = 0; column < columns; column++)
                {
                    if (!Reachable(current, columns, rows, column, row, diagonal)) continue;
                    var point = new Vector2(view.xMin + (column + .5f) * cellSize, view.yMin + (row + .5f) * cellSize);
                    if (Dangerous(instance, time, step, checks, point, playerRadius)) continue;
                    next[row * columns + column] = true;
                    any = true;
                }
                if (!any) return false;
                (current, next) = (next, current);
            }
            return true;
        }

        private static bool Reachable(bool[] set, int columns, int rows, int column, int row, bool diagonal)
        {
            if (set[row * columns + column]) return true;
            if (column > 0 && set[row * columns + column - 1]) return true;
            if (column + 1 < columns && set[row * columns + column + 1]) return true;
            if (row > 0 && set[(row - 1) * columns + column]) return true;
            if (row + 1 < rows && set[(row + 1) * columns + column]) return true;
            if (!diagonal) return false;
            if (column > 0 && row > 0 && set[(row - 1) * columns + column - 1]) return true;
            if (column + 1 < columns && row > 0 && set[(row - 1) * columns + column + 1]) return true;
            if (column > 0 && row + 1 < rows && set[(row + 1) * columns + column - 1]) return true;
            return column + 1 < columns && row + 1 < rows && set[(row + 1) * columns + column + 1];
        }

        private static bool Dangerous(ScreenEventInstance instance, float time, float step, int checks, Vector2 point, float playerRadius)
        {
            for (var check = 0; check < checks; check++)
            {
                var at = time - step * check / checks;
                foreach (var hazard in instance.Hazards)
                    if (hazard.Covers(hazard.StrikeTimeAt(at), point, playerRadius)) return true;
            }
            return false;
        }
    }
}
