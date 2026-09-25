using Game.Content;

namespace Game.Enemy
{
    /// <summary>Opening window of the run in which ordinary enemies appear just outside the
    /// visible screen instead of on the spawn radius, so the first enemies show up quickly
    /// (DECISION-0057, playtest 2026-09-25_5233a664 OBS-01). Seconds are run time from 0;
    /// the margin is world units beyond the camera view edge.</summary>
    public sealed class WaveOpeningSpawnDefinition
    {
        public float DurationSeconds { get; }
        public float ScreenMargin { get; }

        public WaveOpeningSpawnDefinition(float durationSeconds, float screenMargin)
        {
            NumericValidation.ValidatePositive(durationSeconds, nameof(durationSeconds));
            NumericValidation.ValidateNonNegativeFinite(screenMargin, nameof(screenMargin));
            DurationSeconds = durationSeconds;
            ScreenMargin = screenMargin;
        }
    }
}
