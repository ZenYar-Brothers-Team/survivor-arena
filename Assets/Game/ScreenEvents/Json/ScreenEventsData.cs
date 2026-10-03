namespace Game.ScreenEvents.Json
{
    /// <summary>Authoring shape of a field's screen events (DECISION-0157): schedule plus the events it draws from.</summary>
    public sealed class ScreenEventsData
    {
        public int? ReferenceSeed { get; set; }
        /// <summary>Radius of the player's body used by every hit test, in world units.</summary>
        public float? PlayerHitRadius { get; set; }
        /// <summary>The first event starts this many Running seconds after the run starts.</summary>
        public float? FirstDelaySeconds { get; set; }
        /// <summary>Rare events are at least this far apart (from the end of the previous rare event).</summary>
        public float? RareMinIntervalSeconds { get; set; }
        /// <summary>While a boss is alive no new event starts and its timer waits.</summary>
        public bool? SuspendWhileBossAlive { get; set; }
        /// <summary>Length of one wave of intensity: calm, build-up, peak, ease-off.</summary>
        public float? WavePeriodSeconds { get; set; }
        /// <summary>How close an event's intensity must be to the current one to be picked (larger = less picky).</summary>
        public float? IntensityTolerance { get; set; }
        /// <summary>Rare events only start while the wave is at least this high.</summary>
        public float? RareMinIntensity { get; set; }
        /// <summary>Walking speed (world units per second) a fair event must be survivable at; below the slowest character.</summary>
        public float? FairnessSpeed { get; set; }
        /// <summary>How many times an unsurvivable layout is re-rolled before the event runs anyway.</summary>
        public int? FairnessAttempts { get; set; }
        /// <summary>Cell size of the survivability search, in world units.</summary>
        public float? FairnessCellSize { get; set; }
        public ScreenEventStageData[] Stages { get; set; }
        public ScreenEventData[] Events { get; set; }
    }
}
