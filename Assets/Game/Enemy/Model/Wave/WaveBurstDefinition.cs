using System;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>One group at the first running tick in [offset, offset + window).
    /// Seconds are relative to the phase start; zero count is an explicit empty group.</summary>
    public sealed class WaveBurstDefinition
    {
        public int Count { get; }
        public float OffsetSeconds { get; }
        public float WindowSeconds { get; }

        public WaveBurstDefinition(int count, float offsetSeconds, float windowSeconds)
        {
            NumericValidation.ValidateNonNegative(count, nameof(count));
            NumericValidation.ValidateNonNegativeFinite(offsetSeconds, nameof(offsetSeconds));
            NumericValidation.ValidatePositive(windowSeconds, nameof(windowSeconds));
            Count = count;
            OffsetSeconds = offsetSeconds;
            WindowSeconds = windowSeconds;
        }
    }
}
