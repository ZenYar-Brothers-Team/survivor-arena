namespace Game.Enemy.Json
{
    public sealed class WavePhaseData
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public WavePhaseTag Tag { get; set; }
        public float DurationSeconds { get; set; }
        public float SpawnIntervalSeconds { get; set; }
        public int MaxAliveEnemies { get; set; }
        public WaveCompositionEntryData[] Composition { get; set; }
        public WaveEnemyModifiersData Modifiers { get; set; }
    }
}
