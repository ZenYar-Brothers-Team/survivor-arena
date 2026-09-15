namespace Game.ActiveSkill.Json
{
    public sealed class BeamEffectData : IActiveSkillEffectData
    {
        public ActiveSkillEffectKind Kind => ActiveSkillEffectKind.Beam;
        public float DurationSeconds { get; set; }
        public float TickIntervalSeconds { get; set; }
        public float Width { get; set; }
        public float Range { get; set; }
        public bool TracksTarget { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
    }
}
