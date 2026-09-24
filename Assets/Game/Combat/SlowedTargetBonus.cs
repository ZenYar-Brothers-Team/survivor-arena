using Game.Content;

namespace Game.Combat
{
    /// <summary>Additive bonuses to the damage and outgoing-knockback channels against an already slowed target.</summary>
    public readonly struct SlowedTargetBonus
    {
        public float DamageBonus { get; }
        public float KnockbackBonus { get; }

        public SlowedTargetBonus(float damageBonus, float knockbackBonus)
        {
            NumericValidation.ValidateNonNegative(damageBonus, nameof(damageBonus));
            NumericValidation.ValidateNonNegative(knockbackBonus, nameof(knockbackBonus));
            DamageBonus = damageBonus;
            KnockbackBonus = knockbackBonus;
        }

        public SlowedTargetBonus Plus(SlowedTargetBonus other) =>
            new SlowedTargetBonus(DamageBonus + other.DamageBonus, KnockbackBonus + other.KnockbackBonus);

        /// <summary>Multiplicative damage factor for a hit whose unconditional damage multiplier is <paramref name="damageMultiplier"/>.</summary>
        public float DamageFactor(float damageMultiplier) =>
            damageMultiplier > 0f ? (damageMultiplier + DamageBonus) / damageMultiplier : 1f;
    }
}
