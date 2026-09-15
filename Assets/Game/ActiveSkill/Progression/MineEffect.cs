namespace Game.ActiveSkill
{
    public sealed class MineEffect : IActiveSkillEffect
    {
        public float TriggerRadius { get; }
        public float BlastRadius { get; }
        public float LifetimeSeconds { get; }
        public int MaxConcurrent { get; }
        public float SecondaryDelaySeconds { get; }
        public float SecondaryDamageMultiplier { get; }
        public float DamageMultiplier { get; }

        public MineEffect(
            float triggerRadius,
            float blastRadius,
            float lifetimeSeconds,
            int maxConcurrent,
            float secondaryDelaySeconds = 0f,
            float secondaryDamageMultiplier = 0f,
            float damageMultiplier = 1f)
        {
            ProjectileBurstEffect.ValidatePositive(triggerRadius, nameof(triggerRadius));
            ProjectileBurstEffect.ValidatePositive(blastRadius, nameof(blastRadius));
            ProjectileBurstEffect.ValidatePositive(lifetimeSeconds, nameof(lifetimeSeconds));
            ProjectileBurstEffect.ValidateCount(maxConcurrent, nameof(maxConcurrent));
            ProjectileBurstEffect.ValidateNonNegative(secondaryDelaySeconds, nameof(secondaryDelaySeconds));
            ProjectileBurstEffect.ValidateNonNegative(secondaryDamageMultiplier, nameof(secondaryDamageMultiplier));
            ProjectileBurstEffect.ValidateNonNegative(damageMultiplier, nameof(damageMultiplier));
            TriggerRadius = triggerRadius;
            BlastRadius = blastRadius;
            LifetimeSeconds = lifetimeSeconds;
            MaxConcurrent = maxConcurrent;
            SecondaryDelaySeconds = secondaryDelaySeconds;
            SecondaryDamageMultiplier = secondaryDamageMultiplier;
            DamageMultiplier = damageMultiplier;
        }
    }
}
