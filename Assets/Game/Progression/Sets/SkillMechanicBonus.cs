using System;
using Game.Content;
using UnityEngine;

namespace Game.Progression
{
    /// <summary>
    /// Skill-specific set bonuses that are not character stat channels (sets-v1, DECISION-0061). Every field is
    /// neutral at zero; the executor applies them once from the activation snapshot. Bonuses from several sets on
    /// the same skill add up (<see cref="Plus"/>); no set-to-set multiplication.
    /// </summary>
    public readonly struct SkillMechanicBonus : IEquatable<SkillMechanicBonus>
    {
        /// <summary>Projectile speed; lifetime shrinks by the same factor so travel distance is unchanged.</summary>
        public float ProjectileSpeedBonus { get; }
        /// <summary>Boomerang return pass and ricochet-disc rebounds after the first hit (G-04).</summary>
        public float ReturnDamageBonus { get; }
        public float ReturnSpeedBonus { get; }
        public int ExtraChainTargets { get; }
        public float ChainJumpRangeBonus { get; }
        /// <summary>Share of the per-jump damage falloff removed, 0…1 (0.5 halves the falloff).</summary>
        public float ChainFalloffReduction { get; }
        public int ExtraPierce { get; }
        public float OrbitAngularSpeedBonus { get; }
        /// <summary>Only the explosion part: mine blasts and sphere explosions, not impact hits.</summary>
        public float ExplosionDamageBonus { get; }
        public float ExplosionRadiusBonus { get; }
        /// <summary>Every N-th projectile becomes a heavy one that explodes when it stops; 0 = no replacement (SET-008).</summary>
        public int HeavyEveryNth { get; }
        public float HeavySizeMultiplier { get; }
        public float HeavyStopMultiplier { get; }
        public float HeavyExplosionRadius { get; }
        public float HeavyExplosionDamageMultiplier { get; }
        public float HeavyExplosionKnockback { get; }

        /// <summary>Extra projectiles added to a burst/fan of the skill (SET-034).</summary>
        public int ExtraProjectiles { get; }
        /// <summary>Absolute slow-fraction points added to a skill's existing slow, capped at 1 (SET-028).</summary>
        public float SlowStrengthBonus { get; }
        /// <summary>Slow the set gives a skill that may have none (SET-004 wave, DECISION-0139); the stronger slow wins.</summary>
        public float GrantedSlowFraction { get; }
        public float GrantedSlowSeconds { get; }

        public bool HasHeavyReplacement => HeavyEveryNth > 0;

        public SkillMechanicBonus(float projectileSpeedBonus = 0f, float returnDamageBonus = 0f, float returnSpeedBonus = 0f,
            int extraChainTargets = 0, float chainJumpRangeBonus = 0f, float chainFalloffReduction = 0f, int extraPierce = 0,
            float orbitAngularSpeedBonus = 0f, float explosionDamageBonus = 0f, float explosionRadiusBonus = 0f,
            int heavyEveryNth = 0, float heavySizeMultiplier = 0f, float heavyStopMultiplier = 0f,
            float heavyExplosionRadius = 0f, float heavyExplosionDamageMultiplier = 0f, float heavyExplosionKnockback = 0f,
            int extraProjectiles = 0, float slowStrengthBonus = 0f, float grantedSlowFraction = 0f, float grantedSlowSeconds = 0f)
        {
            NumericValidation.ValidateRange(grantedSlowFraction, 0f, 1f, nameof(grantedSlowFraction));
            NumericValidation.ValidateNonNegative(grantedSlowSeconds, nameof(grantedSlowSeconds));
            if (grantedSlowFraction > 0f) NumericValidation.ValidatePositive(grantedSlowSeconds, nameof(grantedSlowSeconds));
            NumericValidation.ValidateNonNegative(extraProjectiles, nameof(extraProjectiles));
            NumericValidation.ValidateRange(slowStrengthBonus, 0f, 1f, nameof(slowStrengthBonus));
            NumericValidation.ValidateNonNegative(projectileSpeedBonus, nameof(projectileSpeedBonus));
            NumericValidation.ValidateNonNegative(returnDamageBonus, nameof(returnDamageBonus));
            NumericValidation.ValidateNonNegative(returnSpeedBonus, nameof(returnSpeedBonus));
            NumericValidation.ValidateNonNegative(extraChainTargets, nameof(extraChainTargets));
            NumericValidation.ValidateNonNegative(chainJumpRangeBonus, nameof(chainJumpRangeBonus));
            NumericValidation.ValidateRange(chainFalloffReduction, 0f, 1f, nameof(chainFalloffReduction));
            NumericValidation.ValidateNonNegative(extraPierce, nameof(extraPierce));
            NumericValidation.ValidateNonNegative(orbitAngularSpeedBonus, nameof(orbitAngularSpeedBonus));
            NumericValidation.ValidateNonNegative(explosionDamageBonus, nameof(explosionDamageBonus));
            NumericValidation.ValidateNonNegative(explosionRadiusBonus, nameof(explosionRadiusBonus));
            NumericValidation.ValidateNonNegative(heavyEveryNth, nameof(heavyEveryNth));
            if (heavyEveryNth > 0)
            {
                NumericValidation.ValidatePositive(heavySizeMultiplier, nameof(heavySizeMultiplier));
                NumericValidation.ValidatePositive(heavyStopMultiplier, nameof(heavyStopMultiplier));
                NumericValidation.ValidatePositive(heavyExplosionRadius, nameof(heavyExplosionRadius));
                NumericValidation.ValidatePositive(heavyExplosionDamageMultiplier, nameof(heavyExplosionDamageMultiplier));
                NumericValidation.ValidateNonNegative(heavyExplosionKnockback, nameof(heavyExplosionKnockback));
            }
            else if (heavySizeMultiplier != 0f || heavyStopMultiplier != 0f || heavyExplosionRadius != 0f ||
                     heavyExplosionDamageMultiplier != 0f || heavyExplosionKnockback != 0f)
                throw new ArgumentException("Heavy projectile parameters require heavyEveryNth.");

            ProjectileSpeedBonus = projectileSpeedBonus;
            ReturnDamageBonus = returnDamageBonus;
            ReturnSpeedBonus = returnSpeedBonus;
            ExtraChainTargets = extraChainTargets;
            ChainJumpRangeBonus = chainJumpRangeBonus;
            ChainFalloffReduction = chainFalloffReduction;
            ExtraPierce = extraPierce;
            OrbitAngularSpeedBonus = orbitAngularSpeedBonus;
            ExplosionDamageBonus = explosionDamageBonus;
            ExplosionRadiusBonus = explosionRadiusBonus;
            HeavyEveryNth = heavyEveryNth;
            HeavySizeMultiplier = heavySizeMultiplier;
            HeavyStopMultiplier = heavyStopMultiplier;
            HeavyExplosionRadius = heavyExplosionRadius;
            HeavyExplosionDamageMultiplier = heavyExplosionDamageMultiplier;
            HeavyExplosionKnockback = heavyExplosionKnockback;
            ExtraProjectiles = extraProjectiles;
            SlowStrengthBonus = slowStrengthBonus;
            GrantedSlowFraction = grantedSlowFraction;
            GrantedSlowSeconds = grantedSlowSeconds;
        }

        /// <summary>Additive combination; falloff reductions compose, and only one set may own a heavy replacement.</summary>
        public SkillMechanicBonus Plus(SkillMechanicBonus other)
        {
            if (HasHeavyReplacement && other.HasHeavyReplacement)
                throw new InvalidOperationException("Only one set can replace projectiles of the same skill.");
            var heavy = HasHeavyReplacement ? this : other;
            // Granted slows (DECISION-0139) do not merge: the stronger slow keeps its own duration; equal strength keeps the longer.
            var granted = other.GrantedSlowFraction > GrantedSlowFraction ||
                          (other.GrantedSlowFraction == GrantedSlowFraction && other.GrantedSlowSeconds > GrantedSlowSeconds) ? other : this;
            return new SkillMechanicBonus(
                ProjectileSpeedBonus + other.ProjectileSpeedBonus,
                ReturnDamageBonus + other.ReturnDamageBonus,
                ReturnSpeedBonus + other.ReturnSpeedBonus,
                ExtraChainTargets + other.ExtraChainTargets,
                ChainJumpRangeBonus + other.ChainJumpRangeBonus,
                1f - (1f - ChainFalloffReduction) * (1f - other.ChainFalloffReduction),
                ExtraPierce + other.ExtraPierce,
                OrbitAngularSpeedBonus + other.OrbitAngularSpeedBonus,
                ExplosionDamageBonus + other.ExplosionDamageBonus,
                ExplosionRadiusBonus + other.ExplosionRadiusBonus,
                heavy.HeavyEveryNth, heavy.HeavySizeMultiplier, heavy.HeavyStopMultiplier, heavy.HeavyExplosionRadius,
                heavy.HeavyExplosionDamageMultiplier, heavy.HeavyExplosionKnockback,
                ExtraProjectiles + other.ExtraProjectiles, Mathf.Min(1f, SlowStrengthBonus + other.SlowStrengthBonus),
                granted.GrantedSlowFraction, granted.GrantedSlowSeconds);
        }

        public bool Equals(SkillMechanicBonus other) =>
            ProjectileSpeedBonus == other.ProjectileSpeedBonus && ReturnDamageBonus == other.ReturnDamageBonus &&
            ReturnSpeedBonus == other.ReturnSpeedBonus && ExtraChainTargets == other.ExtraChainTargets &&
            ChainJumpRangeBonus == other.ChainJumpRangeBonus && ChainFalloffReduction == other.ChainFalloffReduction &&
            ExtraPierce == other.ExtraPierce && OrbitAngularSpeedBonus == other.OrbitAngularSpeedBonus &&
            ExplosionDamageBonus == other.ExplosionDamageBonus && ExplosionRadiusBonus == other.ExplosionRadiusBonus &&
            HeavyEveryNth == other.HeavyEveryNth && HeavySizeMultiplier == other.HeavySizeMultiplier &&
            HeavyStopMultiplier == other.HeavyStopMultiplier && HeavyExplosionRadius == other.HeavyExplosionRadius &&
            HeavyExplosionDamageMultiplier == other.HeavyExplosionDamageMultiplier &&
            HeavyExplosionKnockback == other.HeavyExplosionKnockback &&
            ExtraProjectiles == other.ExtraProjectiles && SlowStrengthBonus == other.SlowStrengthBonus &&
            GrantedSlowFraction == other.GrantedSlowFraction && GrantedSlowSeconds == other.GrantedSlowSeconds;

        public override bool Equals(object obj) => obj is SkillMechanicBonus other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(ProjectileSpeedBonus, ReturnDamageBonus, ExtraChainTargets, ExtraPierce,
                OrbitAngularSpeedBonus, ExplosionDamageBonus, ExplosionRadiusBonus, HeavyEveryNth);
    }
}
