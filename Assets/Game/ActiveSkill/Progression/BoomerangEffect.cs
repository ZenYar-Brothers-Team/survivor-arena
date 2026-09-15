namespace Game.ActiveSkill
{
    public sealed class BoomerangEffect : IActiveSkillEffect
    {
        public int ProjectileCount { get; }
        public float SpreadDegrees { get; }
        public float Speed { get; }
        public float Range { get; }
        public float CollisionRadius { get; }
        public float ReturnDamageMultiplier { get; }
        public float DamageMultiplier { get; }

        public BoomerangEffect(int projectileCount, float spreadDegrees, float speed, float range, float collisionRadius, float returnDamageMultiplier, float damageMultiplier = 1f)
        {
            ProjectileBurstEffect.ValidateCount(projectileCount, nameof(projectileCount));
            ProjectileBurstEffect.ValidateNonNegative(spreadDegrees, nameof(spreadDegrees));
            ProjectileBurstEffect.ValidatePositive(speed, nameof(speed));
            ProjectileBurstEffect.ValidatePositive(range, nameof(range));
            ProjectileBurstEffect.ValidatePositive(collisionRadius, nameof(collisionRadius));
            ProjectileBurstEffect.ValidateNonNegative(returnDamageMultiplier, nameof(returnDamageMultiplier));
            ProjectileBurstEffect.ValidateNonNegative(damageMultiplier, nameof(damageMultiplier));
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
