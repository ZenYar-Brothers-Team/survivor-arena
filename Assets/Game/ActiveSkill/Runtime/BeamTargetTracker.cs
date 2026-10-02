using System.Collections.Generic;
using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill
{
    /// <summary>
    /// Shared aim state of one beam activation (damage ticks and its visual): origin follows the owner, direction
    /// follows the current target, and when that target dies the beam picks the nearest other visible enemy
    /// (DECISION-0144). With no enemy at all it keeps its last direction.
    /// </summary>
    internal sealed class BeamTargetTracker
    {
        private readonly ICombatTargetQuery _selection;
        private readonly List<IEnemyDamageReceiver> _buffer = new List<IEnemyDamageReceiver>();
        private EnemyTargetLife _target;
        private Vector2 _direction;

        /// <param name="rank">0 = the activation's nearest target; n = the n-th nearest visible enemy (extra beams, DECISION-0148).</param>
        public BeamTargetTracker(ActiveSkillActivation activation, ICombatTargetQuery selection, int rank = 0)
        {
            _selection = selection;
            _target = activation.TargetLife;
            _direction = activation.AimDirection;
            if (rank > 0 && selection != null && activation.OwnerTransform != null)
                SelectRanked((Vector2)activation.OwnerTransform.position, rank);
        }

        private void SelectRanked(Vector2 origin, int rank)
        {
            _selection.CopyAliveTo(_buffer);
            _buffer.Sort((a, b) => (a.Position - origin).sqrMagnitude.CompareTo((b.Position - origin).sqrMagnitude));
            if (_buffer.Count == 0) return;
            var picked = _buffer[Mathf.Min(rank, _buffer.Count - 1)];
            _target = new EnemyTargetLife(picked);
            var toTarget = picked.Position - origin;
            if (toTarget.sqrMagnitude > Mathf.Epsilon) _direction = toTarget.normalized;
        }

        public void Resolve(ActiveSkillActivation activation, BeamEffect effect, out Vector2 origin, out Vector2 direction)
        {
            origin = activation.OwnerTransform != null ? (Vector2)activation.OwnerTransform.position : activation.Origin;
            if (effect.TracksTarget)
            {
                if (!_target.IsAlive) Retarget(origin);
                if (_target.IsAlive)
                {
                    var toTarget = _target.Target.Position - origin;
                    if (toTarget.sqrMagnitude > Mathf.Epsilon) _direction = toTarget.normalized;
                }
            }
            direction = _direction;
        }

        private void Retarget(Vector2 origin)
        {
            if (_selection == null) return;
            _selection.CopyAliveTo(_buffer);
            IEnemyDamageReceiver nearest = null;
            var best = float.MaxValue;
            for (var i = 0; i < _buffer.Count; i++)
            {
                var distance = (_buffer[i].Position - origin).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                nearest = _buffer[i];
            }
            if (nearest != null) _target = new EnemyTargetLife(nearest);
        }
    }
}
