namespace Game.ActiveSkill.Json
{
    public sealed class ActiveSkillLevelData
    {
        public float BaseDamage { get; set; }
        public float CooldownSeconds { get; set; }
        public ActiveSkillTargetingMode TargetingMode { get; set; }
        public string VisualId { get; set; }
        public ActiveSkillActivationWaveData[] Waves { get; set; }
    }
}
