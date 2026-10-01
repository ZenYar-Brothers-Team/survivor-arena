using System.Collections.Generic;

namespace Game.Presentation.Json
{
    public sealed class FieldBlobLayoutData
    {
        public float? EdgeMargin { get; set; }
        public float? StartClearRadius { get; set; }
        public float? PlayerClearRadius { get; set; }
        public float? MinGap { get; set; }
        public int? PlacementAttempts { get; set; }
        public int? MaxRestarts { get; set; }
        public int? ReferenceSeed { get; set; }
        public float? PixelsPerUnit { get; set; }
        public int? OutlinePixels { get; set; }
        public FieldBlobStartData StartScreen { get; set; }
        public Dictionary<string, FieldBlobStyleData> Styles { get; set; }
        /// <summary>Authored illustrated obstacles (fixed size and orientation) that blob entries may reference.</summary>
        public FieldBlobLibraryItemData[] Library { get; set; }
        public FieldBlobEntryData[] Blobs { get; set; }
    }
}
