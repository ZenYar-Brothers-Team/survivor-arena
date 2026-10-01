namespace Game.Zones.Json
{
    public sealed class ZoneLayoutData
    {
        public float? EdgeMargin { get; set; }
        public float? StartClearRadius { get; set; }
        public float? MinGap { get; set; }
        /// <summary>Extra clearance between a zone's rim and any obstacle outline (portals add their exit distance).</summary>
        public float? ObstacleClearance { get; set; }
        public int? PlacementAttempts { get; set; }
        public int? MaxRestarts { get; set; }
        public int? ReferenceSeed { get; set; }
        public ZoneEffectData[] Effects { get; set; }
        public ZoneCountData[] Zones { get; set; }
    }
}
