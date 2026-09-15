namespace Game.ActiveSkill
{
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
}
