namespace Game.Traps
{
    /// <summary>A trap count a screen cell can get and its relative weight.</summary>
    public readonly struct TrapCountWeight
    {
        public int Count { get; }
        public float Weight { get; }

        public TrapCountWeight(int count, float weight)
        {
            Count = count;
            Weight = weight;
        }
    }
}
