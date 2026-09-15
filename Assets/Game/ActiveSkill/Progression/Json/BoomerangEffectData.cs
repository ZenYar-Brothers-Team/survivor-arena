namespace Game.ActiveSkill.Json
{
    public sealed class BoomerangEffectData : IActiveSkillEffectData
    {
        public ActiveSkillEffectKind Kind => ActiveSkillEffectKind.Boomerang;
        public int ProjectileCount { get; set; }
        public float SpreadDegrees { get; set; }
        public float Speed { get; set; }
        public float Range { get; set; }
        public float CollisionRadius { get; set; }
        public float ReturnDamageMultiplier { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
    }
}
