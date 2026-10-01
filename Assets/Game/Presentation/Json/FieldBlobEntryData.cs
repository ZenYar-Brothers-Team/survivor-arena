namespace Game.Presentation.Json
{
    public sealed class FieldBlobEntryData
    {
        public FieldBlobStyle? Style { get; set; }
        public float? Radius { get; set; }
        /// <summary>Library item to place instead of a procedural silhouette; excludes style and radius.</summary>
        public string LibraryId { get; set; }
    }
}
