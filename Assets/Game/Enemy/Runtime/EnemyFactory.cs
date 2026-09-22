using System;
using Game.Pooling;
using Game.Run;
using Game.Presentation;
using UnityEngine;

namespace Game.Enemy
{
    public static class EnemyFactory
    {
        public static EnemyRuntime Spawn(
            EnemyDefinition definition,
            Vector2 position,
            Transform target,
            RunController runController,
            Transform parent = null,
            Sprite visual = null,
            GameObjectPool<EnemyRuntime> pool = null,
            GameObjectPool<EnemyProjectileRuntime> projectilePool = null,
            IEnemyLifecycleSink lifecycleSink = null,
            EnemyCategory category = EnemyCategory.Ordinary,
            SpriteMotionProfile motionProfile = null,
            SpriteContactProfile contact = null,
            EnemyDeathPresentationProfile deathPresentation = null)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (runController == null)
                throw new ArgumentNullException(nameof(runController));

            var runtime = pool != null ? pool.Rent() : CreateInstance();
            runtime.transform.SetParent(parent, worldPositionStays: false);
            runtime.transform.position = position;
            runtime.Initialize(
                definition,
                target,
                runController,
                lifecycleSink,
                visual,
                pool,
                projectilePool,
                category,
                motionProfile,
                contact,
                deathPresentation);
            return runtime;
        }

        public static EnemyRuntime CreateInstance()
        {
            var enemyObject = new GameObject("Enemy");
            enemyObject.AddComponent<Rigidbody2D>();
            enemyObject.AddComponent<CircleCollider2D>();
            enemyObject.AddComponent<SpriteRenderer>();
            enemyObject.AddComponent<LineRenderer>();
            return enemyObject.AddComponent<EnemyRuntime>();
        }
    }
}
