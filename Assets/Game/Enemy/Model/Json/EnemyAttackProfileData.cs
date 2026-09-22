using Game.Combat;

namespace Game.Enemy.Json
{
    // Fields consumed by each pattern are required by FixtureEnemyCatalog.
    public sealed class EnemyAttackProfileData
    {
        public float? TelegraphSeconds { get; set; }
        public CombatControlData Controls { get; set; }
        public string Pattern { get; set; }
        public float Damage { get; set; }
        public float CooldownSeconds { get; set; }
        public float ProjectileSpeed { get; set; }
        public float ProjectileLifetimeSeconds { get; set; }
        public int? ProjectileCount { get; set; }
        public float? SpreadDegrees { get; set; }
        public float? BurstIntervalSeconds { get; set; }
        public float? ProjectileRadius { get; set; }
        public float? ExplosionRadius { get; set; }
        public float? RotationStepDegrees { get; set; }
        public string ProjectileVisualId { get; set; }
    }
}
