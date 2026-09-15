using System;

namespace Game.ActiveSkill
{
    public sealed class ProjectileBurstEffect : IActiveSkillEffect
    {
        public int ProjectileCount { get; }
        public ProjectileLayout Layout { get; }
        public float SpreadDegrees { get; }
        public int PierceCount { get; }
        public float Speed { get; }
        public float LifetimeSeconds { get; }
        public float CollisionRadius { get; }
        public float ImpactAreaRadius { get; }
        public float DamageMultiplier { get; }

        public ProjectileBurstEffect(
            int projectileCount,
            ProjectileLayout layout,
            float spreadDegrees,
            int pierceCount,
            float speed,
            float lifetimeSeconds,
            float collisionRadius,
            float impactAreaRadius = 0f,
            float damageMultiplier = 1f)
        {
            ValidateCount(projectileCount, nameof(projectileCount));
            if (!Enum.IsDefined(typeof(ProjectileLayout), layout))
                throw new ArgumentOutOfRangeException(nameof(layout));
            ValidateNonNegative(spreadDegrees, nameof(spreadDegrees));
            if (pierceCount < 0)
                throw new ArgumentOutOfRangeException(nameof(pierceCount));
            ValidatePositive(speed, nameof(speed));
            ValidatePositive(lifetimeSeconds, nameof(lifetimeSeconds));
            ValidatePositive(collisionRadius, nameof(collisionRadius));
            ValidateNonNegative(impactAreaRadius, nameof(impactAreaRadius));
            ValidateNonNegative(damageMultiplier, nameof(damageMultiplier));

            ProjectileCount = projectileCount;
            Layout = layout;
            SpreadDegrees = spreadDegrees;
            PierceCount = pierceCount;
            Speed = speed;
            LifetimeSeconds = lifetimeSeconds;
            CollisionRadius = collisionRadius;
            ImpactAreaRadius = impactAreaRadius;
            DamageMultiplier = damageMultiplier;
        }

        internal static void ValidateCount(int value, string name)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(name, "Count must be greater than zero.");
        }

        internal static void ValidatePositive(float value, string name)
        {
            ValidateNonNegative(value, name);
            if (value <= 0f)
                throw new ArgumentOutOfRangeException(name, "Value must be greater than zero.");
        }

        internal static void ValidateNonNegative(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(name, "Value must be finite and non-negative.");
        }
    }
}
