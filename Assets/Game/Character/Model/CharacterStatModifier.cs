using System;

namespace Game.Character
{
    public readonly struct CharacterStatModifier
    {
        public float MaxHealthMultiplierBonus { get; }
        public float MovementSpeedMultiplierBonus { get; }
        public float ActiveSkillDamageMultiplierBonus { get; }
        public float ActiveSkillCooldownMultiplierBonus { get; }
        public float IncomingDamageMultiplierBonus { get; }
        public float HealthRestorationMultiplierBonus { get; }
        public float HealthRegenerationPerSecondBonus { get; }
        public float DisappearingXpRecoveryBonus { get; }

        public CharacterStatModifier(
            float maxHealthMultiplierBonus = 0f,
            float movementSpeedMultiplierBonus = 0f,
            float activeSkillDamageMultiplierBonus = 0f,
            float activeSkillCooldownMultiplierBonus = 0f,
            float incomingDamageMultiplierBonus = 0f,
            float healthRestorationMultiplierBonus = 0f,
            float healthRegenerationPerSecondBonus = 0f,
            float disappearingXpRecoveryBonus = 0f)
        {
            MaxHealthMultiplierBonus = maxHealthMultiplierBonus;
            MovementSpeedMultiplierBonus = movementSpeedMultiplierBonus;
            ActiveSkillDamageMultiplierBonus = activeSkillDamageMultiplierBonus;
            ActiveSkillCooldownMultiplierBonus = activeSkillCooldownMultiplierBonus;
            IncomingDamageMultiplierBonus = incomingDamageMultiplierBonus;
            HealthRestorationMultiplierBonus = healthRestorationMultiplierBonus;
            HealthRegenerationPerSecondBonus = healthRegenerationPerSecondBonus;
            DisappearingXpRecoveryBonus = disappearingXpRecoveryBonus;

            ValidateFinite(maxHealthMultiplierBonus, nameof(maxHealthMultiplierBonus));
            ValidateFinite(movementSpeedMultiplierBonus, nameof(movementSpeedMultiplierBonus));
            ValidateFinite(activeSkillDamageMultiplierBonus, nameof(activeSkillDamageMultiplierBonus));
            ValidateFinite(activeSkillCooldownMultiplierBonus, nameof(activeSkillCooldownMultiplierBonus));
            ValidateFinite(incomingDamageMultiplierBonus, nameof(incomingDamageMultiplierBonus));
            ValidateFinite(healthRestorationMultiplierBonus, nameof(healthRestorationMultiplierBonus));
            ValidateFinite(healthRegenerationPerSecondBonus, nameof(healthRegenerationPerSecondBonus));
            ValidateFinite(disappearingXpRecoveryBonus, nameof(disappearingXpRecoveryBonus));
        }

        private static void ValidateFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
        }
    }
}
