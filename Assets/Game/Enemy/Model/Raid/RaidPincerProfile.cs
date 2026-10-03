using System;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>Pincer: two columns parallel to the player's heading that close in from both sides.</summary>
    public sealed class RaidPincerProfile
    {
        public float DurationSeconds { get; }
        /// <summary>Column length, in camera widths.</summary>
        public float LengthScreenWidths { get; }
        /// <summary>Distance between the columns at the start, in camera widths.</summary>
        public float StartGapScreenWidths { get; }
        /// <summary>Distance between the columns at the end, in camera widths.</summary>
        public float EndGapScreenWidths { get; }

        public RaidPincerProfile(float durationSeconds, float lengthScreenWidths, float startGapScreenWidths, float endGapScreenWidths)
        {
            NumericValidation.ValidatePositive(durationSeconds, nameof(durationSeconds));
            DurationSeconds = durationSeconds;
            NumericValidation.ValidatePositive(lengthScreenWidths, nameof(lengthScreenWidths));
            LengthScreenWidths = lengthScreenWidths;
            NumericValidation.ValidatePositive(startGapScreenWidths, nameof(startGapScreenWidths));
            StartGapScreenWidths = startGapScreenWidths;
            NumericValidation.ValidatePositive(endGapScreenWidths, nameof(endGapScreenWidths));
            EndGapScreenWidths = endGapScreenWidths;
        }
    }
}
