using System;
using Game.Run;
using Game.Progression;
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
            Sprite visual = null)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (runController == null)
                throw new ArgumentNullException(nameof(runController));

            var enemyObject = new GameObject("Enemy");
            enemyObject.transform.SetParent(parent, worldPositionStays: false);
            enemyObject.transform.position = position;
            enemyObject.AddComponent<Rigidbody2D>();
            enemyObject.AddComponent<CircleCollider2D>();
            enemyObject.AddComponent<SpriteRenderer>();

            var runtime = enemyObject.AddComponent<EnemyRuntime>();
            runtime.Initialize(
                definition,
                target,
                runController,
                target.GetComponent<PlayerExperienceRuntime>(),
                visual);
            return runtime;
        }
    }
}
