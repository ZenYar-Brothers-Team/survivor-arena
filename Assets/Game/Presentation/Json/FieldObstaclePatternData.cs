namespace Game.Presentation.Json
{
    /// <summary>A layout pattern (DECISION-0068); every field is required.</summary>
    public sealed class FieldObstaclePatternData
    {
        public string Id { get; set; }
        public float? Weight { get; set; }
        public int[] Rotations { get; set; }
        public FieldObstaclePieceData[] Pieces { get; set; }
    }
}
