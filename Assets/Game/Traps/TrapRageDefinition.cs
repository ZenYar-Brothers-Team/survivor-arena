using System;
using Game.Content;
using Game.Traps.Json;

namespace Game.Traps
{
    /// <summary>
    /// While a trap is active it fires faster: the multiplier grows linearly from 1 to <see cref="MaxMultiplier"/> over
    /// <see cref="SecondsToMax"/> seconds of activity and drops back to 1 the moment it deactivates (DECISION-0156).
    /// </summary>
    public sealed class TrapRageDefinition
    {
        public float MaxMultiplier { get; }
        public float SecondsToMax { get; }

        public TrapRageDefinition(TrapRageData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            MaxMultiplier = data.MaxMultiplier ?? throw new ArgumentException("rage.maxMultiplier is required.");
            SecondsToMax = data.SecondsToMax ?? throw new ArgumentException("rage.secondsToMax is required.");
            if (MaxMultiplier < 1f) throw new ArgumentOutOfRangeException(nameof(MaxMultiplier), "Rage cannot slow a trap down.");
            NumericValidation.ValidatePositive(SecondsToMax, nameof(SecondsToMax));
        }

        /// <summary>Rage progress 0..1 after the given seconds of continuous activity.</summary>
        public float Progress(float activeSeconds) => Math.Min(1f, Math.Max(0f, activeSeconds) / SecondsToMax);

        /// <summary>Fire-rate multiplier after the given seconds of continuous activity (1 .. MaxMultiplier).</summary>
        public float Multiplier(float activeSeconds) => 1f + (MaxMultiplier - 1f) * Progress(activeSeconds);
    }
}
