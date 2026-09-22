using System.Collections.Generic;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>Scene-independent hostile target discovery; no concrete enemy component requirement.</summary>
    public interface ICombatTargetQuery
    {
        void CopyAliveTo(List<IEnemyDamageReceiver> destination, EnemyTargetCategories categories = EnemyTargetCategories.All);
        bool TryFindNearest(Vector2 origin, out IEnemyDamageReceiver nearest, EnemyTargetCategories categories = EnemyTargetCategories.All);
    }
}
