using Game.Content;

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
        public float PickedUpXpMultiplier { get; }
        public float XpDropLifetimeBonusSeconds { get; }
        public float PickupRadius { get; }

        public CharacterBaseStats(
            float maxHealth,
            float movementSpeed,
            float activeSkillDamageMultiplier = 1f,
            float activeSkillCooldownMultiplier = 1f,
            float incomingDamageMultiplier = 1f,
            float healthRestorationMultiplier = 1f,
            float healthRegenerationPerSecond = 0f,
            float disappearingXpRecovery = 0f,
            float pickedUpXpMultiplier = 1f,
            float xpDropLifetimeBonusSeconds = 0f,
            float pickupRadius = 0.2f)
        {
            MaxHealth = maxHealth;
            MovementSpeed = movementSpeed;
            ActiveSkillDamageMultiplier = activeSkillDamageMultiplier;
            ActiveSkillCooldownMultiplier = activeSkillCooldownMultiplier;
            IncomingDamageMultiplier = incomingDamageMultiplier;
            HealthRestorationMultiplier = healthRestorationMultiplier;
            HealthRegenerationPerSecond = healthRegenerationPerSecond;
            DisappearingXpRecovery = disappearingXpRecovery;
            PickedUpXpMultiplier = pickedUpXpMultiplier;
            XpDropLifetimeBonusSeconds = xpDropLifetimeBonusSeconds;
            PickupRadius = pickupRadius;

            Validate();
        }

        internal void Validate()
        {
            NumericValidation.ValidatePositive(MaxHealth, nameof(MaxHealth));
            NumericValidation.ValidateNonNegative(MovementSpeed, nameof(MovementSpeed));
            NumericValidation.ValidateNonNegative(ActiveSkillDamageMultiplier, nameof(ActiveSkillDamageMultiplier));
            NumericValidation.ValidatePositive(ActiveSkillCooldownMultiplier, nameof(ActiveSkillCooldownMultiplier));
            NumericValidation.ValidatePositive(IncomingDamageMultiplier, nameof(IncomingDamageMultiplier));
            NumericValidation.ValidateNonNegative(HealthRestorationMultiplier, nameof(HealthRestorationMultiplier));
            NumericValidation.ValidateNonNegative(HealthRegenerationPerSecond, nameof(HealthRegenerationPerSecond));
            NumericValidation.ValidateRange(DisappearingXpRecovery, 0f, 1f, nameof(DisappearingXpRecovery));
            NumericValidation.ValidateNonNegative(PickedUpXpMultiplier, nameof(PickedUpXpMultiplier));
            NumericValidation.ValidateNonNegative(XpDropLifetimeBonusSeconds, nameof(XpDropLifetimeBonusSeconds));
            NumericValidation.ValidatePositive(PickupRadius, nameof(PickupRadius));
        }
    }
}
