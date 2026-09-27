namespace Game.Presentation.Json
{
    /// <summary>One piece of a layout pattern, relative to the pattern center (DECISION-0068); every field is required.</summary>
    public sealed class FieldObstaclePieceData
    {
        public FieldObstacleKind? Kind { get; set; }
        public float? X { get; set; }
        public float? Y { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }
    }
}
