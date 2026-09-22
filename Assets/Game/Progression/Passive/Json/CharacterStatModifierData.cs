namespace Game.Progression.Json
{
    public sealed class CharacterStatModifierData
    {
        public float MaxHealthMultiplierBonus { get; set; }
        public float MovementSpeedMultiplierBonus { get; set; }
        public float ActiveSkillDamageMultiplierBonus { get; set; }
        public float ActionSpeedBonus { get; set; }
        public float IncomingDamageReductionBonus { get; set; }
        public float HealthRestorationMultiplierBonus { get; set; }
        public float HealthRegenerationPerSecondBonus { get; set; }
        public float DisappearingXpRecoveryBonus { get; set; }
        public float PickedUpXpMultiplierBonus { get; set; }
        public float XpDropLifetimeBonusSeconds { get; set; }
        public float? KnockbackResistanceBonus { get; set; }
        public float? OutgoingKnockbackBonus { get; set; }
        public float? PickupRadiusMultiplierBonus { get; set; }
        public float? EffectSizeMultiplierBonus { get; set; }
        public float? EffectRangeMultiplierBonus { get; set; }
        public float? PotionDropMultiplierBonus { get; set; }
        public float? LowHealthDamageMaxBonus { get; set; }
    }
}
