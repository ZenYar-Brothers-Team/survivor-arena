namespace Game.Progression.Json
{
    /// <summary>JSON authoring of <see cref="SkillMechanicBonus"/>; omitted fields are the neutral zero owned by that type.</summary>
    public sealed class SkillMechanicBonusData
    {
        public float ProjectileSpeedBonus { get; set; }
        public float ReturnDamageBonus { get; set; }
        public float ReturnSpeedBonus { get; set; }
        public int ExtraChainTargets { get; set; }
        public float ChainJumpRangeBonus { get; set; }
        public float ChainFalloffReduction { get; set; }
        public int ExtraPierce { get; set; }
        public float OrbitAngularSpeedBonus { get; set; }
        public float ExplosionDamageBonus { get; set; }
        public float ExplosionRadiusBonus { get; set; }
        public int HeavyEveryNth { get; set; }
        public float HeavySizeMultiplier { get; set; }
        public float HeavyStopMultiplier { get; set; }
        public float HeavyExplosionRadius { get; set; }
        public float HeavyExplosionDamageMultiplier { get; set; }
        public float HeavyExplosionKnockback { get; set; }

        public SkillMechanicBonus ToBonus() => new SkillMechanicBonus(ProjectileSpeedBonus, ReturnDamageBonus, ReturnSpeedBonus,
            ExtraChainTargets, ChainJumpRangeBonus, ChainFalloffReduction, ExtraPierce, OrbitAngularSpeedBonus,
            ExplosionDamageBonus, ExplosionRadiusBonus, HeavyEveryNth, HeavySizeMultiplier, HeavyStopMultiplier,
            HeavyExplosionRadius, HeavyExplosionDamageMultiplier, HeavyExplosionKnockback);
    }
}
