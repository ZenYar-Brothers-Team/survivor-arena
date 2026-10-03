namespace Game.Traps.Json
{
    /// <summary>A turret placed at a fixed offset from the player's start so it is on the first screen.</summary>
    public sealed class TrapStartData
    {
        public string Type { get; set; }
        public float? OffsetX { get; set; }
        public float? OffsetY { get; set; }
        /// <summary>Optional model key: this placement is drawn with that 3D model instead of the placeholder shape.</summary>
        public string Model { get; set; }
    }
}
