using Game.Content;

namespace Game.ActiveSkill
{
    public sealed class AreaEffect : IActiveSkillEffect
    {
        public float Radius { get; }
        public float DamageMultiplier { get; }
        /// <summary>0 = instant disk; positive = front grows from 0 to Radius, each target hit once per wave.</summary>
        public float ExpansionSeconds { get; }
        /// <summary>0 = full disk; positive = instant cone of this total angle centred on the aim direction (SET-022).</summary>
        public float ArcDegrees { get; }

        public AreaEffect(float radius, float damageMultiplier = 1f, float expansionSeconds = 0f, float arcDegrees = 0f)
        {
            NumericValidation.ValidatePositive(radius, nameof(radius));
            NumericValidation.ValidateNonNegativeFinite(damageMultiplier, nameof(damageMultiplier));
            Radius = radius;
            DamageMultiplier = damageMultiplier;
            NumericValidation.ValidateNonNegative(expansionSeconds, nameof(expansionSeconds));
            ExpansionSeconds = expansionSeconds;
            NumericValidation.ValidateRange(arcDegrees, 0f, 360f, nameof(arcDegrees));
            if (arcDegrees > 0f && expansionSeconds > 0f) throw new System.ArgumentException("A cone is instant; arc and expansion cannot combine.", nameof(arcDegrees));
            ArcDegrees = arcDegrees;
        }
    }
}
