namespace Game.Character.Json
{
    // JSON shape for CharacterBaseStats. Defaults mirror CharacterBaseStats'
    // own constructor defaults, so omitting a field in config behaves the same
    // as omitting the matching constructor argument.
    public sealed class CharacterBaseStatsData
    {
        public string Id { get; set; }
        public float MaxHealth { get; set; }
        public float MovementSpeed { get; set; }
        public float ActiveSkillDamageMultiplier { get; set; } = 1f;
        public float ActiveSkillCooldownMultiplier { get; set; } = 1f;
        public float IncomingDamageMultiplier { get; set; } = 1f;
        public float HealthRestorationMultiplier { get; set; } = 1f;
        public float HealthRegenerationPerSecond { get; set; }
        public float DisappearingXpRecovery { get; set; }
        public float PickedUpXpMultiplier { get; set; } = 1f;
        public float XpDropLifetimeBonusSeconds { get; set; }
        public float PickupRadius { get; set; }
    }
}
