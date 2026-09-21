namespace Game.Presentation.Json
{
    public sealed class PresentationFixtureData
    {
        public string Id { get; set; }
        public SpriteRole? Role { get; set; }
        public float? X { get; set; }
        public float? Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float[] Color { get; set; }
        public bool? BodyMotion { get; set; }
    }
}
