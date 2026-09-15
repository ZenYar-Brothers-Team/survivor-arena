namespace Game.ActiveSkill.Json
{
    public sealed class ActiveSkillProgressionData
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public ActiveSkillLevelData[] Levels { get; set; }
    }

    public sealed class ActiveSkillLevelData
    {
        public float BaseDamage { get; set; }
        public float CooldownSeconds { get; set; }
        public ActiveSkillTargetingMode TargetingMode { get; set; }
        public string VisualId { get; set; }
        public ActiveSkillActivationWaveData[] Waves { get; set; }
    }

    public sealed class ActiveSkillActivationWaveData
    {
        public float DelaySeconds { get; set; }
        public float RotationDegrees { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
        public IActiveSkillEffectData[] Effects { get; set; }
    }
}
