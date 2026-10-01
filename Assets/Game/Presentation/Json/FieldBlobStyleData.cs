namespace Game.Presentation.Json
{
    /// <summary>Generation and paint parameters of one blob style; each style requires its own subset.</summary>
    public sealed class FieldBlobStyleData
    {
        public string FillColor { get; set; }
        public string OutlineColor { get; set; }
        public float? StretchMin { get; set; }
        public float? StretchMax { get; set; }
        // Round: radial harmonics 2, 3 and 5 around the base radius.
        public int? OutlineVertexCount { get; set; }
        public float[] HarmonicAmplitudes { get; set; }
        public float? HarmonicScaleMin { get; set; }
        public float? HarmonicScaleMax { get; set; }
        // Angular: convex hull of random points.
        public int? MinVertices { get; set; }
        public int? MaxVertices { get; set; }
        public float? RadiusJitterMin { get; set; }
        public float? RadiusJitterMax { get; set; }
        public float? MinAspect { get; set; }
        // Linear: bent tapered band.
        public float? LengthMin { get; set; }
        public float? LengthMax { get; set; }
        public float? WidthMin { get; set; }
        public float? WidthMax { get; set; }
        public float? BendMax { get; set; }
        public float? Taper { get; set; }
        public int? SpineSamples { get; set; }
    }
}
