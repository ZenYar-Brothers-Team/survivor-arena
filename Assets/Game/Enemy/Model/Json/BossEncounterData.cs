namespace Game.Enemy.Json
{
    public sealed class BossEncounterData
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Hook { get; set; }
        public EnemyDefinitionData Body { get; set; }
        public float? SpawnOffsetX { get; set; }
        public float? SpawnOffsetY { get; set; }
        public BossPhaseData[] Phases { get; set; }
    }
}
