namespace Game.ActiveSkill
{
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
}
