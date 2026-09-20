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
        public float KnockbackResistance { get; }
        public float OutgoingKnockbackBonus { get; }
        public float EffectSizeMultiplier { get; }
        public float EffectRangeMultiplier { get; }
        public float PotionDropMultiplier { get; }
        public float LowHealthDamageMaxBonus { get; }

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
            float pickupRadius = 0.2f,
            float knockbackResistance = 0f,
            float outgoingKnockbackBonus = 0f,
            float effectSizeMultiplier = 1f,
            float effectRangeMultiplier = 1f,
            float potionDropMultiplier = 1f,
            float lowHealthDamageMaxBonus = 0f)
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
            KnockbackResistance = knockbackResistance;
            OutgoingKnockbackBonus = outgoingKnockbackBonus;
            EffectSizeMultiplier = effectSizeMultiplier;
            EffectRangeMultiplier = effectRangeMultiplier;
            PotionDropMultiplier = potionDropMultiplier;
            LowHealthDamageMaxBonus = lowHealthDamageMaxBonus;

            Validate();
        }

        public void Validate()
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
            NumericValidation.ValidateRange(KnockbackResistance, 0f, 1f, nameof(KnockbackResistance));
            NumericValidation.ValidateNonNegative(OutgoingKnockbackBonus, nameof(OutgoingKnockbackBonus));
            NumericValidation.ValidatePositive(EffectSizeMultiplier, nameof(EffectSizeMultiplier));
            NumericValidation.ValidatePositive(EffectRangeMultiplier, nameof(EffectRangeMultiplier));
            NumericValidation.ValidateNonNegative(PotionDropMultiplier, nameof(PotionDropMultiplier));
            NumericValidation.ValidateNonNegative(LowHealthDamageMaxBonus, nameof(LowHealthDamageMaxBonus));
        }
    }
}
