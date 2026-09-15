using Game.Content;

namespace Game.ActiveSkill
{
    public sealed class AreaEffect : IActiveSkillEffect
    {
        public float Radius { get; }
        public float DamageMultiplier { get; }

        public AreaEffect(float radius, float damageMultiplier = 1f)
        {
            NumericValidation.ValidatePositive(radius, nameof(radius));
            NumericValidation.ValidateNonNegativeFinite(damageMultiplier, nameof(damageMultiplier));
            Radius = radius;
            DamageMultiplier = damageMultiplier;
        }
    }
}
