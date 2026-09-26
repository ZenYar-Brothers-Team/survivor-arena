using System;
using Game.Content;

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

        public bool HasHeavyReplacement => HeavyEveryNth > 0;

        public SkillMechanicBonus(float projectileSpeedBonus = 0f, float returnDamageBonus = 0f, float returnSpeedBonus = 0f,
            int extraChainTargets = 0, float chainJumpRangeBonus = 0f, float chainFalloffReduction = 0f, int extraPierce = 0,
            float orbitAngularSpeedBonus = 0f, float explosionDamageBonus = 0f, float explosionRadiusBonus = 0f,
            int heavyEveryNth = 0, float heavySizeMultiplier = 0f, float heavyStopMultiplier = 0f,
            float heavyExplosionRadius = 0f, float heavyExplosionDamageMultiplier = 0f, float heavyExplosionKnockback = 0f)
        {
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
        }

        /// <summary>Additive combination; falloff reductions compose, and only one set may own a heavy replacement.</summary>
        public SkillMechanicBonus Plus(SkillMechanicBonus other)
        {
            if (HasHeavyReplacement && other.HasHeavyReplacement)
                throw new InvalidOperationException("Only one set can replace projectiles of the same skill.");
            var heavy = HasHeavyReplacement ? this : other;
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
                heavy.HeavyExplosionDamageMultiplier, heavy.HeavyExplosionKnockback);
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
            HeavyExplosionKnockback == other.HeavyExplosionKnockback;

        public override bool Equals(object obj) => obj is SkillMechanicBonus other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(ProjectileSpeedBonus, ReturnDamageBonus, ExtraChainTargets, ExtraPierce,
                OrbitAngularSpeedBonus, ExplosionDamageBonus, ExplosionRadiusBonus, HeavyEveryNth);
    }
}
