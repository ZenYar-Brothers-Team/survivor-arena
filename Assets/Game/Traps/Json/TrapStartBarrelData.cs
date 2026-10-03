namespace Game.Traps.Json
{
    /// <summary>A barrel placed at a fixed offset from the player's start; it counts towards barrels.count.</summary>
    public sealed class TrapStartBarrelData
    {
        public float? OffsetX { get; set; }
        public float? OffsetY { get; set; }
        public bool? Explosive { get; set; }
    }
}
