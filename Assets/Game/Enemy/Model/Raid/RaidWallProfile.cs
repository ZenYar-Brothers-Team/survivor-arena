using System;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>Wall: a line perpendicular to the player's average heading, ahead of the player. The heading is sampled for SampleSeconds after launch (participants move as usual meanwhile).</summary>
    public sealed class RaidWallProfile
    {
        public float DurationSeconds { get; }
        /// <summary>Heading sampling window; the wall forms for the rest of the duration.</summary>
        public float SampleSeconds { get; }
        /// <summary>Distance of the front row from the player, in camera widths.</summary>
        public float DistanceScreenWidths { get; }
        /// <summary>Wall length, in camera widths.</summary>
        public float WidthScreenWidths { get; }

        public RaidWallProfile(float durationSeconds, float sampleSeconds, float distanceScreenWidths, float widthScreenWidths)
        {
            NumericValidation.ValidatePositive(durationSeconds, nameof(durationSeconds));
            DurationSeconds = durationSeconds;
            NumericValidation.ValidatePositive(sampleSeconds, nameof(sampleSeconds));
            SampleSeconds = sampleSeconds;
            NumericValidation.ValidatePositive(distanceScreenWidths, nameof(distanceScreenWidths));
            DistanceScreenWidths = distanceScreenWidths;
            NumericValidation.ValidatePositive(widthScreenWidths, nameof(widthScreenWidths));
            WidthScreenWidths = widthScreenWidths;
            if (sampleSeconds >= durationSeconds) throw new ArgumentOutOfRangeException(nameof(sampleSeconds), "Sampling must be shorter than the wall duration.");
        }
    }
}
