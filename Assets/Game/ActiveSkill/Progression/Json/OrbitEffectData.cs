namespace Game.ActiveSkill.Json
{
    public sealed class OrbitEffectData : IActiveSkillEffectData
    {
        public ActiveSkillEffectKind Kind => ActiveSkillEffectKind.Orbit;
        public float? BladeHitboxRadius { get; set; }
        public int BladeCount { get; set; }
        public float Radius { get; set; }
        public float AngularSpeedDegrees { get; set; }
        public float DurationSeconds { get; set; }
        public float HitCooldownSeconds { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
    }
}
