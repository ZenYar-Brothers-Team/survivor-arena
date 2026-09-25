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
        /// <summary>Vertical/horizontal ratio of the struck ground ellipse: the 3/4 camera sees a ground
        /// circle flattened (1 = circle; DECISION-0058). Horizontal extent stays <see cref="Radius"/>.</summary>
        public float VerticalScale { get; }

        public StrikeEffect(float radius, float telegraphSeconds, float damageMultiplier = 1f, float verticalScale = 1f)
        {
            NumericValidation.ValidatePositive(radius, nameof(radius));
            NumericValidation.ValidateNonNegative(telegraphSeconds, nameof(telegraphSeconds));
            NumericValidation.ValidateNonNegativeFinite(damageMultiplier, nameof(damageMultiplier));
            Radius = radius;
            TelegraphSeconds = telegraphSeconds;
            DamageMultiplier = damageMultiplier;
            NumericValidation.ValidateRange(verticalScale, 0.1f, 1f, nameof(verticalScale));
            VerticalScale = verticalScale;
        }
    }
}
