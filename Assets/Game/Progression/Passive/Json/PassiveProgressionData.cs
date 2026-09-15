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
}
