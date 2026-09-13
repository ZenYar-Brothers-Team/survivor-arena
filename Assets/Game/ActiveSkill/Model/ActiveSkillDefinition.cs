using System;
using Game.Content;

namespace Game.ActiveSkill
{
    public sealed class ActiveSkillDefinition : IContentDefinition
    {
        public ContentId Id { get; }
        public float BaseDamage { get; }
        public float CooldownSeconds { get; }
        public float ProjectileSpeed { get; }
        public float ProjectileLifetimeSeconds { get; }
        public float ProjectileCollisionRadius { get; }
        public float ImpactAreaRadius { get; }

        public ActiveSkillDefinition(
            ContentId id,
            float baseDamage,
            float cooldownSeconds,
            float projectileSpeed,
            float projectileLifetimeSeconds,
            float projectileCollisionRadius,
            float impactAreaRadius)
        {
            if (!id.IsValid)
                throw new ArgumentException("Active skill definition requires a valid content id.", nameof(id));

            ValidateNonNegative(baseDamage, nameof(baseDamage));
            ValidatePositive(cooldownSeconds, nameof(cooldownSeconds));
            ValidatePositive(projectileSpeed, nameof(projectileSpeed));
            ValidatePositive(projectileLifetimeSeconds, nameof(projectileLifetimeSeconds));
            ValidatePositive(projectileCollisionRadius, nameof(projectileCollisionRadius));
            ValidateNonNegative(impactAreaRadius, nameof(impactAreaRadius));

            Id = id;
            BaseDamage = baseDamage;
            CooldownSeconds = cooldownSeconds;
            ProjectileSpeed = projectileSpeed;
            ProjectileLifetimeSeconds = projectileLifetimeSeconds;
            ProjectileCollisionRadius = projectileCollisionRadius;
            ImpactAreaRadius = impactAreaRadius;
        }

        private static void ValidatePositive(float value, string parameterName)
        {
            ValidateFinite(value, parameterName);
            if (value <= 0f)
                throw new ArgumentOutOfRangeException(parameterName, "Value must be greater than zero.");
        }

        private static void ValidateNonNegative(float value, string parameterName)
        {
            ValidateFinite(value, parameterName);
            if (value < 0f)
                throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative.");
        }

        private static void ValidateFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
        }
    }
}
