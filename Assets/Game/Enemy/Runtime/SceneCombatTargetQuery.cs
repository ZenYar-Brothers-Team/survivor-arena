using System.Collections.Generic;
using UnityEngine;

namespace Game.Enemy
{
    public sealed class SceneCombatTargetQuery : ICombatTargetQuery
    {
        public void CopyAliveTo(List<IEnemyDamageReceiver> destination, EnemyTargetCategories categories = EnemyTargetCategories.All)
            => EnemyRegistry.CopyTargetsTo(destination, categories);

        public bool TryFindNearest(Vector2 origin, out IEnemyDamageReceiver nearest, EnemyTargetCategories categories = EnemyTargetCategories.All)
            => EnemyRegistry.TryFindNearestTarget(origin, out nearest, categories);
    }
}
