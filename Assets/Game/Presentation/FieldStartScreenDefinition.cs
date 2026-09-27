using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;

namespace Game.Presentation
{
    /// <summary>Two seeded, diagonally opposite visible props outside the spawn clearance.</summary>
    public sealed class FieldStartScreenDefinition
    {
        public float HalfWidth { get; }
        public float HalfHeight { get; }
        public float MinAbsX { get; }
        public float MaxAbsX { get; }
        public float MinAbsY { get; }
        public float MaxAbsY { get; }
        public IReadOnlyList<string> PatternIds { get; }

        public FieldStartScreenDefinition(float halfWidth, float halfHeight, float minAbsX, float maxAbsX,
            float minAbsY, float maxAbsY, IEnumerable<string> patternIds)
        {
            NumericValidation.ValidatePositive(halfWidth, nameof(halfWidth));
            NumericValidation.ValidatePositive(halfHeight, nameof(halfHeight));
            NumericValidation.ValidatePositive(minAbsX, nameof(minAbsX));
            NumericValidation.ValidatePositive(minAbsY, nameof(minAbsY));
            NumericValidation.ValidatePositive(maxAbsX, nameof(maxAbsX));
            NumericValidation.ValidatePositive(maxAbsY, nameof(maxAbsY));
            if (maxAbsX < minAbsX || maxAbsY < minAbsY)
                throw new ArgumentException("Start-screen coordinate ranges are reversed.");
            var ids = new List<string>(patternIds ?? throw new ArgumentNullException(nameof(patternIds)));
            if (ids.Count == 0 || ids.Any(string.IsNullOrWhiteSpace) || ids.Distinct(StringComparer.Ordinal).Count() != ids.Count)
                throw new ArgumentException("Start-screen patterns need distinct, valid IDs.", nameof(patternIds));
            HalfWidth = halfWidth;
            HalfHeight = halfHeight;
            MinAbsX = minAbsX;
            MaxAbsX = maxAbsX;
            MinAbsY = minAbsY;
            MaxAbsY = maxAbsY;
            PatternIds = ids.AsReadOnly();
        }
    }
}
