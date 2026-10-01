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
        /// <summary>Library items one of which partly covers the start screen; replaces radius and styles.</summary>
        public string[] LibraryIds { get; set; }
    }
}
