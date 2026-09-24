using System;
using Game.Content;
using Game.Combat;
using Game.Presentation;

namespace Game.Enemy
{
    public sealed class EnemyAttackProfile
    {
        public EnemyProjectilePattern Pattern { get; }
        public float TelegraphSeconds { get; }
        public float Damage { get; }
        public CombatControlProfile Controls { get; }
        public float CooldownSeconds { get; }
        public float ProjectileSpeed { get; }
        public float ProjectileLifetimeSeconds { get; }
        public int ProjectileCount { get; }
        public float SpreadDegrees { get; }
        public float BurstIntervalSeconds { get; }
        public float ProjectileRadius { get; }
        public float ExplosionRadius { get; }
        public float RotationStepDegrees { get; }
        public ContentRef<SpriteDefinition> ProjectileVisual { get; }
        public EnemyAttackCadence Cadence { get; }
        /// <summary>Pattern starts at 0° (world +X) instead of the aim direction, e.g. BOSS-001 ring.</summary>
        public bool FixedOrientation { get; }

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
            float rotationStepDegrees = 0f,
            CombatControlProfile controls = null,
            float telegraphSeconds = 0f,
            ContentRef<SpriteDefinition> projectileVisual = default,
            EnemyAttackCadence cadence = EnemyAttackCadence.CooldownAfterShot,
            bool fixedOrientation = false)
        {
            if (!Enum.IsDefined(typeof(EnemyAttackCadence), cadence))
                throw new ArgumentOutOfRangeException(nameof(cadence));
            if (!Enum.IsDefined(typeof(EnemyProjectilePattern), pattern))
                throw new ArgumentOutOfRangeException(nameof(pattern));
            NumericValidation.ValidateNonNegative(damage, nameof(damage));
            NumericValidation.ValidatePositive(cooldownSeconds, nameof(cooldownSeconds));
            NumericValidation.ValidatePositive(projectileSpeed, nameof(projectileSpeed));
            NumericValidation.ValidatePositive(projectileLifetimeSeconds, nameof(projectileLifetimeSeconds));
            NumericValidation.ValidateCount(projectileCount, nameof(projectileCount));
            NumericValidation.ValidateRange(spreadDegrees, 0f, 360f, nameof(spreadDegrees));
            NumericValidation.ValidatePositive(burstIntervalSeconds, nameof(burstIntervalSeconds));
            NumericValidation.ValidatePositive(projectileRadius, nameof(projectileRadius));
            NumericValidation.ValidateNonNegative(explosionRadius, nameof(explosionRadius));
            NumericValidation.ValidateFinite(rotationStepDegrees, nameof(rotationStepDegrees));
            if (pattern == EnemyProjectilePattern.Explosive && explosionRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(explosionRadius), "Explosive projectiles require a positive explosion radius.");

            NumericValidation.ValidateNonNegative(telegraphSeconds, nameof(telegraphSeconds));
            TelegraphSeconds = telegraphSeconds;
            if ((pattern == EnemyProjectilePattern.Single || pattern == EnemyProjectilePattern.Explosive) && projectileCount != 1)
                throw new ArgumentException("Single/explosive patterns require one projectile.", nameof(projectileCount));
            if (pattern == EnemyProjectilePattern.Cross && projectileCount != 4)
                throw new ArgumentException("Cross requires four projectiles.", nameof(projectileCount));
            Controls = controls ?? CombatControlProfile.None;
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
            ProjectileVisual = projectileVisual;
            Cadence = cadence;
            FixedOrientation = fixedOrientation;
        }
    }
}
