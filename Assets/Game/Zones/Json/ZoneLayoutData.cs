namespace Game.Zones.Json
{
    public sealed class ZoneLayoutData
    {
        public bool? SuppressAreaUnitFeedback { get; set; }
        public RandomZoneScheduleData RandomSchedule { get; set; }
        public float? EdgeMargin { get; set; }
        public float? StartClearRadius { get; set; }
        public float? MinGap { get; set; }
        /// <summary>Extra clearance between a zone's rim and any obstacle outline (portals add their exit distance).</summary>
        public float? ObstacleClearance { get; set; }
        /// <summary>Optional altar foundation radius for obstacle clearance; absent keeps the full effect radius.</summary>
        public float? AltarObstacleRadius { get; set; }
        /// <summary>
        /// Zones only work, show and (when pulsing or bursting) reappear within the player's screen widened by this fraction of
        /// the screen size on every side (0.5 = half a screen beyond each edge). Limits the zone workload.
        /// </summary>
        public float? ActiveScreenMargin { get; set; }
        public int? PlacementAttempts { get; set; }
        public int? MaxRestarts { get; set; }
        public int? ReferenceSeed { get; set; }
        public int? MaxPerScreen { get; set; }
        /// <summary>
        /// Optional: how many valid centers the layout compares for each altar so every polarity spreads evenly over the arena
        /// (best candidate wins). Absent or below 2 keeps the plain random scatter.
        /// </summary>
        public int? PolarityMixCandidates { get; set; }
        public float? ScreenPadding { get; set; }
        public ZoneEffectData[] Effects { get; set; }
        public ZoneCountData[] Zones { get; set; }
    }
}
