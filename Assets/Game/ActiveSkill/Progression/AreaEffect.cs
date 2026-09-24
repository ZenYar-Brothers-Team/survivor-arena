using Game.Content;

namespace Game.ActiveSkill
{
    public sealed class AreaEffect : IActiveSkillEffect
    {
        public float Radius { get; }
        public float DamageMultiplier { get; }
        /// <summary>0 = instant disk; positive = front grows from 0 to Radius, each target hit once per wave.</summary>
        public float ExpansionSeconds { get; }

        public AreaEffect(float radius, float damageMultiplier = 1f, float expansionSeconds = 0f)
        {
            NumericValidation.ValidatePositive(radius, nameof(radius));
            NumericValidation.ValidateNonNegativeFinite(damageMultiplier, nameof(damageMultiplier));
            Radius = radius;
            DamageMultiplier = damageMultiplier;
            NumericValidation.ValidateNonNegative(expansionSeconds, nameof(expansionSeconds));
            ExpansionSeconds = expansionSeconds;
        }
    }
}
