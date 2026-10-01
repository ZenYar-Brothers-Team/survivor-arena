namespace Game.Presentation.Json
{
    /// <summary>One authored illustrated obstacle: its sprite and the collision outline around the sprite pivot (world units, y up).</summary>
    public sealed class FieldBlobLibraryItemData
    {
        public string Id { get; set; }
        public string VisualId { get; set; }
        public float[][] Points { get; set; }
    }
}
