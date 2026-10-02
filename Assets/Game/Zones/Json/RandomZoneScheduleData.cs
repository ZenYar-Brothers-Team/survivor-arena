namespace Game.Zones.Json
{
    /// <summary>Independent occurrence chains; delays in seconds, spawn radius in current screen widths around the player.</summary>
    public sealed class RandomZoneScheduleData
    {
        public int? Chains { get; set; }
        public float? IntervalMinSeconds { get; set; }
        public float? IntervalMaxSeconds { get; set; }
        public float? SpawnRadiusScreenWidths { get; set; }
        /// <summary>Optional: scheduled portal pairs live on their own extra chain with this delay range and never occupy a general chain.</summary>
        public float? PortalIntervalMinSeconds { get; set; }
        public float? PortalIntervalMaxSeconds { get; set; }
    }
}
