namespace Game.Progression.Json
{
    // JSON shape for PassiveProgressionDefinition: one entry per level, matching
    // CharacterStatModifier's bonus fields. Omitted fields default to 0, same as
    // CharacterStatModifier's own constructor defaults.
    public sealed class PassiveProgressionData
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public CharacterStatModifierData[] Levels { get; set; }
    }

    public sealed class CharacterStatModifierData
    {
        public float MaxHealthMultiplierBonus { get; set; }
        public float MovementSpeedMultiplierBonus { get; set; }
        public float ActiveSkillDamageMultiplierBonus { get; set; }
        public float ActiveSkillCooldownReductionBonus { get; set; }
        public float IncomingDamageReductionBonus { get; set; }
        public float HealthRestorationMultiplierBonus { get; set; }
        public float HealthRegenerationPerSecondBonus { get; set; }
        public float DisappearingXpRecoveryBonus { get; set; }
        public float PickedUpXpMultiplierBonus { get; set; }
        public float XpDropLifetimeBonusSeconds { get; set; }
    }
}
