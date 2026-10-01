namespace Game.Presentation.Json
{
    public sealed class FieldBlobStartData
    {
        public float? HalfWidth { get; set; }
        public float? HalfHeight { get; set; }
        /// <summary>How far past the start screen the blob center may be drawn.</summary>
        public float? Reach { get; set; }
        public float? Radius { get; set; }
        public FieldBlobStyle[] Styles { get; set; }
    }
}
