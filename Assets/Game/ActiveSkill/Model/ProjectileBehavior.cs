using Game.Content;

namespace Game.ActiveSkill
{
    /// <summary>Optional projectile mechanics; zero/false values disable a mechanic, never supply tuning.</summary>
    public sealed class ProjectileBehavior
    {
        public static ProjectileBehavior None { get; } = new ProjectileBehavior();
        public float StopAfterSeconds { get; }
        public bool UnlimitedPierce { get; }
        public int RicochetCount { get; }
        public float RicochetRange { get; }
        public float RicochetRetention { get; }
        public bool RepeatRicochetTargets { get; }
        public bool DistinctNearestTargets { get; }
        public float ExplosionDamageMultiplier { get; }
        public bool ExplodeOnExpiry { get; }
        public float ExplosionKnockbackMultiplier { get; }

        public ProjectileBehavior(float stopAfterSeconds = 0f, bool unlimitedPierce = false,
            int ricochetCount = 0, float ricochetRange = 0f, float ricochetRetention = 1f,
            bool repeatRicochetTargets = false, bool distinctNearestTargets = false,
            float explosionDamageMultiplier = 0f, bool explodeOnExpiry = false, float explosionKnockbackMultiplier = 1f)
        {
            NumericValidation.ValidateNonNegative(stopAfterSeconds, nameof(stopAfterSeconds));
            NumericValidation.ValidateNonNegative(ricochetCount, nameof(ricochetCount));
            NumericValidation.ValidateNonNegative(ricochetRange, nameof(ricochetRange));
            NumericValidation.ValidateNonNegative(ricochetRetention, nameof(ricochetRetention));
            NumericValidation.ValidateNonNegative(explosionDamageMultiplier, nameof(explosionDamageMultiplier));
            NumericValidation.ValidateNonNegative(explosionKnockbackMultiplier, nameof(explosionKnockbackMultiplier));
            if (ricochetCount > 0) NumericValidation.ValidatePositive(ricochetRange, nameof(ricochetRange));
            StopAfterSeconds = stopAfterSeconds;
            UnlimitedPierce = unlimitedPierce;
            RicochetCount = ricochetCount;
            RicochetRange = ricochetRange;
            RicochetRetention = ricochetRetention;
            RepeatRicochetTargets = repeatRicochetTargets;
            DistinctNearestTargets = distinctNearestTargets;
            ExplosionDamageMultiplier = explosionDamageMultiplier;
            ExplodeOnExpiry = explodeOnExpiry;
            ExplosionKnockbackMultiplier = explosionKnockbackMultiplier;
        }
    }
}
