using System;
using Game.Content;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Validated FIELD-003 generation and surface profile; world units.</summary>
    public sealed class FieldRoadLayoutDefinition
    {
        public float ArenaSideLength { get; }
        public float MainRoadWidth { get; }
        public float DeadEndWidth { get; }
        public float DeadEndLengthMin { get; }
        public float DeadEndLengthMax { get; }
        public float DeadEndEndRadius { get; }
        public float IndependentSurfaceGap { get; }
        public float InteriorNodeGap { get; }
        public float MinimumCycleLength { get; }
        public float MinimumJunctionAngleDegrees { get; }
        public float IndependentAxisGap { get; }
        public float BookEntranceGap { get; }
        public float BookEntranceJunctionGap { get; }
        public float RingInset { get; }
        public float RingCornerRadius { get; }
        public float InteriorMargin { get; }
        public float AnchorCornerMargin { get; }
        public float MinimumEdgeLength { get; }
        public float MaximumExtraEdgeLength { get; }
        public float AnchorGap { get; }
        public float BendTrim { get; }
        public float BranchTiltDegrees { get; }
        public float ParentOverlapRadius { get; }
        public float FieldPadding { get; }
        public float SurfaceStep { get; }
        public int InteriorNodeCountMin { get; }
        public int InteriorNodeCountMax { get; }
        public int AdditionalLinkCountMin { get; }
        public int AdditionalLinkCountMax { get; }
        public int MinimumRingConnections { get; }
        public int BookPlacementAttempts { get; }
        public int MinimumDeadEnds { get; }
        public int GraphAttempts { get; }
        public int LayoutAttempts { get; }
        public int NodePlacementAttempts { get; }
        public int RingArcSamples { get; }
        public int BendSamples { get; }
        public int ReferenceSeed { get; }
        public int BookUpgradeCount { get; }
        public Color MainColor { get; }
        public Color DeadEndColor { get; }
        public FieldRoadLayoutDefinition(FieldRoadLayoutData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            ArenaSideLength = data.ArenaSideLength ?? throw new ArgumentException("arenaSideLength is required.");
            NumericValidation.ValidatePositive(ArenaSideLength, nameof(ArenaSideLength));
            MainRoadWidth = data.MainRoadWidth ?? throw new ArgumentException("mainRoadWidth is required.");
            NumericValidation.ValidatePositive(MainRoadWidth, nameof(MainRoadWidth));
            DeadEndWidth = data.DeadEndWidth ?? throw new ArgumentException("deadEndWidth is required.");
            NumericValidation.ValidatePositive(DeadEndWidth, nameof(DeadEndWidth));
            DeadEndLengthMin = data.DeadEndLengthMin ?? throw new ArgumentException("deadEndLengthMin is required.");
            NumericValidation.ValidatePositive(DeadEndLengthMin, nameof(DeadEndLengthMin));
            DeadEndLengthMax = data.DeadEndLengthMax ?? throw new ArgumentException("deadEndLengthMax is required.");
            NumericValidation.ValidatePositive(DeadEndLengthMax, nameof(DeadEndLengthMax));
            DeadEndEndRadius = data.DeadEndEndRadius ?? throw new ArgumentException("deadEndEndRadius is required.");
            NumericValidation.ValidatePositive(DeadEndEndRadius, nameof(DeadEndEndRadius));
            IndependentSurfaceGap = data.IndependentSurfaceGap ?? throw new ArgumentException("independentSurfaceGap is required.");
            NumericValidation.ValidatePositive(IndependentSurfaceGap, nameof(IndependentSurfaceGap));
            InteriorNodeGap = data.InteriorNodeGap ?? throw new ArgumentException("interiorNodeGap is required.");
            NumericValidation.ValidatePositive(InteriorNodeGap, nameof(InteriorNodeGap));
            MinimumCycleLength = data.MinimumCycleLength ?? throw new ArgumentException("minimumCycleLength is required.");
            NumericValidation.ValidatePositive(MinimumCycleLength, nameof(MinimumCycleLength));
            MinimumJunctionAngleDegrees = data.MinimumJunctionAngleDegrees ?? throw new ArgumentException("minimumJunctionAngleDegrees is required.");
            NumericValidation.ValidatePositive(MinimumJunctionAngleDegrees, nameof(MinimumJunctionAngleDegrees));
            IndependentAxisGap = data.IndependentAxisGap ?? throw new ArgumentException("independentAxisGap is required.");
            NumericValidation.ValidatePositive(IndependentAxisGap, nameof(IndependentAxisGap));
            BookEntranceGap = data.BookEntranceGap ?? throw new ArgumentException("bookEntranceGap is required.");
            NumericValidation.ValidatePositive(BookEntranceGap, nameof(BookEntranceGap));
            BookEntranceJunctionGap = data.BookEntranceJunctionGap ?? throw new ArgumentException("bookEntranceJunctionGap is required.");
            NumericValidation.ValidatePositive(BookEntranceJunctionGap, nameof(BookEntranceJunctionGap));
            RingInset = data.RingInset ?? throw new ArgumentException("ringInset is required.");
            NumericValidation.ValidatePositive(RingInset, nameof(RingInset));
            RingCornerRadius = data.RingCornerRadius ?? throw new ArgumentException("ringCornerRadius is required.");
            NumericValidation.ValidatePositive(RingCornerRadius, nameof(RingCornerRadius));
            InteriorMargin = data.InteriorMargin ?? throw new ArgumentException("interiorMargin is required.");
            NumericValidation.ValidatePositive(InteriorMargin, nameof(InteriorMargin));
            AnchorCornerMargin = data.AnchorCornerMargin ?? throw new ArgumentException("anchorCornerMargin is required.");
            NumericValidation.ValidatePositive(AnchorCornerMargin, nameof(AnchorCornerMargin));
            MinimumEdgeLength = data.MinimumEdgeLength ?? throw new ArgumentException("minimumEdgeLength is required.");
            NumericValidation.ValidatePositive(MinimumEdgeLength, nameof(MinimumEdgeLength));
            MaximumExtraEdgeLength = data.MaximumExtraEdgeLength ?? throw new ArgumentException("maximumExtraEdgeLength is required.");
            NumericValidation.ValidatePositive(MaximumExtraEdgeLength, nameof(MaximumExtraEdgeLength));
            AnchorGap = data.AnchorGap ?? throw new ArgumentException("anchorGap is required.");
            NumericValidation.ValidatePositive(AnchorGap, nameof(AnchorGap));
            BendTrim = data.BendTrim ?? throw new ArgumentException("bendTrim is required.");
            NumericValidation.ValidatePositive(BendTrim, nameof(BendTrim));
            BranchTiltDegrees = data.BranchTiltDegrees ?? throw new ArgumentException("branchTiltDegrees is required.");
            NumericValidation.ValidatePositive(BranchTiltDegrees, nameof(BranchTiltDegrees));
            ParentOverlapRadius = data.ParentOverlapRadius ?? throw new ArgumentException("parentOverlapRadius is required.");
            NumericValidation.ValidatePositive(ParentOverlapRadius, nameof(ParentOverlapRadius));
            FieldPadding = data.FieldPadding ?? throw new ArgumentException("fieldPadding is required.");
            NumericValidation.ValidatePositive(FieldPadding, nameof(FieldPadding));
            SurfaceStep = data.SurfaceStep ?? throw new ArgumentException("surfaceStep is required.");
            NumericValidation.ValidatePositive(SurfaceStep, nameof(SurfaceStep));
            InteriorNodeCountMin = data.InteriorNodeCountMin ?? throw new ArgumentException("interiorNodeCountMin is required.");
            NumericValidation.ValidateCount(InteriorNodeCountMin, nameof(InteriorNodeCountMin));
            InteriorNodeCountMax = data.InteriorNodeCountMax ?? throw new ArgumentException("interiorNodeCountMax is required.");
            NumericValidation.ValidateCount(InteriorNodeCountMax, nameof(InteriorNodeCountMax));
            AdditionalLinkCountMin = data.AdditionalLinkCountMin ?? throw new ArgumentException("additionalLinkCountMin is required.");
            NumericValidation.ValidateCount(AdditionalLinkCountMin, nameof(AdditionalLinkCountMin));
            AdditionalLinkCountMax = data.AdditionalLinkCountMax ?? throw new ArgumentException("additionalLinkCountMax is required.");
            NumericValidation.ValidateCount(AdditionalLinkCountMax, nameof(AdditionalLinkCountMax));
            MinimumRingConnections = data.MinimumRingConnections ?? throw new ArgumentException("minimumRingConnections is required.");
            NumericValidation.ValidateCount(MinimumRingConnections, nameof(MinimumRingConnections));
            BookPlacementAttempts = data.BookPlacementAttempts ?? throw new ArgumentException("bookPlacementAttempts is required.");
            NumericValidation.ValidateCount(BookPlacementAttempts, nameof(BookPlacementAttempts));
            MinimumDeadEnds = data.MinimumDeadEnds ?? throw new ArgumentException("minimumDeadEnds is required.");
            NumericValidation.ValidateCount(MinimumDeadEnds, nameof(MinimumDeadEnds));
            GraphAttempts = data.GraphAttempts ?? throw new ArgumentException("graphAttempts is required.");
            NumericValidation.ValidateCount(GraphAttempts, nameof(GraphAttempts));
            LayoutAttempts = data.LayoutAttempts ?? throw new ArgumentException("layoutAttempts is required.");
            NumericValidation.ValidateCount(LayoutAttempts, nameof(LayoutAttempts));
            NodePlacementAttempts = data.NodePlacementAttempts ?? throw new ArgumentException("nodePlacementAttempts is required.");
            NumericValidation.ValidateCount(NodePlacementAttempts, nameof(NodePlacementAttempts));
            RingArcSamples = data.RingArcSamples ?? throw new ArgumentException("ringArcSamples is required.");
            NumericValidation.ValidateCount(RingArcSamples, nameof(RingArcSamples));
            BendSamples = data.BendSamples ?? throw new ArgumentException("bendSamples is required.");
            NumericValidation.ValidateCount(BendSamples, nameof(BendSamples));
            ReferenceSeed = data.ReferenceSeed ?? throw new ArgumentException("referenceSeed is required.");
            BookUpgradeCount = data.BookUpgradeCount ?? throw new ArgumentException("bookUpgradeCount is required.");
            NumericValidation.ValidateCount(BookUpgradeCount, nameof(BookUpgradeCount));
            if (!ColorUtility.TryParseHtmlString(data.MainColor, out var main) || !ColorUtility.TryParseHtmlString(data.DeadEndColor, out var branch))
                throw new ArgumentException("Road colors are required HTML colors.");
            MainColor = main; DeadEndColor = branch;
            if (InteriorNodeCountMax < InteriorNodeCountMin || AdditionalLinkCountMax < AdditionalLinkCountMin || DeadEndLengthMax < DeadEndLengthMin ||
                RingArcSamples < 2 || BendSamples < 2 || BookUpgradeCount != 1 || SurfaceStep > 1f ||
                RingInset <= MainRoadWidth * .5f + FieldPadding || InteriorMargin * 2 >= ArenaSideLength ||
                DeadEndEndRadius <= DeadEndWidth * .5f || DeadEndLengthMin <= DeadEndEndRadius ||
                RingCornerRadius <= 0 || MinimumJunctionAngleDegrees >= 180 || BranchTiltDegrees >= 90)
                throw new ArgumentException("Inconsistent road geometry profile.");
        }
    }
}
