namespace Game.Presentation.Json
{
    public sealed class FieldObstacleData
    {
        public string Id { get; set; }
        public FieldObstacleKind? Kind { get; set; }
        public float? X { get; set; }
        public float? Y { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }
    }
}
