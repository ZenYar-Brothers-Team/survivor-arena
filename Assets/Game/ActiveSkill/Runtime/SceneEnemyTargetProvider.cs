using Game.Enemy;
using System.Collections.Generic;
using UnityEngine;

namespace Game.ActiveSkill
{
    public sealed class SceneEnemyTargetProvider : IActiveSkillTargetSetProvider
    {
        private readonly ICombatTargetQuery _targets;
        public SceneEnemyTargetProvider(ICombatTargetQuery targets = null) { _targets = targets ?? new SceneCombatTargetQuery(); }

        public void CopyAliveTo(List<IEnemyDamageReceiver> destination) => _targets.CopyAliveTo(destination);

        public bool TryGetTarget(Vector2 origin, out IEnemyDamageReceiver target)
        {
            return _targets.TryFindNearest(origin, out target);
        }
    }
}
