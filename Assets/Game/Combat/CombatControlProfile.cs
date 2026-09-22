using Game.Content;

namespace Game.Combat
{
    /// <summary>Authoring values; zero controls are neutral, positive controls require explicit duration.</summary>
    public sealed class CombatControlProfile
    {
        public static CombatControlProfile None { get; } = new CombatControlProfile();
        public float KnockbackDistance { get; }
        public float KnockbackSeconds { get; }
        public float SlowFraction { get; }
        public float SlowSeconds { get; }
        public string Channel { get; }
        public CombatControlProfile(float knockbackDistance = 0f, float knockbackSeconds = 0f,
            float slowFraction = 0f, float slowSeconds = 0f, string channel = "primary")
        {
            NumericValidation.ValidateNonNegative(knockbackDistance, nameof(knockbackDistance));
            NumericValidation.ValidateNonNegative(knockbackSeconds, nameof(knockbackSeconds));
            NumericValidation.ValidateRange(slowFraction, 0f, 1f, nameof(slowFraction));
            NumericValidation.ValidateNonNegative(slowSeconds, nameof(slowSeconds));
            if (knockbackDistance > 0f) NumericValidation.ValidatePositive(knockbackSeconds, nameof(knockbackSeconds));
            if (slowFraction > 0f) NumericValidation.ValidatePositive(slowSeconds, nameof(slowSeconds));
            if (string.IsNullOrWhiteSpace(channel)) throw new System.ArgumentException("Control channel is required.", nameof(channel));
            KnockbackDistance = knockbackDistance;
            KnockbackSeconds = knockbackSeconds;
            SlowFraction = slowFraction;
            SlowSeconds = slowSeconds;
            Channel = channel;
        }
    }
}
