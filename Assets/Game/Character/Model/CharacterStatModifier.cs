using Game.Content;

namespace Game.Character
{
    public readonly struct CharacterStatModifier
    {
        public float MaxHealthMultiplierBonus { get; }
        public float MovementSpeedMultiplierBonus { get; }
        public float ActiveSkillDamageMultiplierBonus { get; }
        public float ActiveSkillCooldownReductionBonus { get; }
        public float IncomingDamageReductionBonus { get; }
        public float HealthRestorationMultiplierBonus { get; }
        public float HealthRegenerationPerSecondBonus { get; }
        public float DisappearingXpRecoveryBonus { get; }
        public float PickedUpXpMultiplierBonus { get; }
        public float XpDropLifetimeBonusSeconds { get; }

        public CharacterStatModifier(
            float maxHealthMultiplierBonus = 0f,
            float movementSpeedMultiplierBonus = 0f,
            float activeSkillDamageMultiplierBonus = 0f,
            float activeSkillCooldownReductionBonus = 0f,
            float incomingDamageReductionBonus = 0f,
            float healthRestorationMultiplierBonus = 0f,
            float healthRegenerationPerSecondBonus = 0f,
            float disappearingXpRecoveryBonus = 0f,
            float pickedUpXpMultiplierBonus = 0f,
            float xpDropLifetimeBonusSeconds = 0f)
        {
            MaxHealthMultiplierBonus = maxHealthMultiplierBonus;
            MovementSpeedMultiplierBonus = movementSpeedMultiplierBonus;
            ActiveSkillDamageMultiplierBonus = activeSkillDamageMultiplierBonus;
            ActiveSkillCooldownReductionBonus = activeSkillCooldownReductionBonus;
            IncomingDamageReductionBonus = incomingDamageReductionBonus;
            HealthRestorationMultiplierBonus = healthRestorationMultiplierBonus;
            HealthRegenerationPerSecondBonus = healthRegenerationPerSecondBonus;
            DisappearingXpRecoveryBonus = disappearingXpRecoveryBonus;
            PickedUpXpMultiplierBonus = pickedUpXpMultiplierBonus;
            XpDropLifetimeBonusSeconds = xpDropLifetimeBonusSeconds;

            NumericValidation.ValidateFinite(maxHealthMultiplierBonus, nameof(maxHealthMultiplierBonus));
            NumericValidation.ValidateFinite(movementSpeedMultiplierBonus, nameof(movementSpeedMultiplierBonus));
            NumericValidation.ValidateFinite(activeSkillDamageMultiplierBonus, nameof(activeSkillDamageMultiplierBonus));
            NumericValidation.ValidateNonNegativeFinite(activeSkillCooldownReductionBonus, nameof(activeSkillCooldownReductionBonus));
            NumericValidation.ValidateNonNegativeFinite(incomingDamageReductionBonus, nameof(incomingDamageReductionBonus));
            NumericValidation.ValidateFinite(healthRestorationMultiplierBonus, nameof(healthRestorationMultiplierBonus));
            NumericValidation.ValidateFinite(healthRegenerationPerSecondBonus, nameof(healthRegenerationPerSecondBonus));
            NumericValidation.ValidateFinite(disappearingXpRecoveryBonus, nameof(disappearingXpRecoveryBonus));
            NumericValidation.ValidateFinite(pickedUpXpMultiplierBonus, nameof(pickedUpXpMultiplierBonus));
            NumericValidation.ValidateFinite(xpDropLifetimeBonusSeconds, nameof(xpDropLifetimeBonusSeconds));
        }
    }
}
