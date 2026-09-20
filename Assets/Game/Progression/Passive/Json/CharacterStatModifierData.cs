namespace Game.Progression.Json
{
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
