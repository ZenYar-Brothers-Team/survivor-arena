using System;
using Game.Combat;
using Game.Run;
using UnityEngine;

namespace Game.Enemy
{
    public static class EnemyProjectileDamage
    {
        public static float Apply(
            EnemyAttackProfile profile,
            Vector2 impactPosition,
            Vector2 targetPosition,
            Health target,
            RunState runState)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (runState != RunState.Running)
                return 0f;
            if (profile.Pattern == EnemyProjectilePattern.Explosive &&
                Vector2.Distance(impactPosition, targetPosition) > profile.ExplosionRadius)
                return 0f;
            return target.TakeDamage(profile.Damage);
        }
    }
}
