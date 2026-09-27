namespace Game.Enemy.Json
{
    public sealed class BossPhaseData
    {
        public string Id { get; set; }
        public float? HealthThreshold { get; set; }
        public string[] AttackEnemyIds { get; set; }
        /// <summary>Optional dash movement used while this phase is active (DECISION-0066, E3).</summary>
        public EnemyMovementProfileData Movement { get; set; }
    }
}
