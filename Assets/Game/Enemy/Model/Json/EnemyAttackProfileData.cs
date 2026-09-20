namespace Game.Enemy.Json
{
    // No tuning defaults live here: projectile count and radius are always required, and
    // burst interval is required for the Burst pattern (FixtureEnemyCatalog rejects a
    // missing one by name). Pattern-specific extras that stay at zero (spread, rotation
    // step, explosion radius) are only meaningful when the pattern uses them.
    public sealed class EnemyAttackProfileData
    {
        public string Pattern { get; set; }
        public float Damage { get; set; }
        public float CooldownSeconds { get; set; }
        public float ProjectileSpeed { get; set; }
        public float ProjectileLifetimeSeconds { get; set; }
        public int? ProjectileCount { get; set; }
        public float SpreadDegrees { get; set; }
        public float? BurstIntervalSeconds { get; set; }
        public float? ProjectileRadius { get; set; }
        public float ExplosionRadius { get; set; }
        public float RotationStepDegrees { get; set; }
    }
}
