using System;
using System.Collections.Generic;
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
        /// <summary>Extra volleys of the same pattern after each main shot, by ascending delay (DECISION-0066, E1).</summary>
        public IReadOnlyList<EnemyAttackFollowUp> FollowUps { get; }
        /// <summary>Movement speed factor while this attack winds up; 1 = unchanged (MIDBOSS-009, DECISION-0066, E6).</summary>
        public float WindupMovementMultiplier { get; }

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
            bool fixedOrientation = false,
            IEnumerable<EnemyAttackFollowUp> followUps = null,
            float windupMovementMultiplier = 1f)
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
            if (pattern == EnemyProjectilePattern.Single && projectileCount != 1)
                throw new ArgumentException("Single pattern requires one projectile.", nameof(projectileCount));
            // Several explosives leave as a fan (BOSS-007, DECISION-0066, E2).
            if (pattern == EnemyProjectilePattern.Explosive && projectileCount > 1 && spreadDegrees <= 0f)
                throw new ArgumentException("Several explosives need a fan spread.", nameof(spreadDegrees));
            var copy = new List<EnemyAttackFollowUp>(followUps ?? Array.Empty<EnemyAttackFollowUp>());
            for (var i = 1; i < copy.Count; i++)
                if (copy[i].DelaySeconds <= copy[i - 1].DelaySeconds)
                    throw new ArgumentException("Follow-up delays must strictly increase.", nameof(followUps));
            if (copy.Count > 0 && pattern == EnemyProjectilePattern.Burst)
                throw new ArgumentException("A burst already repeats; follow-ups are not supported for it.", nameof(followUps));
            NumericValidation.ValidatePositive(windupMovementMultiplier, nameof(windupMovementMultiplier));
            NumericValidation.ValidateRange(windupMovementMultiplier, 0f, 1f, nameof(windupMovementMultiplier));
            FollowUps = copy.AsReadOnly();
            WindupMovementMultiplier = windupMovementMultiplier;
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
