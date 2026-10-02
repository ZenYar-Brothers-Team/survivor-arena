namespace Game.Presentation.Json
{
    /// <summary>Authoring DTO of the circular-platform field geometry study (FIELD-009).</summary>
    public sealed class FieldPlatformLayoutData
    {
        public FieldPlatformArtData Art { get; set; }
        public float? ArenaSideLength { get; set; }
        public float? MinimumRadius { get; set; }
        public float? MaximumRadius { get; set; }
        public float? GiantRadiusMin { get; set; }
        public float? GiantChance { get; set; }
        public float? RadiusSkew { get; set; }
        public float? DeadEndMinimumRadius { get; set; }
        public float? SmallRadiusMax { get; set; }
        public float? BookChance { get; set; }
        public float? EdgeMargin { get; set; }
        public float? MinimumPlatformGap { get; set; }
        public float? MaximumBridgeLength { get; set; }
        public float? BridgeWidth { get; set; }
        public float? BridgeClearance { get; set; }
        public float? TreeMinimumAngleDegrees { get; set; }
        public float? ExtraMinimumAngleDegrees { get; set; }
        public float? EdgeWeightJitterMin { get; set; }
        public float? EdgeWeightJitterMax { get; set; }
        public float? VoidDamagePerSecond { get; set; }
        public float? KeptPlatformFraction { get; set; }
        public int? PlatformCount { get; set; }
        public int? ExtraBridgeCount { get; set; }
        public int? PlacementAttempts { get; set; }
        public int? LayoutAttempts { get; set; }
        public int? CircleSegments { get; set; }
        public int? ReferenceSeed { get; set; }
        public int? BookUpgradeCount { get; set; }
        public string VoidColor { get; set; }
        public string PlatformColor { get; set; }
        public string BridgeColor { get; set; }
        public string StartPlatformColor { get; set; }
    }
}
