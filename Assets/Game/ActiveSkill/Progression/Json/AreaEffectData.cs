namespace Game.ActiveSkill.Json
{
    public sealed class AreaEffectData : IActiveSkillEffectData
    {
        public ActiveSkillEffectKind Kind => ActiveSkillEffectKind.Area;
        public float Radius { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
    }
}
