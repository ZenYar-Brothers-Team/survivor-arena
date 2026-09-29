using Game.Content;

namespace Game.Enemy
{
    /// <summary>Run-time opening window that scales continuous spawn cadence without changing phases or burst counts.</summary>
    public sealed class WaveOpeningIntensityDefinition
    {
        public float DurationSeconds { get; }
        public float RateMultiplier { get; }

        public WaveOpeningIntensityDefinition(float durationSeconds, float rateMultiplier)
        {
            NumericValidation.ValidatePositive(durationSeconds, nameof(durationSeconds));
            NumericValidation.ValidateRange(rateMultiplier, 0f, 1f, nameof(rateMultiplier));
            DurationSeconds = durationSeconds;
            RateMultiplier = rateMultiplier;
        }
    }
}
