using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// Pure state for <see cref="EnemyDashVolleyProfile"/>: detects the end of a dash series from the movement phase
    /// (Dashing → anything but another dash telegraph, DECISION-0066 E4), counts series and holds delayed entries and the
    /// delayed repeat ring. Volleys come back as shots carrying their own profile; zones through <see cref="TriggeredZones"/>.
    /// Pause advances nothing; pending entries survive pause.
    /// </summary>
    public sealed class EnemyDashVolleyController
    {
        private static readonly EnemyShotCommand[] NoShots = Array.Empty<EnemyShotCommand>();
        private readonly EnemyDashVolleyProfile _profile;
        private readonly List<EnemyShotCommand> _shots = new List<EnemyShotCommand>();
        private readonly List<BossZoneProfile> _zones = new List<BossZoneProfile>();
        private readonly List<EnemyDashVolleyPending> _pending = new List<EnemyDashVolleyPending>();
        private bool _inSeries;
        private Vector2 _lastDashDirection;

        /// <summary>Completed dash series.</summary>
        public int DashCount { get; private set; }
        /// <summary>Some delayed entry or repeat has not fired yet.</summary>
        public bool RepeatPending => _pending.Count > 0;
        /// <summary>Zones started during the last <see cref="Tick"/>.</summary>
        public IReadOnlyList<BossZoneProfile> TriggeredZones => _zones;

        public EnemyDashVolleyController(EnemyDashVolleyProfile profile) =>
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));

        /// <param name="dashDirection">Direction of the current dash while dashing (movement frame); used by AwayFromDash.</param>
        public EnemyShotCommand[] Tick(EnemyMovementPhase phase, float healthFraction, float deltaTime, bool isSimulating, Vector2 aim,
            Vector2 dashDirection = default)
        {
            _zones.Clear();
            if (!isSimulating) return NoShots;
            _shots.Clear();
            for (var i = 0; i < _pending.Count; i++)
            {
                var item = _pending[i];
                item.Remaining -= deltaTime;
                if (item.Remaining > 0f) { _pending[i] = item; continue; }
                _pending.RemoveAt(i--);
                Emit(item.Entry, item.Direction, item.RotationDegrees);
            }
            if (phase == EnemyMovementPhase.Dashing)
            {
                _inSeries = true;
                if (dashDirection.sqrMagnitude > Mathf.Epsilon) _lastDashDirection = dashDirection.normalized;
            }
            else if (_inSeries && phase != EnemyMovementPhase.TelegraphingDash)
            {
                _inSeries = false;
                DashCount++;
                var toward = aim.sqrMagnitude > Mathf.Epsilon ? aim.normalized : Vector2.right;
                var replaced = _profile.ReplacementEntries.Count > 0 && healthFraction < _profile.ReplacementBelowHealthFraction;
                var entries = replaced ? _profile.ReplacementEntries : _profile.Entries;
                foreach (var entry in entries)
                {
                    var direction = entry.Orientation == EnemyDashVolleyOrientation.AwayFromDash
                        ? (_lastDashDirection.sqrMagnitude > Mathf.Epsilon ? -_lastDashDirection : -toward)
                        : toward;
                    Schedule(entry, direction, 0f, entry.DelaySeconds);
                }
                if (_profile.RepeatEveryNthDash > 0 && DashCount % _profile.RepeatEveryNthDash == 0 &&
                    healthFraction < _profile.RepeatBelowHealthFraction)
                    Schedule(_profile.Entries[0], toward, _profile.RepeatRotationDegrees, _profile.RepeatDelaySeconds);
            }
            return _shots.Count == 0 ? NoShots : _shots.ToArray();
        }

        private void Schedule(EnemyDashVolleyEntry entry, Vector2 direction, float rotation, float delay)
        {
            if (delay <= 0f) Emit(entry, direction, rotation);
            else _pending.Add(new EnemyDashVolleyPending { Remaining = delay, Entry = entry, Direction = direction, RotationDegrees = rotation });
        }

        private void Emit(EnemyDashVolleyEntry entry, Vector2 direction, float rotation)
        {
            if (entry.Zone != null)
            {
                _zones.Add(entry.Zone);
                return;
            }
            foreach (var shot in EnemyProjectilePatternGenerator.Create(entry.Attack, direction, rotation))
                _shots.Add(new EnemyShotCommand(shot.Direction, shot.IsExplosive, entry.Attack));
        }
    }
}
