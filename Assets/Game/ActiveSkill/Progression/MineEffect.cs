using Game.Content;

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
            NumericValidation.ValidatePositive(triggerRadius, nameof(triggerRadius));
            NumericValidation.ValidatePositive(blastRadius, nameof(blastRadius));
            NumericValidation.ValidatePositive(lifetimeSeconds, nameof(lifetimeSeconds));
            NumericValidation.ValidateCount(maxConcurrent, nameof(maxConcurrent));
            NumericValidation.ValidateNonNegativeFinite(secondaryDelaySeconds, nameof(secondaryDelaySeconds));
            NumericValidation.ValidateNonNegativeFinite(secondaryDamageMultiplier, nameof(secondaryDamageMultiplier));
            NumericValidation.ValidateNonNegativeFinite(damageMultiplier, nameof(damageMultiplier));
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
