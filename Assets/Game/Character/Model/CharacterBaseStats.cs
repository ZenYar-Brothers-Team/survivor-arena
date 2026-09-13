using System;

namespace Game.Character
{
    public readonly struct CharacterBaseStats
    {
        public float MaxHealth { get; }
        public float MovementSpeed { get; }
        public float ActiveSkillDamageMultiplier { get; }
        public float ActiveSkillCooldownMultiplier { get; }
        public float IncomingDamageMultiplier { get; }
        public float HealthRestorationMultiplier { get; }
        public float HealthRegenerationPerSecond { get; }
        public float DisappearingXpRecovery { get; }

        public CharacterBaseStats(
            float maxHealth,
            float movementSpeed,
            float activeSkillDamageMultiplier = 1f,
            float activeSkillCooldownMultiplier = 1f,
            float incomingDamageMultiplier = 1f,
            float healthRestorationMultiplier = 1f,
            float healthRegenerationPerSecond = 0f,
            float disappearingXpRecovery = 0f)
        {
            MaxHealth = maxHealth;
            MovementSpeed = movementSpeed;
            ActiveSkillDamageMultiplier = activeSkillDamageMultiplier;
            ActiveSkillCooldownMultiplier = activeSkillCooldownMultiplier;
            IncomingDamageMultiplier = incomingDamageMultiplier;
            HealthRestorationMultiplier = healthRestorationMultiplier;
            HealthRegenerationPerSecond = healthRegenerationPerSecond;
            DisappearingXpRecovery = disappearingXpRecovery;

            Validate();
        }

        internal void Validate()
        {
            ValidatePositive(MaxHealth, nameof(MaxHealth));
            ValidateNonNegative(MovementSpeed, nameof(MovementSpeed));
            ValidateNonNegative(ActiveSkillDamageMultiplier, nameof(ActiveSkillDamageMultiplier));
            ValidateNonNegative(ActiveSkillCooldownMultiplier, nameof(ActiveSkillCooldownMultiplier));
            ValidateNonNegative(IncomingDamageMultiplier, nameof(IncomingDamageMultiplier));
            ValidateNonNegative(HealthRestorationMultiplier, nameof(HealthRestorationMultiplier));
            ValidateNonNegative(HealthRegenerationPerSecond, nameof(HealthRegenerationPerSecond));
            ValidateRange(DisappearingXpRecovery, 0f, 1f, nameof(DisappearingXpRecovery));
        }

        private static void ValidatePositive(float value, string parameterName)
        {
            ValidateFinite(value, parameterName);
            if (value <= 0f)
                throw new ArgumentOutOfRangeException(parameterName, "Value must be greater than zero.");
        }

        private static void ValidateNonNegative(float value, string parameterName)
        {
            ValidateFinite(value, parameterName);
            if (value < 0f)
                throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative.");
        }

        private static void ValidateRange(float value, float minimum, float maximum, string parameterName)
        {
            ValidateFinite(value, parameterName);
            if (value < minimum || value > maximum)
                throw new ArgumentOutOfRangeException(parameterName, $"Value must be between {minimum} and {maximum}.");
        }

        private static void ValidateFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
        }
    }
}
