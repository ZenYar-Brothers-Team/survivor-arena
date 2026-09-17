namespace Game.ActiveSkill.Json
{
    public sealed class ChainEffectData : IActiveSkillEffectData
    {
        public ActiveSkillEffectKind Kind => ActiveSkillEffectKind.Chain;
        public int TargetCount { get; set; }
        public float JumpRange { get; set; }
        public float DamageRetentionPerJump { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
    }
}
