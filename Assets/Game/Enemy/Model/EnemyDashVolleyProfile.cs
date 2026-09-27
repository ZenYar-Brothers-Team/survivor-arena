using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>
    /// Actions fired when a dash series ends. BOSS-002/MIDBOSS-002 (DECISION-0063): one ring, optionally repeated every
    /// N-th series below a health fraction after a delay, rotated by a fixed angle. Later bosses (DECISION-0066, E4):
    /// several entries (volleys in any pattern, or a zone around the boss) with their own delays, and a replacement list
    /// used while health is strictly below <see cref="ReplacementBelowHealthFraction"/>. The dash telegraph is the warning.
    /// </summary>
    public sealed class EnemyDashVolleyProfile
    {
        public IReadOnlyList<EnemyDashVolleyEntry> Entries { get; }
        /// <summary>Empty = no replacement.</summary>
        public IReadOnlyList<EnemyDashVolleyEntry> ReplacementEntries { get; }
        public float ReplacementBelowHealthFraction { get; }
        /// <summary>First projectile volley of <see cref="Entries"/>; the ring of BOSS-002-style profiles.</summary>
        public EnemyAttackProfile Attack { get; }
        /// <summary>0 = never repeat.</summary>
        public int RepeatEveryNthDash { get; }
        /// <summary>Repeat only while health / max health is strictly below this value.</summary>
        public float RepeatBelowHealthFraction { get; }
        public float RepeatDelaySeconds { get; }
        public float RepeatRotationDegrees { get; }

        /// <summary>One ring on dash end with an optional repeat (BOSS-002/MIDBOSS-002, DECISION-0063).</summary>
        public EnemyDashVolleyProfile(EnemyAttackProfile attack, int repeatEveryNthDash = 0, float repeatBelowHealthFraction = 0f,
            float repeatDelaySeconds = 0f, float repeatRotationDegrees = 0f)
            : this(new[] { new EnemyDashVolleyEntry(0f, EnemyDashVolleyOrientation.TowardPlayer,
                attack ?? throw new ArgumentNullException(nameof(attack))) })
        {
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

        /// <summary>Several dash-end entries with an optional replacement below a health fraction (DECISION-0066, E4).</summary>
        public EnemyDashVolleyProfile(IEnumerable<EnemyDashVolleyEntry> entries, float replacementBelowHealthFraction = 0f,
            IEnumerable<EnemyDashVolleyEntry> replacementEntries = null)
        {
            var copy = new List<EnemyDashVolleyEntry>(entries ?? throw new ArgumentNullException(nameof(entries)));
            var replacement = new List<EnemyDashVolleyEntry>(replacementEntries ?? Array.Empty<EnemyDashVolleyEntry>());
            if (copy.Count == 0 || copy.Contains(null) || replacement.Contains(null))
                throw new ArgumentException("Dash-end entries are required and cannot be null.", nameof(entries));
            NumericValidation.ValidateRange(replacementBelowHealthFraction, 0f, 1f, nameof(replacementBelowHealthFraction));
            if ((replacement.Count > 0) != (replacementBelowHealthFraction > 0f))
                throw new ArgumentException("A replacement needs both entries and a health threshold.", nameof(replacementEntries));
            Entries = copy.AsReadOnly();
            ReplacementEntries = replacement.AsReadOnly();
            ReplacementBelowHealthFraction = replacementBelowHealthFraction;
            Attack = copy.Concat(replacement).Select(entry => entry.Attack).FirstOrDefault(attack => attack != null);
        }

        /// <summary>Every projectile profile this volley can fire.</summary>
        public IEnumerable<EnemyAttackProfile> Attacks =>
            Entries.Concat(ReplacementEntries).Select(entry => entry.Attack).Where(attack => attack != null);

        /// <summary>Same timing and zones with every volley profile replaced (field damage modifiers).</summary>
        public EnemyDashVolleyProfile MapAttacks(Func<EnemyAttackProfile, EnemyAttackProfile> map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (RepeatEveryNthDash > 0 || IsSingleRing)
                return new EnemyDashVolleyProfile(map(Attack), RepeatEveryNthDash, RepeatBelowHealthFraction, RepeatDelaySeconds,
                    RepeatRotationDegrees);
            EnemyDashVolleyEntry Map(EnemyDashVolleyEntry entry) => entry.Attack == null ? entry
                : new EnemyDashVolleyEntry(entry.DelaySeconds, entry.Orientation, map(entry.Attack));
            return new EnemyDashVolleyProfile(Entries.Select(Map), ReplacementBelowHealthFraction, ReplacementEntries.Select(Map));
        }

        private bool IsSingleRing => Entries.Count == 1 && ReplacementEntries.Count == 0 && Entries[0].Attack != null &&
                                     Entries[0].DelaySeconds == 0f && Entries[0].Orientation == EnemyDashVolleyOrientation.TowardPlayer &&
                                     Entries[0].Attack.Pattern == EnemyProjectilePattern.Ring;
    }
}
