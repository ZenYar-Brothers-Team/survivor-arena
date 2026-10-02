namespace Game.Presentation.Json
{
    /// <summary>Visual-only FIELD-009 material sampling; world-unit repeats and normalized source rectangles.</summary>
    public sealed class FieldPlatformArtData
    {
        public string VisualId { get; set; }
        public float[] GroundUvBounds { get; set; }
        public float[] SurfaceUvBounds { get; set; }
        public float GroundRepeat { get; set; }
        public float SurfaceRepeat { get; set; }
        public float ContourStep { get; set; }
        public float RimWidth { get; set; }
        public float InlayWidth { get; set; }
        public float FaceHeight { get; set; }
        public string RimColor { get; set; }
        public string InlayColor { get; set; }
        public string FaceColor { get; set; }
        // Optional presentation mode; false keeps the existing stone bridge contract.
        public bool BridgeVeil { get; set; }
        public string BridgeVeilColor { get; set; }
        public string BridgeThreadColor { get; set; }
        public string BridgeEdgeColor { get; set; }
        public float? BridgeVeilOpacity { get; set; }
        public float? BridgeThreadOpacity { get; set; }
        public float? BridgeEdgeOpacity { get; set; }
        public float? BridgeWeaveLength { get; set; }
        public float? BridgeThreadWidth { get; set; }
        public float? BridgeEdgeWidth { get; set; }
    }
}
