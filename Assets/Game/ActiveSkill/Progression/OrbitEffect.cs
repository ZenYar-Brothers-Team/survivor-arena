using Game.Content;

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
            NumericValidation.ValidateCount(bladeCount, nameof(bladeCount));
            NumericValidation.ValidatePositive(radius, nameof(radius));
            NumericValidation.ValidatePositive(angularSpeedDegrees, nameof(angularSpeedDegrees));
            NumericValidation.ValidatePositive(durationSeconds, nameof(durationSeconds));
            NumericValidation.ValidatePositive(hitCooldownSeconds, nameof(hitCooldownSeconds));
            NumericValidation.ValidateNonNegativeFinite(damageMultiplier, nameof(damageMultiplier));
            BladeCount = bladeCount;
            Radius = radius;
            AngularSpeedDegrees = angularSpeedDegrees;
            DurationSeconds = durationSeconds;
            HitCooldownSeconds = hitCooldownSeconds;
            DamageMultiplier = damageMultiplier;
        }
    }
}
