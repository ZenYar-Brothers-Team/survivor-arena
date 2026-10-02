namespace Game.Presentation.Json
{
    public sealed class SlowStatusPresentationProfileData
    {
        public float[] TintColor { get; set; }
        public float[] IceColor { get; set; }
        public string IceVisualId { get; set; }
        public float[] OutlineColor { get; set; }
        public float? OutlineWidth { get; set; }
        public float? BarWidth { get; set; }
        public float? BarHeight { get; set; }
        public float? BarOffsetY { get; set; }
        public float[] BarFillColor { get; set; }
        public float[] BarBackColor { get; set; }
        public float[] SpeedBarFillColor { get; set; }
        public float[] SpeedBoltColor { get; set; }
        public float? SpeedBoltScale { get; set; }
        public float? SpeedBlinkPeriod { get; set; }
        public float? PreviewSlowFraction { get; set; }
        public float? PreviewSlowSeconds { get; set; }
        public float[] ShieldBarFillColor { get; set; }
        public float[] ExperienceBarFillColor { get; set; }
        public float[] PowerBarFillColor { get; set; }
    }
}
