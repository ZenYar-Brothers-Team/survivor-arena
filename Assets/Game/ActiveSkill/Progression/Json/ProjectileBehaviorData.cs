namespace Game.ActiveSkill.Json
{
    public sealed class ProjectileBehaviorData
    {
        public float StopAfterSeconds { get; set; }
        public bool UnlimitedPierce { get; set; }
        public int RicochetCount { get; set; }
        public float RicochetRange { get; set; }
        public float? RicochetRetention { get; set; }
        public bool RepeatRicochetTargets { get; set; }
        public bool DistinctNearestTargets { get; set; }
        public float ExplosionDamageMultiplier { get; set; }
        public bool ExplodeOnExpiry { get; set; }
        public float? ExplosionKnockbackMultiplier { get; set; }

        public ProjectileBehavior ToBehavior() => new ProjectileBehavior(StopAfterSeconds, UnlimitedPierce,
            RicochetCount, RicochetRange, RicochetRetention ?? ProjectileBehavior.None.RicochetRetention,
            RepeatRicochetTargets, DistinctNearestTargets, ExplosionDamageMultiplier, ExplodeOnExpiry,
            ExplosionKnockbackMultiplier ?? ProjectileBehavior.None.ExplosionKnockbackMultiplier);
    }
}
