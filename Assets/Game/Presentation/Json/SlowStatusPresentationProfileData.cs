namespace Game.Presentation.Json
{
    public sealed class SlowStatusPresentationProfileData
    {
        public float[] TintColor { get; set; }
        public float[] IceColor { get; set; }
        public float[] OutlineColor { get; set; }
        public float? OutlineWidth { get; set; }
        public float? BarWidth { get; set; }
        public float? BarHeight { get; set; }
        public float? BarOffsetY { get; set; }
        public float[] BarFillColor { get; set; }
        public float[] BarBackColor { get; set; }
        public float? PreviewSlowFraction { get; set; }
        public float? PreviewSlowSeconds { get; set; }
    }
}
