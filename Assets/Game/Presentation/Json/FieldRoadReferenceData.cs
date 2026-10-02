namespace Game.Presentation.Json
{
    /// <summary>Frozen approved geometry, used only when the bounded generator exhausts its budget.</summary>
    public sealed class FieldRoadReferenceData
    {
        public int? Seed { get; set; }
        public float[][][] RoadCenterlines { get; set; }
        public FieldRoadDeadEndData[] DeadEnds { get; set; }
    }
}
