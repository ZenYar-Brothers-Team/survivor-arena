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
        /// <summary>Spread of the group around the opposite-of-mass direction, 0 = ordinary biased scatter,
        /// otherwise the group stands as an evenly spaced arc of this many degrees (DECISION-0146).</summary>
        public float ArcDegrees { get; }

        public WaveBurstDefinition(int count, float offsetSeconds, float windowSeconds, float arcDegrees = 0f)
        {
            NumericValidation.ValidateNonNegative(count, nameof(count));
            NumericValidation.ValidateNonNegativeFinite(offsetSeconds, nameof(offsetSeconds));
            NumericValidation.ValidatePositive(windowSeconds, nameof(windowSeconds));
            NumericValidation.ValidateRange(arcDegrees, 0f, 360f, nameof(arcDegrees));
            Count = count;
            OffsetSeconds = offsetSeconds;
            WindowSeconds = windowSeconds;
            ArcDegrees = arcDegrees;
        }
    }
}
