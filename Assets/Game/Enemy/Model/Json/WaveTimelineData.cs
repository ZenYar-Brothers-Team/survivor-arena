namespace Game.Enemy.Json
{
    public sealed class WaveTimelineData
    {
        public string Id { get; set; }
        public int Seed { get; set; }
        public float SpawnRadius { get; set; }
        public WavePhaseData[] Phases { get; set; }
        public WaveHookData[] Hooks { get; set; }
    }
}
