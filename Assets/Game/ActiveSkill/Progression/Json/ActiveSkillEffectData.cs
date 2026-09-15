namespace Game.ActiveSkill.Json
{
    // JSON shape for the IActiveSkillEffect hierarchy. Each concrete effect
    // carries its own Kind so ActiveSkillEffectJsonConverter can pick the
    // right DTO type on the way in; see that converter for the read side.
    // Values match the JSON "kind" string exactly (StringEnumConverter).
    public enum ActiveSkillEffectKind
    {
        ProjectileBurst,
        Beam,
        Orbit,
        Boomerang,
        Chain,
        Area,
        Mine
    }

    public interface IActiveSkillEffectData
    {
        ActiveSkillEffectKind Kind { get; }
    }

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

    public sealed class OrbitEffectData : IActiveSkillEffectData
    {
        public ActiveSkillEffectKind Kind => ActiveSkillEffectKind.Orbit;
        public int BladeCount { get; set; }
        public float Radius { get; set; }
        public float AngularSpeedDegrees { get; set; }
        public float DurationSeconds { get; set; }
        public float HitCooldownSeconds { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
    }

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

    public sealed class ChainEffectData : IActiveSkillEffectData
    {
        public ActiveSkillEffectKind Kind => ActiveSkillEffectKind.Chain;
        public int TargetCount { get; set; }
        public float JumpRange { get; set; }
        public float DamageRetentionPerJump { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
    }

    public sealed class AreaEffectData : IActiveSkillEffectData
    {
        public ActiveSkillEffectKind Kind => ActiveSkillEffectKind.Area;
        public float Radius { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
    }

    public sealed class MineEffectData : IActiveSkillEffectData
    {
        public ActiveSkillEffectKind Kind => ActiveSkillEffectKind.Mine;
        public float TriggerRadius { get; set; }
        public float BlastRadius { get; set; }
        public float LifetimeSeconds { get; set; }
        public int MaxConcurrent { get; set; }
        public float SecondaryDelaySeconds { get; set; }
        public float SecondaryDamageMultiplier { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
    }
}
