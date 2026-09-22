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
        public CombatDamageRequest(CombatSource source, float amount, CombatControlProfile controls = null,
            float directionX = 0f, float directionY = 0f, float outgoingKnockbackMultiplier = 1f)
        {
            NumericValidation.ValidateNonNegative(amount, nameof(amount));
            NumericValidation.ValidateFinite(directionX, nameof(directionX));
            NumericValidation.ValidateFinite(directionY, nameof(directionY));
            NumericValidation.ValidateNonNegative(outgoingKnockbackMultiplier, nameof(outgoingKnockbackMultiplier));
            Source = source;
            Amount = amount;
            Controls = controls ?? CombatControlProfile.None;
            DirectionX = directionX;
            DirectionY = directionY;
            OutgoingKnockbackMultiplier = outgoingKnockbackMultiplier;
        }
        public CombatDamageRequest WithAmount(float amount) => new CombatDamageRequest(Source, amount, Controls,
            DirectionX, DirectionY, OutgoingKnockbackMultiplier);
        public CombatDamageRequest WithDirection(float x, float y) => new CombatDamageRequest(Source, Amount, Controls,
            x, y, OutgoingKnockbackMultiplier);
    }
}
