namespace Game.Enemy.Json
{
    public sealed class WaveBurstData
    {
        public int? Count { get; set; }
        public float? OffsetSeconds { get; set; }
        public float? WindowSeconds { get; set; }
        /// <summary>Optional arc formation; omitted = 0 = scattered burst.</summary>
        public float ArcDegrees { get; set; }
    }
}
