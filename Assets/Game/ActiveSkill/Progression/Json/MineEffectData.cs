namespace Game.ActiveSkill.Json
{
    public sealed class MineEffectData : IActiveSkillEffectData
    {
        public ActiveSkillEffectKind Kind => ActiveSkillEffectKind.Mine;
        public float? SecondaryRadiusMultiplier { get; set; }
        public float? SecondaryKnockbackMultiplier { get; set; }
        public float TriggerRadius { get; set; }
        public float BlastRadius { get; set; }
        public float LifetimeSeconds { get; set; }
        public int MaxConcurrent { get; set; }
        public float SecondaryDelaySeconds { get; set; }
        public float SecondaryDamageMultiplier { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
    }
}
