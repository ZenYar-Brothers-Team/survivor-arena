namespace Game.Enemy.Json
{
    public sealed class BossPhaseData
    {
        public string Id { get; set; }
        public float? HealthThreshold { get; set; }
        public string[] AttackEnemyIds { get; set; }
    }
}
