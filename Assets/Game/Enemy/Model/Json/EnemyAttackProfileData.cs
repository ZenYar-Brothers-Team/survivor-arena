namespace Game.Enemy.Json
{
    public sealed class EnemyAttackProfileData
    {
        public string Pattern { get; set; }
        public float Damage { get; set; }
        public float CooldownSeconds { get; set; }
        public float ProjectileSpeed { get; set; }
        public float ProjectileLifetimeSeconds { get; set; }
        public int ProjectileCount { get; set; } = 1;
        public float SpreadDegrees { get; set; }
        public float BurstIntervalSeconds { get; set; } = 0.15f;
        public float ProjectileRadius { get; set; } = 0.12f;
        public float ExplosionRadius { get; set; }
        public float RotationStepDegrees { get; set; }
    }
}
