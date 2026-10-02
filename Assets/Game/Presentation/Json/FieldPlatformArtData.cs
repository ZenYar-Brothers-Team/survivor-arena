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
    }
}
