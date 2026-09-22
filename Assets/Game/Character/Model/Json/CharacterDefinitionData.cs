namespace Game.Character.Json
{
    public sealed class CharacterDefinitionData
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string LockReason { get; set; }
        public CharacterPresentationData Presentation { get; set; }
        public bool InitiallyUnlocked { get; set; }
        public string StartingActiveSkillId { get; set; }
        public string VisualId { get; set; }
        public string MotionProfileId { get; set; }
        public CharacterBaseStatsData BaseStats { get; set; }
        public CharacterDraftWeightData[] DraftWeights { get; set; }
    }
}
