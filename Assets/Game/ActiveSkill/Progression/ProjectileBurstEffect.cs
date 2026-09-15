using System;
using Game.Content;

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
            NumericValidation.ValidateCount(projectileCount, nameof(projectileCount));
            if (!Enum.IsDefined(typeof(ProjectileLayout), layout))
                throw new ArgumentOutOfRangeException(nameof(layout));
            NumericValidation.ValidateNonNegativeFinite(spreadDegrees, nameof(spreadDegrees));
            if (pierceCount < 0)
                throw new ArgumentOutOfRangeException(nameof(pierceCount));
            NumericValidation.ValidatePositive(speed, nameof(speed));
            NumericValidation.ValidatePositive(lifetimeSeconds, nameof(lifetimeSeconds));
            NumericValidation.ValidatePositive(collisionRadius, nameof(collisionRadius));
            NumericValidation.ValidateNonNegativeFinite(impactAreaRadius, nameof(impactAreaRadius));
            NumericValidation.ValidateNonNegativeFinite(damageMultiplier, nameof(damageMultiplier));

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
    }
}
