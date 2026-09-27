namespace Game.Enemy.Json
{
    /// <summary>
    /// Boss-owned sequence step; the carrier reuses the boss body values and never spawns on its own. Exactly one of
    /// attack (projectiles) or zone/beam/summon (DECISION-0066, with the step's cooldownSeconds).
    /// </summary>
    public sealed class BossAttackData
    {
        public string Id { get; set; }
        public EnemyAttackProfileData Attack { get; set; }
        public float? CooldownSeconds { get; set; }
        public BossZoneData Zone { get; set; }
        public BossBeamData Beam { get; set; }
        public BossSummonData Summon { get; set; }
    }
}
