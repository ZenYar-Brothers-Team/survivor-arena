using System;
using System.Collections.Generic;
using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill
{
    public static class NearestEnemyTargetSelector
    {
        public static bool TrySelect(
            Vector2 origin,
            IReadOnlyList<IEnemyDamageReceiver> candidates,
            out IEnemyDamageReceiver target)
        {
            if (candidates == null)
                throw new ArgumentNullException(nameof(candidates));

            target = null;
            var nearestSqrDistance = float.PositiveInfinity;

            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate == null || !candidate.IsAlive)
                    continue;

                var sqrDistance = (candidate.Position - origin).sqrMagnitude;
                if (sqrDistance >= nearestSqrDistance)
                    continue;

                nearestSqrDistance = sqrDistance;
                target = candidate;
            }

            return target != null;
        }
    }
}
