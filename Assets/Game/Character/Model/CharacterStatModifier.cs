using Game.Content;

namespace Game.Character
{
    public readonly struct CharacterStatModifier
    {
        public float MaxHealthMultiplierBonus { get; }
        public float MovementSpeedMultiplierBonus { get; }
        public float ActiveSkillDamageMultiplierBonus { get; }
        public float ActionSpeedBonus { get; }
        public float IncomingDamageReductionBonus { get; }
        public float HealthRestorationMultiplierBonus { get; }
        public float HealthRegenerationPerSecondBonus { get; }
        public float DisappearingXpRecoveryBonus { get; }
        public float PickedUpXpMultiplierBonus { get; }
        public float XpDropLifetimeBonusSeconds { get; }
        public float KnockbackResistanceBonus { get; }
        public float OutgoingKnockbackBonus { get; }
        public float PickupRadiusMultiplierBonus { get; }
        public float EffectSizeMultiplierBonus { get; }
        public float EffectRangeMultiplierBonus { get; }
        public float PotionDropMultiplierBonus { get; }
        public float LowHealthDamageMaxBonus { get; }

        public CharacterStatModifier(
            float maxHealthMultiplierBonus = 0f,
            float movementSpeedMultiplierBonus = 0f,
            float activeSkillDamageMultiplierBonus = 0f,
            float actionSpeedBonus = 0f,
            float incomingDamageReductionBonus = 0f,
            float healthRestorationMultiplierBonus = 0f,
            float healthRegenerationPerSecondBonus = 0f,
            float disappearingXpRecoveryBonus = 0f,
            float pickedUpXpMultiplierBonus = 0f,
            float xpDropLifetimeBonusSeconds = 0f,
            float knockbackResistanceBonus = 0f,
            float outgoingKnockbackBonus = 0f,
            float pickupRadiusMultiplierBonus = 0f,
            float effectSizeMultiplierBonus = 0f,
            float effectRangeMultiplierBonus = 0f,
            float potionDropMultiplierBonus = 0f,
            float lowHealthDamageMaxBonus = 0f)
        {
            MaxHealthMultiplierBonus = maxHealthMultiplierBonus;
            MovementSpeedMultiplierBonus = movementSpeedMultiplierBonus;
            ActiveSkillDamageMultiplierBonus = activeSkillDamageMultiplierBonus;
            ActionSpeedBonus = actionSpeedBonus;
            IncomingDamageReductionBonus = incomingDamageReductionBonus;
            HealthRestorationMultiplierBonus = healthRestorationMultiplierBonus;
            HealthRegenerationPerSecondBonus = healthRegenerationPerSecondBonus;
            DisappearingXpRecoveryBonus = disappearingXpRecoveryBonus;
            PickedUpXpMultiplierBonus = pickedUpXpMultiplierBonus;
            XpDropLifetimeBonusSeconds = xpDropLifetimeBonusSeconds;
            KnockbackResistanceBonus = knockbackResistanceBonus;
            OutgoingKnockbackBonus = outgoingKnockbackBonus;
            PickupRadiusMultiplierBonus = pickupRadiusMultiplierBonus;
            EffectSizeMultiplierBonus = effectSizeMultiplierBonus;
            EffectRangeMultiplierBonus = effectRangeMultiplierBonus;
            PotionDropMultiplierBonus = potionDropMultiplierBonus;
            LowHealthDamageMaxBonus = lowHealthDamageMaxBonus;

            NumericValidation.ValidateFinite(maxHealthMultiplierBonus, nameof(maxHealthMultiplierBonus));
            NumericValidation.ValidateFinite(movementSpeedMultiplierBonus, nameof(movementSpeedMultiplierBonus));
            NumericValidation.ValidateFinite(activeSkillDamageMultiplierBonus, nameof(activeSkillDamageMultiplierBonus));
            NumericValidation.ValidateNonNegativeFinite(actionSpeedBonus, nameof(actionSpeedBonus));
            NumericValidation.ValidateNonNegativeFinite(incomingDamageReductionBonus, nameof(incomingDamageReductionBonus));
            NumericValidation.ValidateFinite(healthRestorationMultiplierBonus, nameof(healthRestorationMultiplierBonus));
            NumericValidation.ValidateFinite(healthRegenerationPerSecondBonus, nameof(healthRegenerationPerSecondBonus));
            NumericValidation.ValidateFinite(disappearingXpRecoveryBonus, nameof(disappearingXpRecoveryBonus));
            NumericValidation.ValidateFinite(pickedUpXpMultiplierBonus, nameof(pickedUpXpMultiplierBonus));
            NumericValidation.ValidateFinite(xpDropLifetimeBonusSeconds, nameof(xpDropLifetimeBonusSeconds));
            NumericValidation.ValidateNonNegative(knockbackResistanceBonus, nameof(knockbackResistanceBonus));
            NumericValidation.ValidateNonNegative(outgoingKnockbackBonus, nameof(outgoingKnockbackBonus));
            NumericValidation.ValidateNonNegative(pickupRadiusMultiplierBonus, nameof(pickupRadiusMultiplierBonus));
            NumericValidation.ValidateNonNegative(effectSizeMultiplierBonus, nameof(effectSizeMultiplierBonus));
            NumericValidation.ValidateNonNegative(effectRangeMultiplierBonus, nameof(effectRangeMultiplierBonus));
            NumericValidation.ValidateNonNegative(potionDropMultiplierBonus, nameof(potionDropMultiplierBonus));
            NumericValidation.ValidateNonNegative(lowHealthDamageMaxBonus, nameof(lowHealthDamageMaxBonus));
        }
    }
}
