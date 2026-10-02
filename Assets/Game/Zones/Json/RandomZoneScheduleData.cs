namespace Game.Zones.Json
{
    /// <summary>Independent occurrence chains; delays in seconds, spawn padding in world units.</summary>
    public sealed class RandomZoneScheduleData
    {
        public int? Chains { get; set; }
        public float? IntervalMinSeconds { get; set; }
        public float? IntervalMaxSeconds { get; set; }
        public float? ScreenMarginWorldUnits { get; set; }
    }
}
