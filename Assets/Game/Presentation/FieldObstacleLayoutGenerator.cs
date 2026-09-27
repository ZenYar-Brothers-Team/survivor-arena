using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Places a field's obstacle patterns for one run (DECISION-0068): uniform density by cells, a random pattern (by
    /// weight), rotation and point inside each cell, a clear start circle and passages between cells. The same seed and
    /// arena always give the same layout.
    /// </summary>
    public static class FieldObstacleLayoutGenerator
    {
        public static IReadOnlyList<FieldObstacleDefinition> Generate(FieldObstacleLayoutDefinition layout, float sideLength,
            Vector2 start, int seed, string idPrefix)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            NumericValidation.ValidatePositive(sideLength, nameof(sideLength));
            if (string.IsNullOrWhiteSpace(idPrefix)) throw new ArgumentException("Obstacle id prefix is required.", nameof(idPrefix));
            var random = new System.Random(seed);
            var inner = sideLength * .5f - layout.EdgeMargin;
            var cells = Mathf.FloorToInt(2f * inner / layout.CellSize + 1e-4f);
            if (cells < 1) throw new ArgumentException("The arena is smaller than one layout cell.", nameof(sideLength));
            var origin = -cells * layout.CellSize * .5f;
            var totalWeight = layout.Patterns.Sum(p => p.Weight);
            var result = new List<FieldObstacleDefinition>();
            var reserved = ReserveStartScreen(layout, start, idPrefix, random, cells, origin, result);
            var inCell = new List<Rect>();
            var candidate = new List<Rect>();
            var kinds = new List<FieldObstacleKind>();
            for (var row = 0; row < cells; row++)
                for (var column = 0; column < cells; column++)
                {
                    inCell.Clear();
                    if (reserved.TryGetValue(row * cells + column, out var startRect)) inCell.Add(startRect);
                    var cellMin = new Vector2(origin + column * layout.CellSize + layout.CellMargin,
                        origin + row * layout.CellSize + layout.CellMargin);
                    var cellMax = cellMin + Vector2.one * (layout.CellSize - 2f * layout.CellMargin);
                    for (var placed = inCell.Count; placed < layout.PatternsPerCell; placed++)
                        for (var attempt = 0; attempt < layout.PlacementAttempts; attempt++)
                        {
                            var pattern = Pick(layout.Patterns, totalWeight, random);
                            var rotation = pattern.Rotations[random.Next(pattern.Rotations.Count)];
                            var pieces = pattern.Pieces.Select(p => p.Rotated(rotation)).ToList();
                            var bounds = Bounds(pieces);
                            var x = Range(random, cellMin.x - bounds.xMin, cellMax.x - bounds.xMax);
                            var y = Range(random, cellMin.y - bounds.yMin, cellMax.y - bounds.yMax);
                            candidate.Clear();
                            kinds.Clear();
                            foreach (var piece in pieces)
                            {
                                candidate.Add(new Rect(x + piece.X - piece.Width * .5f, y + piece.Y - piece.Height * .5f,
                                    piece.Width, piece.Height));
                                kinds.Add(piece.Kind);
                            }
                            if (candidate.Any(r => Distance(r, start) < layout.StartClearRadius)) continue;
                            if (candidate.Any(a => inCell.Any(b => Gap(a, b) < layout.MinPatternGap))) continue;
                            for (var i = 0; i < candidate.Count; i++)
                            {
                                var r = candidate[i];
                                result.Add(new FieldObstacleDefinition($"{idPrefix}-O{result.Count + 1:000}", kinds[i],
                                    r.center.x, r.center.y, r.width, r.height));
                            }
                            inCell.AddRange(candidate);
                            break;
                        }
                }
            return result.AsReadOnly();
        }

        private static Dictionary<int, Rect> ReserveStartScreen(FieldObstacleLayoutDefinition layout, Vector2 start,
            string idPrefix, System.Random random, int cells, float origin, List<FieldObstacleDefinition> result)
        {
            var reserved = new Dictionary<int, Rect>();
            var config = layout.StartScreen;
            if (config == null) return reserved;
            var diagonal = random.Next(2) == 0 ? 1 : -1;
            for (var index = 0; index < 2; index++)
            {
                var signX = index == 0 ? -1 : 1;
                var signY = index == 0 ? diagonal : -diagonal;
                var id = config.PatternIds[random.Next(config.PatternIds.Count)];
                var piece = layout.Patterns.First(pattern => pattern.Id == id).Pieces[0];
                var x = start.x + signX * Range(random, config.MinAbsX, config.MaxAbsX);
                var y = start.y + signY * Range(random, config.MinAbsY, config.MaxAbsY);
                var rect = new Rect(x - piece.Width * .5f, y - piece.Height * .5f, piece.Width, piece.Height);
                var column = Mathf.FloorToInt((x - origin) / layout.CellSize);
                var row = Mathf.FloorToInt((y - origin) / layout.CellSize);
                var cellMin = new Vector2(origin + column * layout.CellSize + layout.CellMargin,
                    origin + row * layout.CellSize + layout.CellMargin);
                var cellMax = cellMin + Vector2.one * (layout.CellSize - 2f * layout.CellMargin);
                if (column < 0 || column >= cells || row < 0 || row >= cells ||
                    rect.xMin < cellMin.x || rect.yMin < cellMin.y || rect.xMax > cellMax.x || rect.yMax > cellMax.y ||
                    Distance(rect, start) < layout.StartClearRadius || !reserved.TryAdd(row * cells + column, rect))
                    throw new InvalidOperationException("Start-screen obstacle configuration does not fit its clear layout cell.");
                result.Add(new FieldObstacleDefinition($"{idPrefix}-O{result.Count + 1:000}", piece.Kind,
                    x, y, piece.Width, piece.Height));
            }
            return reserved;
        }

        private static FieldObstaclePattern Pick(IReadOnlyList<FieldObstaclePattern> patterns, float totalWeight, System.Random random)
        {
            var roll = (float)random.NextDouble() * totalWeight;
            foreach (var pattern in patterns)
            {
                roll -= pattern.Weight;
                if (roll < 0f) return pattern;
            }
            return patterns[patterns.Count - 1];
        }

        private static float Range(System.Random random, float min, float max) =>
            max <= min ? (min + max) * .5f : min + (float)random.NextDouble() * (max - min);

        /// <summary>Axis-aligned bounds of pattern pieces around the pattern center.</summary>
        public static Rect Bounds(IEnumerable<FieldObstaclePiece> pieces)
        {
            var list = pieces.ToList();
            return Rect.MinMaxRect(list.Min(p => p.X - p.Width * .5f), list.Min(p => p.Y - p.Height * .5f),
                list.Max(p => p.X + p.Width * .5f), list.Max(p => p.Y + p.Height * .5f));
        }

        /// <summary>Clear distance between two rectangles along the more separated axis; negative when they overlap.</summary>
        public static float Gap(float ax, float ay, float aw, float ah, float bx, float by, float bw, float bh) =>
            Mathf.Max(Mathf.Abs(ax - bx) - (aw + bw) * .5f, Mathf.Abs(ay - by) - (ah + bh) * .5f);

        public static float Gap(Rect a, Rect b) =>
            Gap(a.center.x, a.center.y, a.width, a.height, b.center.x, b.center.y, b.width, b.height);

        /// <summary>Distance from a point to the nearest point of a rectangle (0 inside).</summary>
        public static float Distance(Rect rect, Vector2 point)
        {
            var dx = Mathf.Max(Mathf.Abs(point.x - rect.center.x) - rect.width * .5f, 0f);
            var dy = Mathf.Max(Mathf.Abs(point.y - rect.center.y) - rect.height * .5f, 0f);
            return Mathf.Sqrt(dx * dx + dy * dy);
        }
    }
}
