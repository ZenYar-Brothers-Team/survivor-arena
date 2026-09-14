using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill
{
    public sealed class SceneEnemyTargetProvider : IActiveSkillTargetProvider
    {
        public bool TryGetTarget(Vector2 origin, out IEnemyDamageReceiver target)
        {
            var found = EnemyRegistry.TryFindNearest(origin, out var enemy);
            target = enemy;
            return found;
        }
    }
}
