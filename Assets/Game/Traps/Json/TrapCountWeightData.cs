namespace Game.Traps.Json
{
    /// <summary>How likely a screen-sized cell gets exactly this many traps (relative weight).</summary>
    public sealed class TrapCountWeightData
    {
        public int? Count { get; set; }
        public float? Weight { get; set; }
    }
}
