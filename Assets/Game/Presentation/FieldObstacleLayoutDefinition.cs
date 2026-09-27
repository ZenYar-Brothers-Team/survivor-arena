using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;

namespace Game.Presentation
{
    /// <summary>
    /// Per-run obstacle layout of a field (Game Design «Поля», DECISION-0068). The arena, minus <see cref="EdgeMargin"/>
    /// from its outer edge, is cut into square cells of <see cref="CellSize"/>; every cell receives
    /// <see cref="PatternsPerCell"/> patterns at random points, kept <see cref="CellMargin"/> inside the cell so neighbouring
    /// cells always leave a passage of at least twice that margin. Nothing touches the circle of
    /// <see cref="StartClearRadius"/> around the player start; patterns in one cell keep <see cref="MinPatternGap"/> apart.
    /// Distances are world units.
    /// </summary>
    public sealed class FieldObstacleLayoutDefinition
    {
        public float CellSize { get; }
        public int PatternsPerCell { get; }
        public float EdgeMargin { get; }
        public float CellMargin { get; }
        public float StartClearRadius { get; }
        public float MinPatternGap { get; }
        public int PlacementAttempts { get; }
        /// <summary>Seed used by tests and reference runs; normal runs draw a fresh seed.</summary>
        public int ReferenceSeed { get; }
        public IReadOnlyList<FieldObstaclePattern> Patterns { get; }

        public FieldObstacleLayoutDefinition(float cellSize, int patternsPerCell, float edgeMargin, float cellMargin,
            float startClearRadius, float minPatternGap, int placementAttempts, int referenceSeed, IEnumerable<FieldObstaclePattern> patterns)
        {
            NumericValidation.ValidatePositive(cellSize, nameof(cellSize));
            NumericValidation.ValidateCount(patternsPerCell, nameof(patternsPerCell));
            NumericValidation.ValidateNonNegativeFinite(edgeMargin, nameof(edgeMargin));
            NumericValidation.ValidateNonNegativeFinite(cellMargin, nameof(cellMargin));
            NumericValidation.ValidateNonNegativeFinite(startClearRadius, nameof(startClearRadius));
            NumericValidation.ValidateNonNegativeFinite(minPatternGap, nameof(minPatternGap));
            NumericValidation.ValidateCount(placementAttempts, nameof(placementAttempts));
            var copy = new List<FieldObstaclePattern>(patterns ?? throw new ArgumentNullException(nameof(patterns)));
            if (copy.Count == 0 || copy.Contains(null)) throw new ArgumentException("A layout needs patterns.", nameof(patterns));
            if (copy.Select(p => p.Id).Distinct(StringComparer.Ordinal).Count() != copy.Count)
                throw new ArgumentException("Pattern ids must be unique.", nameof(patterns));
            var interior = cellSize - 2f * cellMargin;
            if (interior <= 0f) throw new ArgumentException("Cell margin leaves no room inside a cell.", nameof(cellMargin));
            foreach (var pattern in copy)
                foreach (var rotation in pattern.Rotations)
                {
                    var bounds = FieldObstacleLayoutGenerator.Bounds(pattern.Pieces.Select(p => p.Rotated(rotation)));
                    if (bounds.width > interior || bounds.height > interior)
                        throw new ArgumentException($"Pattern {pattern.Id} does not fit inside a cell.", nameof(patterns));
                }
            CellSize = cellSize;
            PatternsPerCell = patternsPerCell;
            EdgeMargin = edgeMargin;
            CellMargin = cellMargin;
            StartClearRadius = startClearRadius;
            MinPatternGap = minPatternGap;
            PlacementAttempts = placementAttempts;
            ReferenceSeed = referenceSeed;
            Patterns = copy.AsReadOnly();
        }

        public bool UsesKind(FieldObstacleKind kind) => Patterns.Any(p => p.Pieces.Any(piece => piece.Kind == kind));
    }
}
