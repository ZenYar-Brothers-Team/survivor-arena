namespace Game.ActiveSkill.Json
{
    public sealed class ActiveSkillActivationWaveData
    {
        public float DelaySeconds { get; set; }
        public float RotationDegrees { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
        public IActiveSkillEffectData[] Effects { get; set; }
    }
}
