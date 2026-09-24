using Game.Content;

namespace Game.ActiveSkill
{
    /// <summary>
    /// Telegraphed area strike: when its wave starts, the strike point is snapshotted
    /// (the activation target for the first wave, a new distinct random target for later
    /// RandomEnemy waves) and damage lands there after <see cref="TelegraphSeconds"/>.
    /// </summary>
    public sealed class StrikeEffect : IActiveSkillEffect
    {
        public float Radius { get; }
        public float TelegraphSeconds { get; }
        public float DamageMultiplier { get; }

        public StrikeEffect(float radius, float telegraphSeconds, float damageMultiplier = 1f)
        {
            NumericValidation.ValidatePositive(radius, nameof(radius));
            NumericValidation.ValidateNonNegative(telegraphSeconds, nameof(telegraphSeconds));
            NumericValidation.ValidateNonNegativeFinite(damageMultiplier, nameof(damageMultiplier));
            Radius = radius;
            TelegraphSeconds = telegraphSeconds;
            DamageMultiplier = damageMultiplier;
        }
    }
}
