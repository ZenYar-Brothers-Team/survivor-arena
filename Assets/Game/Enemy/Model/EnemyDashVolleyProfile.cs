using System;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>
    /// A ring fired the moment a dash ends (BOSS-002, MIDBOSS-002; DECISION-0063). Optionally every N-th dash below a
    /// health fraction adds one more ring after a delay, rotated by a fixed angle. The dash telegraph is the warning.
    /// </summary>
    public sealed class EnemyDashVolleyProfile
    {
        public EnemyAttackProfile Attack { get; }
        /// <summary>0 = never repeat.</summary>
        public int RepeatEveryNthDash { get; }
        /// <summary>Repeat only while health / max health is strictly below this value.</summary>
        public float RepeatBelowHealthFraction { get; }
        public float RepeatDelaySeconds { get; }
        public float RepeatRotationDegrees { get; }

        public EnemyDashVolleyProfile(EnemyAttackProfile attack, int repeatEveryNthDash = 0, float repeatBelowHealthFraction = 0f,
            float repeatDelaySeconds = 0f, float repeatRotationDegrees = 0f)
        {
            Attack = attack ?? throw new ArgumentNullException(nameof(attack));
            if (attack.Pattern != EnemyProjectilePattern.Ring)
                throw new ArgumentException("A dash volley is a ring.", nameof(attack));
            NumericValidation.ValidateNonNegative(repeatEveryNthDash, nameof(repeatEveryNthDash));
            NumericValidation.ValidateRange(repeatBelowHealthFraction, 0f, 1f, nameof(repeatBelowHealthFraction));
            NumericValidation.ValidateNonNegativeFinite(repeatDelaySeconds, nameof(repeatDelaySeconds));
            NumericValidation.ValidateFinite(repeatRotationDegrees, nameof(repeatRotationDegrees));
            if (repeatEveryNthDash > 0 && repeatBelowHealthFraction <= 0f)
                throw new ArgumentException("A repeat needs a health threshold.", nameof(repeatBelowHealthFraction));
            RepeatEveryNthDash = repeatEveryNthDash;
            RepeatBelowHealthFraction = repeatBelowHealthFraction;
            RepeatDelaySeconds = repeatDelaySeconds;
            RepeatRotationDegrees = repeatRotationDegrees;
        }
    }
}
