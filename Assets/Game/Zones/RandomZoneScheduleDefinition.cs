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
        /// <summary>Spawn centers lie within this many current screen widths of the player.</summary>
        public float SpawnRadiusScreenWidths { get; }
        /// <summary>Delay range of the dedicated portal chain; null when the layout has no scheduled portals.</summary>
        public float? PortalIntervalMinSeconds { get; }
        public float? PortalIntervalMaxSeconds { get; }
        public bool HasPortalChain => PortalIntervalMinSeconds.HasValue;

        public RandomZoneScheduleDefinition(RandomZoneScheduleData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            Chains = data.Chains ?? throw new ArgumentException("randomSchedule.chains is required.");
            IntervalMinSeconds = data.IntervalMinSeconds ?? throw new ArgumentException("randomSchedule.intervalMinSeconds is required.");
            IntervalMaxSeconds = data.IntervalMaxSeconds ?? throw new ArgumentException("randomSchedule.intervalMaxSeconds is required.");
            SpawnRadiusScreenWidths = data.SpawnRadiusScreenWidths ?? throw new ArgumentException("randomSchedule.spawnRadiusScreenWidths is required.");
            NumericValidation.ValidateCount(Chains, nameof(Chains));
            NumericValidation.ValidatePositive(IntervalMinSeconds, nameof(IntervalMinSeconds));
            NumericValidation.ValidateRange(IntervalMaxSeconds, IntervalMinSeconds, float.MaxValue, nameof(IntervalMaxSeconds));
            NumericValidation.ValidatePositive(SpawnRadiusScreenWidths, nameof(SpawnRadiusScreenWidths));
            if (data.PortalIntervalMinSeconds.HasValue != data.PortalIntervalMaxSeconds.HasValue)
                throw new ArgumentException("randomSchedule portal intervals must both be present or both absent.");
            if (data.PortalIntervalMinSeconds.HasValue)
            {
                PortalIntervalMinSeconds = data.PortalIntervalMinSeconds; PortalIntervalMaxSeconds = data.PortalIntervalMaxSeconds;
                NumericValidation.ValidatePositive(PortalIntervalMinSeconds.Value, nameof(PortalIntervalMinSeconds));
                NumericValidation.ValidateRange(PortalIntervalMaxSeconds.Value, PortalIntervalMinSeconds.Value, float.MaxValue, nameof(PortalIntervalMaxSeconds));
            }
        }
    }
}
