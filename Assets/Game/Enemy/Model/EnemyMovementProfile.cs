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
        /// <summary>OffsetPursuit/ArcPassPursuit: time at the end of each cycle spent directly pursuing the player.</summary>
        public float DirectPursuitSeconds { get; }
        public float BlockedTriggerSeconds { get; }
        public float BlockedProgressFraction { get; }
        public float SidestepSeconds { get; }
        public float SidestepCooldownSeconds { get; }
        public float SidestepNearDistance { get; }
        public float SidestepNearSeconds { get; }
        public float TurnResponseSeconds { get; }
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
        /// <summary>TelegraphedDash, optional (DECISION-0118): fixed dash length in world units. When positive the
        /// dash speed is length / <see cref="DashDurationSeconds"/> and <see cref="DashSpeedMultiplier"/> is unused;
        /// zero keeps the classic speed x multiplier dash.</summary>
        public float DashDistance { get; }
        /// <summary>TelegraphedDash, optional: telegraph line width in world units; zero keeps the thin default line.</summary>
        public float DashTelegraphWidth { get; }
        /// <summary>TelegraphedDash, optional: telegraph line length in world units; zero draws the whole dash length.</summary>
        public float DashTelegraphLength { get; }
        /// <summary>TelegraphedDash, optional: radius around the dashing body that shoves ordinary enemies aside and lets
        /// the dasher pass through them and the player; zero disables the shove and the pass-through.</summary>
        public float DashShoveRadius { get; }
        /// <summary>Sideways knockback given to every shoved enemy once per dash (before its knockback resistance).</summary>
        public float DashShoveDistance { get; }
        public float DashShoveSeconds { get; }
        public bool DashShoves => DashShoveRadius > 0f;
        public float DashSpeed(float movementSpeed) =>
            DashDistance > 0f ? DashDistance / DashDurationSeconds : movementSpeed * DashSpeedMultiplier;

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
            bool showDashTelegraphLine = true,
            float directPursuitSeconds = 0f,
            float blockedTriggerSeconds = 0f,
            float blockedProgressFraction = 0f,
            float sidestepSeconds = 0f,
            float sidestepCooldownSeconds = 0f,
            float turnResponseSeconds = 0f,
            float sidestepNearDistance = 0f,
            float sidestepNearSeconds = 0f,
            float dashDistance = 0f,
            float dashTelegraphWidth = 0f,
            float dashShoveRadius = 0f,
            float dashShoveDistance = 0f,
            float dashShoveSeconds = 0f,
            float dashTelegraphLength = 0f)
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
            NumericValidation.ValidateNonNegative(directPursuitSeconds, nameof(directPursuitSeconds));
            NumericValidation.ValidateNonNegative(blockedTriggerSeconds, nameof(blockedTriggerSeconds));
            NumericValidation.ValidateRange(blockedProgressFraction, 0f, 1f, nameof(blockedProgressFraction));
            NumericValidation.ValidateNonNegative(sidestepSeconds, nameof(sidestepSeconds));
            NumericValidation.ValidateNonNegative(sidestepCooldownSeconds, nameof(sidestepCooldownSeconds));
            NumericValidation.ValidateNonNegative(turnResponseSeconds, nameof(turnResponseSeconds));
            NumericValidation.ValidateNonNegative(sidestepNearDistance, nameof(sidestepNearDistance));
            NumericValidation.ValidateNonNegative(sidestepNearSeconds, nameof(sidestepNearSeconds));
            NumericValidation.ValidateNonNegative(dashDistance, nameof(dashDistance));
            NumericValidation.ValidateNonNegative(dashTelegraphWidth, nameof(dashTelegraphWidth));
            NumericValidation.ValidateNonNegative(dashShoveRadius, nameof(dashShoveRadius));
            NumericValidation.ValidateNonNegative(dashShoveDistance, nameof(dashShoveDistance));
            NumericValidation.ValidateNonNegative(dashShoveSeconds, nameof(dashShoveSeconds));
            NumericValidation.ValidateNonNegative(dashTelegraphLength, nameof(dashTelegraphLength));
            if (dashShoveRadius > 0f && (dashShoveDistance <= 0f || dashShoveSeconds <= 0f))
                throw new ArgumentOutOfRangeException(nameof(dashShoveRadius),
                    "A dash shove needs a positive knockback distance and duration.");
            if (kind == EnemyMovementKind.DistanceReposition && (repositionSeconds <= 0f || repositionSeconds >= cycleSeconds))
                throw new ArgumentOutOfRangeException(nameof(repositionSeconds), "Reposition time must be positive and shorter than the cycle.");
            if (kind == EnemyMovementKind.OffsetPursuit && (preferredDistance <= 0f || distanceTolerance <= 0f))
                throw new ArgumentOutOfRangeException(nameof(preferredDistance),
                    "Offset pursuit requires positive offset and arrival radii.");
            if (kind == EnemyMovementKind.OffsetPursuit &&
                (directPursuitSeconds <= 0f || directPursuitSeconds >= cycleSeconds))
                throw new ArgumentOutOfRangeException(nameof(directPursuitSeconds),
                    "Direct pursuit time must be positive and shorter than the offset cycle.");
            if (kind == EnemyMovementKind.BlockedSidestep &&
                (preferredDistance <= 0f || lateralStrength <= 0f || blockedTriggerSeconds <= 0f ||
                 blockedProgressFraction <= 0f || sidestepSeconds <= 0f ||
                 sidestepNearDistance <= preferredDistance || sidestepNearSeconds <= 0f))
                throw new ArgumentOutOfRangeException(nameof(kind),
                    "Blocked sidestep requires positive timing and strength, with near distance beyond the minimum activation distance.");
            if (kind == EnemyMovementKind.ArcPassPursuit &&
                (preferredDistance <= 0f || lateralStrength <= 0f || directPursuitSeconds <= 0f ||
                 directPursuitSeconds >= cycleSeconds))
                throw new ArgumentOutOfRangeException(nameof(kind),
                    "Arc pass requires a positive near distance and lateral strength, with a direct phase shorter than the cycle.");
            if (kind == EnemyMovementKind.InertialPursuit && turnResponseSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(turnResponseSeconds));

            Kind = kind;
            PreferredDistance = preferredDistance;
            DistanceTolerance = distanceTolerance;
            LateralStrength = lateralStrength;
            CycleSeconds = cycleSeconds;
            DirectPursuitSeconds = directPursuitSeconds;
            BlockedTriggerSeconds = blockedTriggerSeconds;
            BlockedProgressFraction = blockedProgressFraction;
            SidestepSeconds = sidestepSeconds;
            SidestepCooldownSeconds = sidestepCooldownSeconds;
            SidestepNearDistance = sidestepNearDistance;
            SidestepNearSeconds = sidestepNearSeconds;
            TurnResponseSeconds = turnResponseSeconds;
            DashTelegraphSeconds = dashTelegraphSeconds;
            DashDurationSeconds = dashDurationSeconds;
            DashCooldownSeconds = dashCooldownSeconds;
            DashSpeedMultiplier = dashSpeedMultiplier;
            RepositionSeconds = repositionSeconds;
            DashCount = dashCount;
            FollowUpTelegraphSeconds = followUpTelegraphSeconds;
            ShowDashTelegraphLine = showDashTelegraphLine;
            DashDistance = dashDistance;
            DashTelegraphWidth = dashTelegraphWidth;
            DashTelegraphLength = dashTelegraphLength;
            DashShoveRadius = dashShoveRadius;
            DashShoveDistance = dashShoveDistance;
            DashShoveSeconds = dashShoveSeconds;
        }
    }
}
