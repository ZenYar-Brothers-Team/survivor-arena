namespace Game.Character.Json
{
    // Required authoring values; neutral defaults belong to domain types, never DTOs.
    public sealed class CharacterBaseStatsData
    {
        public float? MaxHealth { get; set; }
        public float? MovementSpeed { get; set; }
        public float? ActiveSkillDamageMultiplier { get; set; }
        public float? ActiveSkillCooldownMultiplier { get; set; }
        public float? IncomingDamageMultiplier { get; set; }
        public float? HealthRestorationMultiplier { get; set; }
        public float? HealthRegenerationPerSecond { get; set; }
        public float? DisappearingXpRecovery { get; set; }
        public float? PickedUpXpMultiplier { get; set; }
        public float? XpDropLifetimeBonusSeconds { get; set; }
        public float? PickupRadius { get; set; }
        public float? KnockbackResistance { get; set; }
        public float? OutgoingKnockbackBonus { get; set; }
        public float? EffectSizeMultiplier { get; set; }
        public float? EffectRangeMultiplier { get; set; }
        public float? PotionDropMultiplier { get; set; }
        public float? LowHealthDamageMaxBonus { get; set; }
    }
}
