namespace Game.ActiveSkill.Json
{
    public sealed class StrikeEffectData : IActiveSkillEffectData
    {
        public ActiveSkillEffectKind Kind => ActiveSkillEffectKind.Strike;
        public float Radius { get; set; }
        public float TelegraphSeconds { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
        /// <summary>Optional ground-ellipse ratio; omitted = 1 (circle).</summary>
        public float? VerticalScale { get; set; }
    }
}
