namespace Game.ActiveSkill
{
    public sealed class AreaEffect : IActiveSkillEffect
    {
        public float Radius { get; }
        public float DamageMultiplier { get; }

        public AreaEffect(float radius, float damageMultiplier = 1f)
        {
            ProjectileBurstEffect.ValidatePositive(radius, nameof(radius));
            ProjectileBurstEffect.ValidateNonNegative(damageMultiplier, nameof(damageMultiplier));
            Radius = radius;
            DamageMultiplier = damageMultiplier;
        }
    }
}
