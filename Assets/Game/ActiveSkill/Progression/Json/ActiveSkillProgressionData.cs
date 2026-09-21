using Newtonsoft.Json.Linq;

namespace Game.ActiveSkill.Json
{
    public sealed class ActiveSkillProgressionData
    {
        public JObject BaseLevel { get; set; }
        public ActiveSkillLevelChangeData[] LevelChanges { get; set; }
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public ActiveSkillLevelData[] Levels { get; set; }
    }
}
