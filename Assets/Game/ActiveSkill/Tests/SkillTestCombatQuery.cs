using System.Collections.Generic;
using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    public sealed class SkillTestCombatQuery : ICombatTargetQuery
    {
        private readonly IEnemyDamageReceiver[] _targets;
        public SkillTestCombatQuery(params IEnemyDamageReceiver[] targets) { _targets = targets; }

        public void CopyAliveTo(List<IEnemyDamageReceiver> destination, EnemyTargetCategories categories = EnemyTargetCategories.All)
        {
            destination.Clear();
            foreach (var target in _targets) if (target.IsAlive) destination.Add(target);
        }

        public bool TryFindNearest(Vector2 origin, out IEnemyDamageReceiver nearest, EnemyTargetCategories categories = EnemyTargetCategories.All)
        {
            nearest = null;
            var best = float.PositiveInfinity;
            foreach (var target in _targets)
            {
                var distance = (target.Position - origin).sqrMagnitude;
                if (target.IsAlive && distance < best) { nearest = target; best = distance; }
            }
            return nearest != null;
        }
    }
}
