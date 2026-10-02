namespace Game.Presentation.Json
{
    /// <summary>Visual-only road covers and curb; distances are world units, UV bounds normalized.</summary>
    public sealed class FieldRoadArtData
    {
        public string MainVisualId { get; set; }
        public string BranchVisualId { get; set; }
        public string CurbVisualId { get; set; }
        public float? SurfaceRepeat { get; set; }
        public float? CurbWidth { get; set; }
        public float? CurbRepeat { get; set; }
        public float[] CurbUvBounds { get; set; }
        public float? CurbFaceHeight { get; set; }
        public string CurbFaceTint { get; set; }
        public float? EdgeWidth { get; set; }
        public float? EdgeOpacity { get; set; }
        public float? EdgeNoiseScale { get; set; }
        public float? EdgeNoiseStrength { get; set; }
        public string EdgeSoilColor { get; set; }
    }
}
