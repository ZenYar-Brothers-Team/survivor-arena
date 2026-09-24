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
        /// <summary>Optional inline attacks referenced by phase attackEnemyIds (production bosses).</summary>
        public BossAttackData[] Attacks { get; set; }
        public bool? KeepAttackOrderOnPhaseChange { get; set; }
        public bool? StrictHealthThreshold { get; set; }
    }
}
