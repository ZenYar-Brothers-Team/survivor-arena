namespace Game.Presentation.Json
{
    /// <summary>Optional visible, clear-start pair for a field's reference landscape camera.</summary>
    public sealed class FieldStartScreenData
    {
        public float? HalfWidth { get; set; }
        public float? HalfHeight { get; set; }
        public float? MinAbsX { get; set; }
        public float? MaxAbsX { get; set; }
        public float? MinAbsY { get; set; }
        public float? MaxAbsY { get; set; }
        public string[] PatternIds { get; set; }
    }
}
