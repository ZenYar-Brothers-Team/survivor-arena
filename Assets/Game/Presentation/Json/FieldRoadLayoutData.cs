namespace Game.Presentation.Json
{
    public sealed class FieldRoadLayoutData
    {
        public float? ArenaSideLength { get; set; }
        public float? MainRoadWidth { get; set; }
        public float? DeadEndWidth { get; set; }
        public float? DeadEndLengthMin { get; set; }
        public float? DeadEndLengthMax { get; set; }
        public float? DeadEndEndRadius { get; set; }
        public float? IndependentSurfaceGap { get; set; }
        public float? InteriorNodeGap { get; set; }
        public float? MinimumCycleLength { get; set; }
        public float? MinimumJunctionAngleDegrees { get; set; }
        public float? IndependentAxisGap { get; set; }
        public float? BookEntranceGap { get; set; }
        public float? BookEntranceJunctionGap { get; set; }
        public float? RingInset { get; set; }
        public float? RingCornerRadius { get; set; }
        public float? InteriorMargin { get; set; }
        public float? AnchorCornerMargin { get; set; }
        public float? MinimumEdgeLength { get; set; }
        public float? MaximumExtraEdgeLength { get; set; }
        public float? AnchorGap { get; set; }
        public float? BendTrim { get; set; }
        public float? BranchTiltDegrees { get; set; }
        public float? ParentOverlapRadius { get; set; }
        public float? FieldPadding { get; set; }
        public float? SurfaceStep { get; set; }
        public int? InteriorNodeCountMin { get; set; }
        public int? InteriorNodeCountMax { get; set; }
        public int? AdditionalLinkCountMin { get; set; }
        public int? AdditionalLinkCountMax { get; set; }
        public int? MinimumRingConnections { get; set; }
        public int? BookPlacementAttempts { get; set; }
        public int? MinimumDeadEnds { get; set; }
        public int? GraphAttempts { get; set; }
        public int? LayoutAttempts { get; set; }
        public int? NodePlacementAttempts { get; set; }
        public int? RingArcSamples { get; set; }
        public int? BendSamples { get; set; }
        public int? ReferenceSeed { get; set; }
        public int? BookUpgradeCount { get; set; }
        public string MainColor { get; set; }
        public string DeadEndColor { get; set; }
        public float? DeadEndMouthOverlap { get; set; }
    }
}
