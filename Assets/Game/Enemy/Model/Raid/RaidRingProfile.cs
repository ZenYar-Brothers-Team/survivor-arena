using System;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>Ring: participants spread over a circle around the moving player.</summary>
    public sealed class RaidRingProfile
    {
        public float DurationSeconds { get; }
        /// <summary>Radius of the first ring, in camera widths.</summary>
        public float RadiusScreenWidths { get; }

        public RaidRingProfile(float durationSeconds, float radiusScreenWidths)
        {
            NumericValidation.ValidatePositive(durationSeconds, nameof(durationSeconds));
            DurationSeconds = durationSeconds;
            NumericValidation.ValidatePositive(radiusScreenWidths, nameof(radiusScreenWidths));
            RadiusScreenWidths = radiusScreenWidths;
        }
    }
}
