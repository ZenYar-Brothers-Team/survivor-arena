using System;

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

            ValidateFinite(maxHealthMultiplierBonus, nameof(maxHealthMultiplierBonus));
            ValidateFinite(movementSpeedMultiplierBonus, nameof(movementSpeedMultiplierBonus));
            ValidateFinite(activeSkillDamageMultiplierBonus, nameof(activeSkillDamageMultiplierBonus));
            ValidateNonNegativeFinite(activeSkillCooldownReductionBonus, nameof(activeSkillCooldownReductionBonus));
            ValidateNonNegativeFinite(incomingDamageReductionBonus, nameof(incomingDamageReductionBonus));
            ValidateFinite(healthRestorationMultiplierBonus, nameof(healthRestorationMultiplierBonus));
            ValidateFinite(healthRegenerationPerSecondBonus, nameof(healthRegenerationPerSecondBonus));
            ValidateFinite(disappearingXpRecoveryBonus, nameof(disappearingXpRecoveryBonus));
            ValidateFinite(pickedUpXpMultiplierBonus, nameof(pickedUpXpMultiplierBonus));
            ValidateFinite(xpDropLifetimeBonusSeconds, nameof(xpDropLifetimeBonusSeconds));
        }

        private static void ValidateFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
        }

        private static void ValidateNonNegativeFinite(float value, string parameterName)
        {
            ValidateFinite(value, parameterName);
            if (value < 0f)
                throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative.");
        }
    }
}
