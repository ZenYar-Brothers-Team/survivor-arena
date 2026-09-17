namespace Game.ActiveSkill.Json
{
    public sealed class ProjectileBurstEffectData : IActiveSkillEffectData
    {
        public ActiveSkillEffectKind Kind => ActiveSkillEffectKind.ProjectileBurst;
        public int ProjectileCount { get; set; }
        public ProjectileLayout Layout { get; set; }
        public float SpreadDegrees { get; set; }
        public int PierceCount { get; set; }
        public float Speed { get; set; }
        public float LifetimeSeconds { get; set; }
        public float CollisionRadius { get; set; }
        public float ImpactAreaRadius { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
    }
}
