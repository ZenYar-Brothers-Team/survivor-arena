using System.Collections.Generic;
using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    public sealed class SkillTestTargetProvider : IActiveSkillTargetSetProvider
    {
        private readonly IEnemyDamageReceiver[] _targets;
        public SkillTestTargetProvider(params IEnemyDamageReceiver[] targets) { _targets = targets; }
        public void CopyAliveTo(List<IEnemyDamageReceiver> destination) { destination.Clear(); destination.AddRange(_targets); }
        public bool TryGetTarget(Vector2 origin, out IEnemyDamageReceiver target)
        {
            target = null;
            var nearest = float.PositiveInfinity;
            foreach (var candidate in _targets)
            {
                var distance = (candidate.Position - origin).sqrMagnitude;
                if (candidate.IsAlive && distance < nearest) { target = candidate; nearest = distance; }
            }
            return target != null;
        }
    }
}
