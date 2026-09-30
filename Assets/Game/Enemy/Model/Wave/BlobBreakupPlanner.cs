using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>Checks disjoint squares around the player and assigns a staggered fan in every dense square.</summary>
    public static class BlobBreakupPlanner
    {
        public const int GridSide = 7;
        public const int GridLength = GridSide * GridSide;
        private const int CenterCellIndex = GridLength / 2;
        private const float CellSize = 3.5f;

        public struct Cell
        {
            internal int Count;
            internal int Selected;
            internal Vector2 PositionSum;
        }

        public static int Plan(IReadOnlyList<Vector2> positions, Vector2 player,
            BlobBreakupDefinition settings, System.Random random, Cell[] grid,
            List<int> selected, List<Vector2> waypoints, List<float> delays,
            IReadOnlyList<bool> eligible = null)
        {
            if (positions == null || settings == null || random == null ||
                grid == null || grid.Length != GridLength || selected == null ||
                waypoints == null || delays == null ||
                (eligible != null && eligible.Count != positions.Count))
                throw new ArgumentException("Blob breakup requires positions, settings, RNG and reusable buffers.");
            Array.Clear(grid, 0, grid.Length);
            selected.Clear();
            waypoints.Clear();
            delays.Clear();
            for (var i = 0; i < positions.Count; i++)
            {
                if (!TryCell(positions[i], player, out var cellIndex) ||
                    cellIndex == CenterCellIndex) continue;
                grid[cellIndex].Count++;
                grid[cellIndex].PositionSum += positions[i];
            }

            if (positions.Count == 0) return 0;
            // Rotate the first candidate without allocating or favoring list order.
            var start = random.Next(positions.Count);
            for (var offsetIndex = 0; offsetIndex < positions.Count; offsetIndex++)
            {
                var i = (start + offsetIndex) % positions.Count;
                if (!TryCell(positions[i], player, out var cellIndex)) continue;
                ref var cell = ref grid[cellIndex];
                if (cell.Count < settings.MinimumClusterCount ||
                    (eligible != null && !eligible[i]) ||
                    cell.Selected >= settings.MaxSelected ||
                    random.NextDouble() >= settings.SelectionFraction)
                    continue;
                var center = cell.PositionSum / cell.Count;
                var baseDirection = player - center;
                if (baseDirection.sqrMagnitude < .01f) baseDirection = Vector2.right;
                baseDirection.Normalize();
                // Preserve each enemy's lateral side of the group to avoid opposing flows.
                var fromCenter = positions[i] - center;
                var lateral = baseDirection.x * fromCenter.y - baseDirection.y * fromCenter.x;
                var side = lateral > .01f ? 1f : lateral < -.01f ? -1f :
                    cell.Selected % 2 == 0 ? 1f : -1f;
                var angle = side * Mathf.Lerp(25f, settings.ConeHalfAngleDegrees, (float)random.NextDouble()) * Mathf.Deg2Rad;
                var direction = new Vector2(
                    baseDirection.x * Mathf.Cos(angle) - baseDirection.y * Mathf.Sin(angle),
                    baseDirection.x * Mathf.Sin(angle) + baseDirection.y * Mathf.Cos(angle));
                var distance = Vector2.Distance(center, player) + settings.OvershootDistance;
                selected.Add(i);
                waypoints.Add(center + direction * distance);
                delays.Add((float)random.NextDouble() * settings.StaggerSeconds);
                cell.Selected++;
            }
            return selected.Count;
        }

        private static bool TryCell(Vector2 position, Vector2 player, out int index)
        {
            var x = Mathf.FloorToInt((position.x - player.x) / CellSize + GridSide * .5f);
            var y = Mathf.FloorToInt((position.y - player.y) / CellSize + GridSide * .5f);
            index = y * GridSide + x;
            return x >= 0 && x < GridSide && y >= 0 && y < GridSide;
        }
    }
}
