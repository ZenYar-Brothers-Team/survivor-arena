using System;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>
    /// One dash-end action (DECISION-0066, E4): a projectile volley aimed by <see cref="Orientation"/>, or a ground zone
    /// around the boss, <see cref="DelaySeconds"/> after the dash series ends.
    /// </summary>
    public sealed class EnemyDashVolleyEntry
    {
        public float DelaySeconds { get; }
        public EnemyDashVolleyOrientation Orientation { get; }
        public EnemyAttackProfile Attack { get; }
        public BossZoneProfile Zone { get; }

        public EnemyDashVolleyEntry(float delaySeconds, EnemyDashVolleyOrientation orientation, EnemyAttackProfile attack = null,
            BossZoneProfile zone = null)
        {
            NumericValidation.ValidateNonNegativeFinite(delaySeconds, nameof(delaySeconds));
            if (!Enum.IsDefined(typeof(EnemyDashVolleyOrientation), orientation)) throw new ArgumentOutOfRangeException(nameof(orientation));
            if ((attack == null) == (zone == null)) throw new ArgumentException("A dash-end entry is a volley or a zone.");
            if (zone != null && (orientation != EnemyDashVolleyOrientation.Self || zone.Placement != BossZonePlacement.AroundSelf))
                throw new ArgumentException("A dash-end zone is centered on the boss.", nameof(zone));
            if (attack != null && orientation == EnemyDashVolleyOrientation.Self)
                throw new ArgumentException("A dash-end volley needs a direction.", nameof(orientation));
            DelaySeconds = delaySeconds;
            Orientation = orientation;
            Attack = attack;
            Zone = zone;
        }
    }
}
