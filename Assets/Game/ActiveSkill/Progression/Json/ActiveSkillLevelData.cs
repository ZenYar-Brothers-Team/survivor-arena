namespace Game.ActiveSkill.Json
{
    public sealed class ActiveSkillLevelData
    {
        public float? TargetingRadius { get; set; }
        public int? RandomSeed { get; set; }
        public float? InitialDirectionDegrees { get; set; }
        public float RotationPerActivationDegrees { get; set; }
        public float ActionSpeedBonus { get; set; }
        public float BaseDamage { get; set; }
        public float CooldownSeconds { get; set; }
        public ActiveSkillTargetingMode TargetingMode { get; set; }
        public string VisualId { get; set; }
        public ActiveSkillActivationWaveData[] Waves { get; set; }
    }
}
