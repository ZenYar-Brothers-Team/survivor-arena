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
            Vector2 start, int seed, string idPrefix, IReadOnlyList<FieldObstacleExclusion> exclusions = null)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            NumericValidation.ValidatePositive(sideLength, nameof(sideLength));
            if (string.IsNullOrWhiteSpace(idPrefix)) throw new ArgumentException("Obstacle id prefix is required.", nameof(idPrefix));
            using var guard = Game.Diagnostics.PerfGuard.Measure("Field.ObstacleLayout", 100f);
            var random = new System.Random(seed);
            // DECISION-0073: visual variants use their own stream so geometry for a seed stays unchanged.
            var visualRandom = new System.Random(unchecked(seed * 31 + 17));
            var inner = sideLength * .5f - layout.EdgeMargin;
            var cells = Mathf.FloorToInt(2f * inner / layout.CellSize + 1e-4f);
            if (cells < 1) throw new ArgumentException("The arena is smaller than one layout cell.", nameof(sideLength));
            var origin = -cells * layout.CellSize * .5f;
            var totalWeight = layout.Patterns.Sum(p => p.Weight);
            var result = new List<FieldObstacleDefinition>();
            var variantSources = new List<IReadOnlyList<ContentId>>();
            var reserved = ReserveStartScreen(layout, start, idPrefix, random, visualRandom, cells, origin, result, variantSources);
            if (exclusions != null && result.Any(obstacle => exclusions.Any(circle => Distance(
                new Rect(obstacle.X - obstacle.Width * .5f, obstacle.Y - obstacle.Height * .5f, obstacle.Width, obstacle.Height),
                circle.Center) < circle.Radius)))
                throw new InvalidOperationException("Start-screen obstacles intersect a reserved circle.");
            var inCell = new List<Rect>();
            var candidate = new List<Rect>();
            var kinds = new List<FieldObstacleKind>();
            var visualIds = new List<ContentId>();
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
                            visualIds.Clear();
                            foreach (var piece in pieces)
                            {
                                candidate.Add(new Rect(x + piece.X - piece.Width * .5f, y + piece.Y - piece.Height * .5f,
                                    piece.Width, piece.Height));
                                kinds.Add(piece.Kind);
                                visualIds.Add(default);
                            }
                            if (candidate.Any(r => Distance(r, start) < layout.StartClearRadius)) continue;
                            if (exclusions != null && candidate.Any(rect => exclusions.Any(circle =>
                                Distance(rect, circle.Center) < circle.Radius))) continue;
                            if (candidate.Any(a => inCell.Any(b => Gap(a, b) < layout.MinPatternGap))) continue;
                            for (var i = 0; i < candidate.Count; i++)
                            {
                                var r = candidate[i];
                                visualIds[i] = PickVisual(pieces[i], visualRandom);
                                result.Add(new FieldObstacleDefinition($"{idPrefix}-O{result.Count + 1:000}", kinds[i],
                                    r.center.x, r.center.y, r.width, r.height, visualIds[i].ToString()));
                                variantSources.Add(pieces[i].VisualVariants);
                            }
                            inCell.AddRange(candidate);
                            break;
                        }
                }
            EnsureEveryVariantAppears(result, variantSources, visualRandom);
            return result.AsReadOnly();
        }

        /// <summary>
        /// DECISION-0073: every approved visual variant appears at least once per run. A missing variant replaces the
        /// visual of a random obstacle that allows it and whose current visual is repeated elsewhere; geometry is kept.
        /// </summary>
        private static void EnsureEveryVariantAppears(List<FieldObstacleDefinition> result,
            List<IReadOnlyList<ContentId>> variantSources, System.Random visualRandom)
        {
            var counts = new Dictionary<ContentId, int>();
            var required = new List<ContentId>();
            for (var i = 0; i < result.Count; i++)
            {
                if (variantSources[i].Count == 0) continue;
                counts[result[i].VisualId] = counts.TryGetValue(result[i].VisualId, out var count) ? count + 1 : 1;
                foreach (var variant in variantSources[i])
                    if (!required.Contains(variant)) required.Add(variant);
            }
            var candidates = new List<int>();
            foreach (var variant in required)
            {
                if (counts.ContainsKey(variant)) continue;
                candidates.Clear();
                for (var i = 0; i < result.Count; i++)
                    if (variantSources[i].Contains(variant) && counts.TryGetValue(result[i].VisualId, out var shared) && shared > 1)
                        candidates.Add(i);
                if (candidates.Count == 0) continue;
                var index = candidates[visualRandom.Next(candidates.Count)];
                var old = result[index];
                counts[old.VisualId]--;
                counts[variant] = 1;
                result[index] = new FieldObstacleDefinition(old.Id, old.Kind, old.X, old.Y, old.Width, old.Height, variant.ToString());
            }
        }

        private static Dictionary<int, Rect> ReserveStartScreen(FieldObstacleLayoutDefinition layout, Vector2 start,
            string idPrefix, System.Random random, System.Random visualRandom, int cells, float origin,
            List<FieldObstacleDefinition> result, List<IReadOnlyList<ContentId>> variantSources)
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
                    x, y, piece.Width, piece.Height, PickVisual(piece, visualRandom).ToString()));
                variantSources.Add(piece.VisualVariants);
            }
            return reserved;
        }

        private static ContentId PickVisual(FieldObstaclePiece piece, System.Random visualRandom) =>
            piece.VisualVariants.Count == 0 ? piece.VisualId : piece.VisualVariants[visualRandom.Next(piece.VisualVariants.Count)];

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
