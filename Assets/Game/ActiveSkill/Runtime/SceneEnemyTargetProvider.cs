using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill
{
    public sealed class SceneEnemyTargetProvider : IActiveSkillTargetProvider
    {
        private readonly ICombatTargetQuery _targets;
        public SceneEnemyTargetProvider(ICombatTargetQuery targets = null) { _targets = targets ?? new SceneCombatTargetQuery(); }

        public bool TryGetTarget(Vector2 origin, out IEnemyDamageReceiver target)
        {
            return _targets.TryFindNearest(origin, out target);
        }
    }
}
