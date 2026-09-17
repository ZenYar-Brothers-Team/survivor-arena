namespace Game.ActiveSkill.Json
{
    public sealed class ActiveSkillProgressionData
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public ActiveSkillLevelData[] Levels { get; set; }
    }
}
