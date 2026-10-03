using System.Collections.Generic;

namespace Game.Presentation.Json
{
    /// <summary>FIELD-010 visual-only settings; all distances are world units and alphas are in [0,1].</summary>
    public sealed class ScreenEventPresentationData
    {
        public Dictionary<string, ScreenEventArtworkData> Artwork { get; set; }
        public Dictionary<string, string> SweepArtwork { get; set; }
        public float? BorderWidth { get; set; }
        public float? RepeatLength { get; set; }
        public float? ExteriorRibbonWidth { get; set; }
        public float? StrikeRevealSeconds { get; set; }
        public float? CircleRotationDegreesPerSecond { get; set; }
        public float? StripScrollSpeed { get; set; }
        public float? RingScrollCyclesPerSecond { get; set; }
        public float? ExteriorScrollSpeed { get; set; }
        public float? StrikePulseHz { get; set; }
        public float? StrikePulseDepth { get; set; }
        public float? StrikeBoundaryAlpha { get; set; }
        public float? RevealFeatherFraction { get; set; }
        public float? WarningAlphaStart { get; set; }
        public float? WarningAlphaEnd { get; set; }
        public float? WarningFillStart { get; set; }
        public float? WarningFillEnd { get; set; }
        public float? StrikeAlpha { get; set; }
        public float? StrikeFillAlpha { get; set; }
        public float? SafeAlpha { get; set; }
        public float? CorridorAlpha { get; set; }
        public string WarningColor { get; set; }
        public string StrikeColor { get; set; }
        public string SafeColor { get; set; }
        public int? SortingOrder { get; set; }
    }
}
