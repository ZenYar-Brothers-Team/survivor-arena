using Game.Content;

namespace Game.ActiveSkill
{
    public sealed class BoomerangEffect : IActiveSkillEffect
    {
        public float HitCooldownSeconds { get; }
        public float ReturnKnockbackMultiplier { get; }
        public float LifetimeSeconds { get; }
        public int ProjectileCount { get; }
        public float SpreadDegrees { get; }
        public float Speed { get; }
        public float Range { get; }
        public float CollisionRadius { get; }
        public float ReturnDamageMultiplier { get; }
        public float DamageMultiplier { get; }

        public BoomerangEffect(int projectileCount, float spreadDegrees, float speed, float range, float collisionRadius, float returnDamageMultiplier, float damageMultiplier = 1f, float hitCooldownSeconds = 0f, float returnKnockbackMultiplier = 1f, float lifetimeSeconds = 0f)
        {
            NumericValidation.ValidateCount(projectileCount, nameof(projectileCount));
            NumericValidation.ValidateNonNegativeFinite(spreadDegrees, nameof(spreadDegrees));
            NumericValidation.ValidatePositive(speed, nameof(speed));
            NumericValidation.ValidatePositive(range, nameof(range));
            NumericValidation.ValidatePositive(collisionRadius, nameof(collisionRadius));
            NumericValidation.ValidateNonNegativeFinite(returnDamageMultiplier, nameof(returnDamageMultiplier));
            NumericValidation.ValidateNonNegativeFinite(damageMultiplier, nameof(damageMultiplier));
            NumericValidation.ValidateNonNegative(hitCooldownSeconds, nameof(hitCooldownSeconds));
            NumericValidation.ValidateNonNegative(returnKnockbackMultiplier, nameof(returnKnockbackMultiplier));
            NumericValidation.ValidateNonNegative(lifetimeSeconds, nameof(lifetimeSeconds));
            HitCooldownSeconds = hitCooldownSeconds;
            ReturnKnockbackMultiplier = returnKnockbackMultiplier;
            LifetimeSeconds = lifetimeSeconds > 0f ? lifetimeSeconds : 2f * range / speed;
            ProjectileCount = projectileCount;
            SpreadDegrees = spreadDegrees;
            Speed = speed;
            Range = range;
            CollisionRadius = collisionRadius;
            ReturnDamageMultiplier = returnDamageMultiplier;
            DamageMultiplier = damageMultiplier;
        }
    }
}
