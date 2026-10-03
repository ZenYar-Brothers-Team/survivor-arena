namespace Game.Presentation.Json
{
    /// <summary>Approved sprite reference and normalized runtime PNG crop; bottom-left UV coordinates.</summary>
    public sealed class ScreenEventArtworkData
    {
        public string VisualId { get; set; }
        public float[] UvRect { get; set; }
    }
}
