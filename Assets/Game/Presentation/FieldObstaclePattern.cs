using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;

namespace Game.Presentation
{
    /// <summary>
    /// A fixed arrangement of obstacles (a single stump, a row of columns, a ruined wall corner) that the layout
    /// generator places as a whole with one of its allowed 90° rotations (DECISION-0068).
    /// </summary>
    public sealed class FieldObstaclePattern
    {
        public string Id { get; }
        public float Weight { get; }
        public IReadOnlyList<int> Rotations { get; }
        public IReadOnlyList<FieldObstaclePiece> Pieces { get; }

        public FieldObstaclePattern(string id, float weight, IEnumerable<int> rotations, IEnumerable<FieldObstaclePiece> pieces)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Pattern id is required.", nameof(id));
            NumericValidation.ValidatePositive(weight, nameof(weight));
            var turns = new List<int>(rotations ?? throw new ArgumentNullException(nameof(rotations)));
            if (turns.Count == 0 || turns.Any(r => r != 0 && r != 90 && r != 180 && r != 270) || turns.Distinct().Count() != turns.Count)
                throw new ArgumentException($"Pattern {id} needs distinct rotations from 0/90/180/270.", nameof(rotations));
            var copy = new List<FieldObstaclePiece>(pieces ?? throw new ArgumentNullException(nameof(pieces)));
            if (copy.Count == 0 || copy.Contains(null)) throw new ArgumentException($"Pattern {id} needs pieces.", nameof(pieces));
            for (var i = 0; i < copy.Count; i++)
                for (var j = i + 1; j < copy.Count; j++)
                    if (FieldObstacleLayoutGenerator.Gap(copy[i].X, copy[i].Y, copy[i].Width, copy[i].Height,
                            copy[j].X, copy[j].Y, copy[j].Width, copy[j].Height) < 0f)
                        throw new ArgumentException($"Pattern {id} has overlapping pieces.", nameof(pieces));
            Id = id;
            Weight = weight;
            Rotations = turns.AsReadOnly();
            Pieces = copy.AsReadOnly();
        }
    }
}
