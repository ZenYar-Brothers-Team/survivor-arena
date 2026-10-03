using System.Collections.Generic;

namespace Game.ScreenEvents
{
    /// <summary>A stretch of the run with its own pause between events and its own pool of candidate events.</summary>
    public sealed class ScreenEventStage
    {
        public float FromSeconds { get; }
        public float ValleyIntensity { get; }
        public float PeakIntensity { get; }
        public float PauseMinSeconds { get; }
        public float PauseMaxSeconds { get; }
        public IReadOnlyList<ScreenEventPoolEntry> Pool { get; }

        public ScreenEventStage(float fromSeconds, float valleyIntensity, float peakIntensity, float pauseMinSeconds, float pauseMaxSeconds, IReadOnlyList<ScreenEventPoolEntry> pool)
        {
            Game.Content.NumericValidation.ValidateNonNegative(fromSeconds, nameof(fromSeconds));
            Game.Content.NumericValidation.ValidateRange(valleyIntensity, 0f, 1f, nameof(valleyIntensity));
            Game.Content.NumericValidation.ValidateRange(peakIntensity, 0f, 1f, nameof(peakIntensity));
            if (peakIntensity < valleyIntensity)
                throw new System.ArgumentException("peakIntensity is below valleyIntensity.", nameof(peakIntensity));
            Game.Content.NumericValidation.ValidatePositive(pauseMinSeconds, nameof(pauseMinSeconds));
            if (pauseMaxSeconds < pauseMinSeconds)
                throw new System.ArgumentException("pauseMaxSeconds is below pauseMinSeconds.", nameof(pauseMaxSeconds));
            if (pool == null || pool.Count == 0) throw new System.ArgumentException("A stage needs at least one event.", nameof(pool));
            FromSeconds = fromSeconds;
            ValleyIntensity = valleyIntensity;
            PeakIntensity = peakIntensity;
            PauseMinSeconds = pauseMinSeconds;
            PauseMaxSeconds = pauseMaxSeconds;
            Pool = pool;
        }
    }
}
