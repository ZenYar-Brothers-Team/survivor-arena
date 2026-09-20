using System;
using Game.Content;

namespace Game.Enemy
{
    public sealed class EnemyAttackProfile
    {
        public EnemyProjectilePattern Pattern { get; }
        public float Damage { get; }
        public float CooldownSeconds { get; }
        public float ProjectileSpeed { get; }
        public float ProjectileLifetimeSeconds { get; }
        public int ProjectileCount { get; }
        public float SpreadDegrees { get; }
        public float BurstIntervalSeconds { get; }
        public float ProjectileRadius { get; }
        public float ExplosionRadius { get; }
        public float RotationStepDegrees { get; }

        public EnemyAttackProfile(
            EnemyProjectilePattern pattern,
            float damage,
            float cooldownSeconds,
            float projectileSpeed,
            float projectileLifetimeSeconds,
            int projectileCount = 1,
            float spreadDegrees = 0f,
            float burstIntervalSeconds = 0.15f,
            float projectileRadius = 0.12f,
            float explosionRadius = 0f,
            float rotationStepDegrees = 0f)
        {
            if (!Enum.IsDefined(typeof(EnemyProjectilePattern), pattern))
                throw new ArgumentOutOfRangeException(nameof(pattern));
            NumericValidation.ValidateNonNegative(damage, nameof(damage));
            NumericValidation.ValidatePositive(cooldownSeconds, nameof(cooldownSeconds));
            NumericValidation.ValidatePositive(projectileSpeed, nameof(projectileSpeed));
            NumericValidation.ValidatePositive(projectileLifetimeSeconds, nameof(projectileLifetimeSeconds));
            NumericValidation.ValidateCount(projectileCount, nameof(projectileCount));
            NumericValidation.ValidateNonNegative(spreadDegrees, nameof(spreadDegrees));
            NumericValidation.ValidatePositive(burstIntervalSeconds, nameof(burstIntervalSeconds));
            NumericValidation.ValidatePositive(projectileRadius, nameof(projectileRadius));
            NumericValidation.ValidateNonNegative(explosionRadius, nameof(explosionRadius));
            NumericValidation.ValidateFinite(rotationStepDegrees, nameof(rotationStepDegrees));
            if (pattern == EnemyProjectilePattern.Explosive && explosionRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(explosionRadius), "Explosive projectiles require a positive explosion radius.");

            Pattern = pattern;
            Damage = damage;
            CooldownSeconds = cooldownSeconds;
            ProjectileSpeed = projectileSpeed;
            ProjectileLifetimeSeconds = projectileLifetimeSeconds;
            ProjectileCount = projectileCount;
            SpreadDegrees = spreadDegrees;
            BurstIntervalSeconds = burstIntervalSeconds;
            ProjectileRadius = projectileRadius;
            ExplosionRadius = explosionRadius;
            RotationStepDegrees = rotationStepDegrees;
        }
    }
}
