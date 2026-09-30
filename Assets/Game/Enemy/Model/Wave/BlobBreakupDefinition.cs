using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>Optional per-wave trial of a staggered fan for a dense ordinary-enemy group.</summary>
    public sealed class BlobBreakupDefinition
    {
        public float CheckIntervalSeconds { get; }
        public int MinimumClusterCount { get; }
        public float SelectionFraction { get; }
        public int MaxSelected { get; }
        public float ConeHalfAngleDegrees { get; }
        public float OvershootDistance { get; }
        public float StaggerSeconds { get; }
        public float ManeuverSeconds { get; }
        public IReadOnlyList<ContentId> EnemyIds { get; }

        public BlobBreakupDefinition(float checkIntervalSeconds, int minimumClusterCount,
            float selectionFraction, int maxSelected, float coneHalfAngleDegrees,
            float overshootDistance, float staggerSeconds, float maneuverSeconds,
            IReadOnlyList<ContentId> enemyIds = null)
        {
            NumericValidation.ValidatePositive(checkIntervalSeconds, nameof(checkIntervalSeconds));
            NumericValidation.ValidateCount(minimumClusterCount, nameof(minimumClusterCount));
            NumericValidation.ValidateRange(selectionFraction, 0f, 1f, nameof(selectionFraction));
            if (selectionFraction == 0f) throw new ArgumentOutOfRangeException(nameof(selectionFraction));
            NumericValidation.ValidateCount(maxSelected, nameof(maxSelected));
            NumericValidation.ValidateRange(coneHalfAngleDegrees, 25f, 80f, nameof(coneHalfAngleDegrees));
            NumericValidation.ValidatePositive(overshootDistance, nameof(overshootDistance));
            NumericValidation.ValidateNonNegative(staggerSeconds, nameof(staggerSeconds));
            NumericValidation.ValidatePositive(maneuverSeconds, nameof(maneuverSeconds));
            var copy = new ContentId[enemyIds?.Count ?? 0];
            for (var i = 0; i < copy.Length; i++)
            {
                if (!enemyIds[i].IsValid) throw new ArgumentException("Enemy filter contains an invalid ID.", nameof(enemyIds));
                copy[i] = enemyIds[i];
            }
            CheckIntervalSeconds = checkIntervalSeconds;
            MinimumClusterCount = minimumClusterCount;
            SelectionFraction = selectionFraction;
            MaxSelected = maxSelected;
            ConeHalfAngleDegrees = coneHalfAngleDegrees;
            OvershootDistance = overshootDistance;
            StaggerSeconds = staggerSeconds;
            ManeuverSeconds = maneuverSeconds;
            EnemyIds = copy;
        }

        public bool Includes(ContentId id)
        {
            if (EnemyIds.Count == 0) return true;
            for (var i = 0; i < EnemyIds.Count; i++)
                if (EnemyIds[i] == id) return true;
            return false;
        }
    }
}
