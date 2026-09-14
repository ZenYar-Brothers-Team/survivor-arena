using System;

namespace Game.ActiveSkill
{
    public sealed class ProjectileBurstEffect : IActiveSkillEffect
    {
        public int ProjectileCount { get; }
        public ProjectileLayout Layout { get; }
        public float SpreadDegrees { get; }
        public int PierceCount { get; }
        public float Speed { get; }
        public float LifetimeSeconds { get; }
        public float CollisionRadius { get; }
        public float ImpactAreaRadius { get; }
        public float DamageMultiplier { get; }

        public ProjectileBurstEffect(
            int projectileCount,
            ProjectileLayout layout,
            float spreadDegrees,
            int pierceCount,
            float speed,
            float lifetimeSeconds,
            float collisionRadius,
            float impactAreaRadius = 0f,
            float damageMultiplier = 1f)
        {
            ValidateCount(projectileCount, nameof(projectileCount));
            if (!Enum.IsDefined(typeof(ProjectileLayout), layout))
                throw new ArgumentOutOfRangeException(nameof(layout));
            ValidateNonNegative(spreadDegrees, nameof(spreadDegrees));
            if (pierceCount < 0)
                throw new ArgumentOutOfRangeException(nameof(pierceCount));
            ValidatePositive(speed, nameof(speed));
            ValidatePositive(lifetimeSeconds, nameof(lifetimeSeconds));
            ValidatePositive(collisionRadius, nameof(collisionRadius));
            ValidateNonNegative(impactAreaRadius, nameof(impactAreaRadius));
            ValidateNonNegative(damageMultiplier, nameof(damageMultiplier));

            ProjectileCount = projectileCount;
            Layout = layout;
            SpreadDegrees = spreadDegrees;
            PierceCount = pierceCount;
            Speed = speed;
            LifetimeSeconds = lifetimeSeconds;
            CollisionRadius = collisionRadius;
            ImpactAreaRadius = impactAreaRadius;
            DamageMultiplier = damageMultiplier;
        }

        internal static void ValidateCount(int value, string name)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(name, "Count must be greater than zero.");
        }

        internal static void ValidatePositive(float value, string name)
        {
            ValidateNonNegative(value, name);
            if (value <= 0f)
                throw new ArgumentOutOfRangeException(name, "Value must be greater than zero.");
        }

