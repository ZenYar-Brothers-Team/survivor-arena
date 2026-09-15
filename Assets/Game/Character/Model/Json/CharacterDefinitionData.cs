namespace Game.Character.Json
{
    public sealed class CharacterDefinitionData
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public bool InitiallyUnlocked { get; set; }
        public string StartingActiveSkillId { get; set; }
        public CharacterBaseStatsData BaseStats { get; set; }
        public CharacterDraftWeightData[] DraftWeights { get; set; }
    }
}
