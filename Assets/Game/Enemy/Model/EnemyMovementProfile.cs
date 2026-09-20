using System;
using Game.Content;

namespace Game.Enemy
{
    public sealed class EnemyMovementProfile
    {
        public static EnemyMovementProfile Seek { get; } = new EnemyMovementProfile(EnemyMovementKind.Seek);

        public EnemyMovementKind Kind { get; }
        public float PreferredDistance { get; }
        public float DistanceTolerance { get; }
        public float LateralStrength { get; }
        public float CycleSeconds { get; }
        public float DashTelegraphSeconds { get; }
        public float DashDurationSeconds { get; }
        public float DashCooldownSeconds { get; }
        public float DashSpeedMultiplier { get; }

        public EnemyMovementProfile(
            EnemyMovementKind kind,
            float preferredDistance = 0f,
            float distanceTolerance = 0f,
            float lateralStrength = 1f,
            float cycleSeconds = 1f,
            float dashTelegraphSeconds = 0.5f,
            float dashDurationSeconds = 0.4f,
            float dashCooldownSeconds = 3f,
            float dashSpeedMultiplier = 3f)
        {
            if (!Enum.IsDefined(typeof(EnemyMovementKind), kind))
                throw new ArgumentOutOfRangeException(nameof(kind));
            NumericValidation.ValidateNonNegative(preferredDistance, nameof(preferredDistance));
            NumericValidation.ValidateNonNegative(distanceTolerance, nameof(distanceTolerance));
            NumericValidation.ValidateNonNegative(lateralStrength, nameof(lateralStrength));
            NumericValidation.ValidatePositive(cycleSeconds, nameof(cycleSeconds));
            NumericValidation.ValidateNonNegative(dashTelegraphSeconds, nameof(dashTelegraphSeconds));
            NumericValidation.ValidatePositive(dashDurationSeconds, nameof(dashDurationSeconds));
            NumericValidation.ValidatePositive(dashCooldownSeconds, nameof(dashCooldownSeconds));
            NumericValidation.ValidatePositive(dashSpeedMultiplier, nameof(dashSpeedMultiplier));

            Kind = kind;
            PreferredDistance = preferredDistance;
            DistanceTolerance = distanceTolerance;
            LateralStrength = lateralStrength;
            CycleSeconds = cycleSeconds;
            DashTelegraphSeconds = dashTelegraphSeconds;
            DashDurationSeconds = dashDurationSeconds;
            DashCooldownSeconds = dashCooldownSeconds;
            DashSpeedMultiplier = dashSpeedMultiplier;
        }
    }
}
