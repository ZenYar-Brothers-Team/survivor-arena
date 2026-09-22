namespace Game.Presentation.Json
{
    public sealed class FieldEnvironmentPresentationData
    {
        public string Id { get; set; }
        public string EnvironmentId { get; set; }
        public string GroundVisualId { get; set; }
        public string FenceVisualId { get; set; }
        public string ObstacleVisualId { get; set; }
        public string BushVisualId { get; set; }
        public string GrassVisualId { get; set; }
        public string ObstacleName { get; set; }
        public float? FenceHeight { get; set; }
        public float? ObstacleScale { get; set; }
        public float? DecorationSpacing { get; set; }
        public float? DecorationJitter { get; set; }
        public float? DecorationChance { get; set; }
        public float? BushChance { get; set; }
        public float? DecorationMargin { get; set; }
        public float? SafeRadius { get; set; }
        public float? GrassScaleMin { get; set; }
        public float? GrassScaleMax { get; set; }
        public float? BushScaleMin { get; set; }
        public float? BushScaleMax { get; set; }
        public int? Seed { get; set; }
    }
}
