using System;
using System.Collections.Generic;

namespace Game.ScreenEvents
{
    /// <summary>One running occurrence of a screen event: its hazards and its own clock (DECISION-0157).</summary>
    public sealed class ScreenEventInstance
    {
        public ScreenEventDefinition Definition { get; }
        public IReadOnlyList<ScreenHazard> Hazards { get; }
        /// <summary>Seconds since the event started; advances only while the run is Running.</summary>
        public float Elapsed { get; internal set; }
        /// <summary>When the last hazard finishes.</summary>
        public float Duration { get; }
        public bool IsFinished => Elapsed >= Duration;

        public ScreenEventInstance(ScreenEventDefinition definition, IReadOnlyList<ScreenHazard> hazards)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Hazards = hazards ?? throw new ArgumentNullException(nameof(hazards));
            if (hazards.Count == 0) throw new ArgumentException("An event needs at least one hazard.", nameof(hazards));
            var duration = 0f;
            foreach (var hazard in hazards) duration = Math.Max(duration, hazard.TotalSeconds);
            Duration = duration;
        }
    }
}
