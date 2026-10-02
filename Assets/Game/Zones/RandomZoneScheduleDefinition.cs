using System;
using Game.Content;
using Game.Zones.Json;

namespace Game.Zones
{
    /// <summary>Opt-in random appearances (DECISION-0142): one occupied slot per chain, delay starts after disappearance.</summary>
    public sealed class RandomZoneScheduleDefinition
    {
        public int Chains { get; }
        public float IntervalMinSeconds { get; }
        public float IntervalMaxSeconds { get; }
        public float ScreenMarginWorldUnits { get; }

        public RandomZoneScheduleDefinition(RandomZoneScheduleData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            Chains = data.Chains ?? throw new ArgumentException("randomSchedule.chains is required.");
            IntervalMinSeconds = data.IntervalMinSeconds ?? throw new ArgumentException("randomSchedule.intervalMinSeconds is required.");
            IntervalMaxSeconds = data.IntervalMaxSeconds ?? throw new ArgumentException("randomSchedule.intervalMaxSeconds is required.");
            ScreenMarginWorldUnits = data.ScreenMarginWorldUnits ?? throw new ArgumentException("randomSchedule.screenMarginWorldUnits is required.");
            NumericValidation.ValidateCount(Chains, nameof(Chains));
            NumericValidation.ValidatePositive(IntervalMinSeconds, nameof(IntervalMinSeconds));
            NumericValidation.ValidateRange(IntervalMaxSeconds, IntervalMinSeconds, float.MaxValue, nameof(IntervalMaxSeconds));
            NumericValidation.ValidateNonNegativeFinite(ScreenMarginWorldUnits, nameof(ScreenMarginWorldUnits));
        }
    }
}
