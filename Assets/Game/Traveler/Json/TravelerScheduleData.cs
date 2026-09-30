namespace Game.Traveler.Json
{
    public sealed class TravelerScheduleData
    {
        public string Id;
        public string[] TravelerIds;
        public float[] CountProbabilities;
        /// <summary>Fewest Travelers of a run; <see cref="CountProbabilities"/>[i] is the chance of MinCount + i (DECISION-0122).</summary>
        public int? MinCount;
        public int? Seed, FieldRank, PlacementAttempts;
        public float? EndBufferSeconds, SpawnScreenHeights, FieldGrowth, TimeGrowth, InitialHealthMultiplier;
    }
}
