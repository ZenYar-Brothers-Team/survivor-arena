using System;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>Contraction: a full ring whose radius shrinks over the formation time.</summary>
    public sealed class RaidContractionProfile
    {
        public float DurationSeconds { get; }
        /// <summary>Radius at the start, in camera widths.</summary>
        public float StartRadiusScreenWidths { get; }
        /// <summary>Radius at the end, in camera widths.</summary>
        public float EndRadiusScreenWidths { get; }

        public RaidContractionProfile(float durationSeconds, float startRadiusScreenWidths, float endRadiusScreenWidths)
        {
            NumericValidation.ValidatePositive(durationSeconds, nameof(durationSeconds));
            DurationSeconds = durationSeconds;
            NumericValidation.ValidatePositive(startRadiusScreenWidths, nameof(startRadiusScreenWidths));
            StartRadiusScreenWidths = startRadiusScreenWidths;
            NumericValidation.ValidatePositive(endRadiusScreenWidths, nameof(endRadiusScreenWidths));
            EndRadiusScreenWidths = endRadiusScreenWidths;
        }
    }
}
