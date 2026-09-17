using System;
using Game.Character;
using Game.Pooling;
using Game.Run;
using UnityEngine;

namespace Game.Enemy
{
    public static class EnemyProjectileFactory
    {
        public static EnemyProjectileRuntime Spawn(
            EnemyAttackProfile profile,
            Vector2 position,
            Vector2 direction,
            PlayerCharacterRuntime target,
            RunController runController,
            Transform parent = null,
            GameObjectPool<EnemyProjectileRuntime> pool = null)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));
            if (runController == null)
                throw new ArgumentNullException(nameof(runController));

            var runtime = pool != null ? pool.Rent() : CreateInstance();
            runtime.transform.SetParent(parent, false);
            runtime.transform.position = position;
            runtime.Initialize(profile, direction, target, runController, pool);
            return runtime;
        }

        public static EnemyProjectileRuntime CreateInstance()
        {
            var projectile = new GameObject("Enemy Projectile");
            projectile.AddComponent<Rigidbody2D>();
            projectile.AddComponent<CircleCollider2D>();
            projectile.AddComponent<SpriteRenderer>();
            return projectile.AddComponent<EnemyProjectileRuntime>();
        }
    }
}
