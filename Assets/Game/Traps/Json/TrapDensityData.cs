namespace Game.Traps.Json
{
    /// <summary>Screen-based scatter: the arena is cut into cells of one screen and each cell gets a random number of traps.</summary>
    public sealed class TrapDensityData
    {
        public float? CellWidth { get; set; }
        public float? CellHeight { get; set; }
        public TrapCountWeightData[] CountWeights { get; set; }
    }
}
