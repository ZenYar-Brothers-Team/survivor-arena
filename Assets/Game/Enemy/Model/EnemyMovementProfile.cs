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
        /// <summary>DistanceReposition: seconds at the end of each cycle spent moving sideways (the rest holds distance).</summary>
        public float RepositionSeconds { get; }
        /// <summary>TelegraphedDash: dashes per sequence (MIDBOSS-001: 2), each with its own direction snapshot.</summary>
        public int DashCount { get; }
        /// <summary>Telegraph before the 2nd…Nth dash of a sequence.</summary>
        public float FollowUpTelegraphSeconds { get; }
        /// <summary>TelegraphedDash: draw the aim line during the dash telegraph. The telegraph pause
        /// itself stays; ENEMY-007 hides the line (DECISION-0057, playtest 2026-09-25_5233a664 OBS-05).</summary>
        public bool ShowDashTelegraphLine { get; }

        public EnemyMovementProfile(
            EnemyMovementKind kind,
            float preferredDistance = 0f,
            float distanceTolerance = 0f,
            float lateralStrength = 1f,
            float cycleSeconds = 1f,
            float dashTelegraphSeconds = 0.5f,
            float dashDurationSeconds = 0.4f,
            float dashCooldownSeconds = 3f,
            float dashSpeedMultiplier = 3f,
            float repositionSeconds = 0f,
            int dashCount = 1,
            float followUpTelegraphSeconds = 0f,
            bool showDashTelegraphLine = true)
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
            NumericValidation.ValidateNonNegative(repositionSeconds, nameof(repositionSeconds));
            NumericValidation.ValidateCount(dashCount, nameof(dashCount));
            NumericValidation.ValidateNonNegative(followUpTelegraphSeconds, nameof(followUpTelegraphSeconds));
            if (kind == EnemyMovementKind.DistanceReposition && (repositionSeconds <= 0f || repositionSeconds >= cycleSeconds))
                throw new ArgumentOutOfRangeException(nameof(repositionSeconds), "Reposition time must be positive and shorter than the cycle.");

            Kind = kind;
            PreferredDistance = preferredDistance;
            DistanceTolerance = distanceTolerance;
            LateralStrength = lateralStrength;
            CycleSeconds = cycleSeconds;
            DashTelegraphSeconds = dashTelegraphSeconds;
            DashDurationSeconds = dashDurationSeconds;
            DashCooldownSeconds = dashCooldownSeconds;
            DashSpeedMultiplier = dashSpeedMultiplier;
            RepositionSeconds = repositionSeconds;
            DashCount = dashCount;
            FollowUpTelegraphSeconds = followUpTelegraphSeconds;
            ShowDashTelegraphLine = showDashTelegraphLine;
        }
    }
}