        internal static void ValidateNonNegative(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(name, "Value must be finite and non-negative.");
        }
    }

    public sealed class BeamEffect : IActiveSkillEffect
    {
        public float DurationSeconds { get; }
        public float TickIntervalSeconds { get; }
        public float Width { get; }
        public float Range { get; }
        public bool TracksTarget { get; }
        public float DamageMultiplier { get; }

        public BeamEffect(float durationSeconds, float tickIntervalSeconds, float width, float range, bool tracksTarget, float damageMultiplier = 1f)
        {
            ProjectileBurstEffect.ValidatePositive(durationSeconds, nameof(durationSeconds));
            ProjectileBurstEffect.ValidatePositive(tickIntervalSeconds, nameof(tickIntervalSeconds));
            ProjectileBurstEffect.ValidatePositive(width, nameof(width));
            ProjectileBurstEffect.ValidatePositive(range, nameof(range));
            ProjectileBurstEffect.ValidateNonNegative(damageMultiplier, nameof(damageMultiplier));
            DurationSeconds = durationSeconds;
            TickIntervalSeconds = tickIntervalSeconds;
            Width = width;
            Range = range;
            TracksTarget = tracksTarget;
            DamageMultiplier = damageMultiplier;
        }
    }

    public sealed class OrbitEffect : IActiveSkillEffect
    {
        public int BladeCount { get; }
        public float Radius { get; }
        public float AngularSpeedDegrees { get; }
        public float DurationSeconds { get; }
        public float HitCooldownSeconds { get; }
        public float DamageMultiplier { get; }

        public OrbitEffect(int bladeCount, float radius, float angularSpeedDegrees, float durationSeconds, float hitCooldownSeconds, float damageMultiplier = 1f)
        {
            ProjectileBurstEffect.ValidateCount(bladeCount, nameof(bladeCount));
            ProjectileBurstEffect.ValidatePositive(radius, nameof(radius));
            ProjectileBurstEffect.ValidatePositive(angularSpeedDegrees, nameof(angularSpeedDegrees));
            ProjectileBurstEffect.ValidatePositive(durationSeconds, nameof(durationSeconds));
            ProjectileBurstEffect.ValidatePositive(hitCooldownSeconds, nameof(hitCooldownSeconds));
            ProjectileBurstEffect.ValidateNonNegative(damageMultiplier, nameof(damageMultiplier));
            BladeCount = bladeCount;
            Radius = radius;
            AngularSpeedDegrees = angularSpeedDegrees;
            DurationSeconds = durationSeconds;
            HitCooldownSeconds = hitCooldownSeconds;
            DamageMultiplier = damageMultiplier;
        }
    }

    public sealed class BoomerangEffect : IActiveSkillEffect
    {
        public int ProjectileCount { get; }
        public float SpreadDegrees { get; }
        public float Speed { get; }
        public float Range { get; }
        public float CollisionRadius { get; }
        public float ReturnDamageMultiplier { get; }
        public float DamageMultiplier { get; }

        public BoomerangEffect(int projectileCount, float spreadDegrees, float speed, float range, float collisionRadius, float returnDamageMultiplier, float damageMultiplier = 1f)
        {
            ProjectileBurstEffect.ValidateCount(projectileCount, nameof(projectileCount));
            ProjectileBurstEffect.ValidateNonNegative(spreadDegrees, nameof(spreadDegrees));
            ProjectileBurstEffect.ValidatePositive(speed, nameof(speed));
            ProjectileBurstEffect.ValidatePositive(range, nameof(range));
            ProjectileBurstEffect.ValidatePositive(collisionRadius, nameof(collisionRadius));
            ProjectileBurstEffect.ValidateNonNegative(returnDamageMultiplier, nameof(returnDamageMultiplier));
            ProjectileBurstEffect.ValidateNonNegative(damageMultiplier, nameof(damageMultiplier));
            ProjectileCount = projectileCount;
            SpreadDegrees = spreadDegrees;
            Speed = speed;
            Range = range;
            CollisionRadius = collisionRadius;
            ReturnDamageMultiplier = returnDamageMultiplier;
            DamageMultiplier = damageMultiplier;
        }
    }

    public sealed class ChainEffect : IActiveSkillEffect
    {
        public int TargetCount { get; }
        public float JumpRange { get; }
        public float DamageRetentionPerJump { get; }
        public float DamageMultiplier { get; }

        public ChainEffect(int targetCount, float jumpRange, float damageRetentionPerJump, float damageMultiplier = 1f)
        {
            ProjectileBurstEffect.ValidateCount(targetCount, nameof(targetCount));
            ProjectileBurstEffect.ValidatePositive(jumpRange, nameof(jumpRange));
            ProjectileBurstEffect.ValidateNonNegative(damageRetentionPerJump, nameof(damageRetentionPerJump));
            if (damageRetentionPerJump > 1f)
                throw new ArgumentOutOfRangeException(nameof(damageRetentionPerJump));
            ProjectileBurstEffect.ValidateNonNegative(damageMultiplier, nameof(damageMultiplier));
            TargetCount = targetCount;
            JumpRange = jumpRange;
            DamageRetentionPerJump = damageRetentionPerJump;
            DamageMultiplier = damageMultiplier;
        }
    }

    public sealed class AreaEffect : IActiveSkillEffect
    {
        public float Radius { get; }
        public float DamageMultiplier { get; }

        public AreaEffect(float radius, float damageMultiplier = 1f)
        {
            ProjectileBurstEffect.ValidatePositive(radius, nameof(radius));
            ProjectileBurstEffect.ValidateNonNegative(damageMultiplier, nameof(damageMultiplier));
            Radius = radius;
            DamageMultiplier = damageMultiplier;
        }
    }

    public sealed class MineEffect : IActiveSkillEffect
    {
        public float TriggerRadius { get; }
        public float BlastRadius { get; }
        public float LifetimeSeconds { get; }
        public int MaxConcurrent { get; }
        public float SecondaryDelaySeconds { get; }
        public float SecondaryDamageMultiplier { get; }
        public float DamageMultiplier { get; }

        public MineEffect(
            float triggerRadius,
            float blastRadius,
            float lifetimeSeconds,
            int maxConcurrent,
            float secondaryDelaySeconds = 0f,
            float secondaryDamageMultiplier = 0f,
            float damageMultiplier = 1f)
        {
            ProjectileBurstEffect.ValidatePositive(triggerRadius, nameof(triggerRadius));
            ProjectileBurstEffect.ValidatePositive(blastRadius, nameof(blastRadius));
            ProjectileBurstEffect.ValidatePositive(lifetimeSeconds, nameof(lifetimeSeconds));
            ProjectileBurstEffect.ValidateCount(maxConcurrent, nameof(maxConcurrent));
            ProjectileBurstEffect.ValidateNonNegative(secondaryDelaySeconds, nameof(secondaryDelaySeconds));
            ProjectileBurstEffect.ValidateNonNegative(secondaryDamageMultiplier, nameof(secondaryDamageMultiplier));
            ProjectileBurstEffect.ValidateNonNegative(damageMultiplier, nameof(damageMultiplier));
            TriggerRadius = triggerRadius;
            BlastRadius = blastRadius;
            LifetimeSeconds = lifetimeSeconds;
            MaxConcurrent = maxConcurrent;
            SecondaryDelaySeconds = secondaryDelaySeconds;
            SecondaryDamageMultiplier = secondaryDamageMultiplier;
            DamageMultiplier = damageMultiplier;
        }
    }
}
