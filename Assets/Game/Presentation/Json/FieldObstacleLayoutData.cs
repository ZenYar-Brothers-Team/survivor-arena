namespace Game.Presentation.Json
{
    /// <summary>Per-run obstacle layout of a field (DECISION-0068); every field is required when the object is present.</summary>
    public sealed class FieldObstacleLayoutData
    {
        public float? CellSize { get; set; }
        public int? PatternsPerCell { get; set; }
        public float? EdgeMargin { get; set; }
        public float? CellMargin { get; set; }
        public float? StartClearRadius { get; set; }
        public float? MinPatternGap { get; set; }
        public int? PlacementAttempts { get; set; }
        public int? ReferenceSeed { get; set; }
        public FieldStartScreenData StartScreen { get; set; }
        public FieldObstaclePatternData[] Patterns { get; set; }
    }
}
