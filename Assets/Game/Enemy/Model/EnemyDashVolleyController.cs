using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// Pure state for <see cref="EnemyDashVolleyProfile"/>: detects the end of a dash from the movement phase, counts
    /// dashes and holds the delayed repeat ring. Pause advances nothing; a pending repeat survives pause.
    /// </summary>
    public sealed class EnemyDashVolleyController
    {
        private static readonly EnemyShotCommand[] NoShots = Array.Empty<EnemyShotCommand>();
        private readonly EnemyDashVolleyProfile _profile;
        private readonly List<EnemyShotCommand> _shots = new List<EnemyShotCommand>();
        private bool _wasDashing;
        private float _repeatRemaining = -1f;
        private float _repeatBaseDegrees;

        public int DashCount { get; private set; }
        public bool RepeatPending => _repeatRemaining >= 0f;

        public EnemyDashVolleyController(EnemyDashVolleyProfile profile) =>
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));

        public EnemyShotCommand[] Tick(EnemyMovementPhase phase, float healthFraction, float deltaTime, bool isSimulating, Vector2 aim)
        {
            if (!isSimulating) return NoShots;
            _shots.Clear();
            if (_repeatRemaining >= 0f)
            {
                _repeatRemaining -= deltaTime;
                if (_repeatRemaining <= 0f)
                {
                    _repeatRemaining = -1f;
                    AddRing(_repeatBaseDegrees + _profile.RepeatRotationDegrees);
                }
            }
            var dashing = phase == EnemyMovementPhase.Dashing;
            if (_wasDashing && !dashing)
            {
                DashCount++;
                var baseDegrees = aim.sqrMagnitude > Mathf.Epsilon ? Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg : 0f;
                AddRing(baseDegrees);
                if (_profile.RepeatEveryNthDash > 0 && DashCount % _profile.RepeatEveryNthDash == 0 &&
                    healthFraction < _profile.RepeatBelowHealthFraction)
                {
                    _repeatBaseDegrees = baseDegrees;
                    _repeatRemaining = _profile.RepeatDelaySeconds;
                    if (_repeatRemaining <= 0f) { _repeatRemaining = -1f; AddRing(baseDegrees + _profile.RepeatRotationDegrees); }
                }
            }
            _wasDashing = dashing;
            return _shots.Count == 0 ? NoShots : _shots.ToArray();
        }

        private void AddRing(float startDegrees)
        {
            var count = _profile.Attack.ProjectileCount;
            for (var i = 0; i < count; i++)
            {
                var radians = (startDegrees + 360f * i / count) * Mathf.Deg2Rad;
                _shots.Add(new EnemyShotCommand(new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)), false));
            }
        }
    }
}
