using Game.Content;

namespace Game.Combat
{
    public readonly struct CombatDamageRequest
    {
        public CombatSource Source { get; }
        public float Amount { get; }
        public CombatControlProfile Controls { get; }
        public float DirectionX { get; }
        public float DirectionY { get; }
        public float OutgoingKnockbackMultiplier { get; }
        /// <summary>Damage factor applied only when the target was already slowed before this hit (SET-010).</summary>
        public float SlowedTargetDamageFactor { get; }
        /// <summary>Additive outgoing-knockback bonus applied only against an already slowed target (SET-004).</summary>
        public float SlowedTargetKnockbackBonus { get; }

        public CombatDamageRequest(CombatSource source, float amount, CombatControlProfile controls = null,
            float directionX = 0f, float directionY = 0f, float outgoingKnockbackMultiplier = 1f,
            float slowedTargetDamageFactor = 1f, float slowedTargetKnockbackBonus = 0f)
        {
            NumericValidation.ValidateNonNegative(amount, nameof(amount));
            NumericValidation.ValidateFinite(directionX, nameof(directionX));
            NumericValidation.ValidateFinite(directionY, nameof(directionY));
            NumericValidation.ValidateNonNegative(outgoingKnockbackMultiplier, nameof(outgoingKnockbackMultiplier));
            NumericValidation.ValidateNonNegative(slowedTargetDamageFactor, nameof(slowedTargetDamageFactor));
            NumericValidation.ValidateNonNegative(slowedTargetKnockbackBonus, nameof(slowedTargetKnockbackBonus));
            Source = source;
            Amount = amount;
            Controls = controls ?? CombatControlProfile.None;
            DirectionX = directionX;
            DirectionY = directionY;
            OutgoingKnockbackMultiplier = outgoingKnockbackMultiplier;
            SlowedTargetDamageFactor = slowedTargetDamageFactor;
            SlowedTargetKnockbackBonus = slowedTargetKnockbackBonus;
        }

        public CombatDamageRequest WithAmount(float amount) => new CombatDamageRequest(Source, amount, Controls,
            DirectionX, DirectionY, OutgoingKnockbackMultiplier, SlowedTargetDamageFactor, SlowedTargetKnockbackBonus);
        public CombatDamageRequest WithDirection(float x, float y) => new CombatDamageRequest(Source, Amount, Controls,
            x, y, OutgoingKnockbackMultiplier, SlowedTargetDamageFactor, SlowedTargetKnockbackBonus);

        /// <summary>Scales amount and knockback (e.g. ricochet retention, explosion ratios) keeping conditional bonuses.</summary>
        public CombatDamageRequest Scaled(float amountMultiplier, float knockbackMultiplier) => new CombatDamageRequest(Source,
            Amount * amountMultiplier, Controls, DirectionX, DirectionY, OutgoingKnockbackMultiplier * knockbackMultiplier,
            SlowedTargetDamageFactor, SlowedTargetKnockbackBonus);

        public CombatDamageRequest WithSlowedTargetBonus(float damageFactor, float knockbackBonus) => new CombatDamageRequest(Source,
            Amount, Controls, DirectionX, DirectionY, OutgoingKnockbackMultiplier, damageFactor, knockbackBonus);

        /// <summary>
        /// Final request against a target that was slowed before this hit: conditional bonuses are applied once
        /// and cleared, so the current hit's own slow never enables them (baseline v1 predicate).
        /// </summary>
        public CombatDamageRequest ResolveForSlowedTarget() => new CombatDamageRequest(Source, Amount * SlowedTargetDamageFactor,
            Controls, DirectionX, DirectionY, OutgoingKnockbackMultiplier + SlowedTargetKnockbackBonus);
    }
}
