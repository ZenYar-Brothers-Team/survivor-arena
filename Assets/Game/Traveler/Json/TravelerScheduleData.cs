namespace Game.Traveler.Json
{
    public sealed class TravelerScheduleData
    {
        public string Id;
        public string[] TravelerIds;
        public float[] CountProbabilities;
        public int? Seed, FieldRank, PlacementAttempts;
        public float? EndBufferSeconds, SpawnScreenHeights, FieldGrowth, TimeGrowth;
    }
}
