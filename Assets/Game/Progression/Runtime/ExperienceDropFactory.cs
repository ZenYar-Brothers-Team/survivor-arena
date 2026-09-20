using System;
using Game.Pooling;
using Game.Run;
using UnityEngine;

namespace Game.Progression
{
    public static class ExperienceDropFactory
    {
        public static ExperienceDropRuntime Spawn(
            float amount,
            Vector2 position,
            float lifetime,
            PlayerExperienceRuntime target,
            RunController runController,
            Transform parent = null,
            GameObjectPool<ExperienceDropRuntime> pool = null)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (runController == null)
                throw new ArgumentNullException(nameof(runController));

            var runtime = pool != null ? pool.Rent() : CreateInstance();
            runtime.transform.SetParent(parent, worldPositionStays: false);
            runtime.transform.position = position;
            runtime.Initialize(amount, lifetime, target, runController, target.PickupRadius, pool);
            return runtime;
        }

        public static ExperienceDropRuntime CreateInstance()
        {
            var dropObject = new GameObject("Experience Drop");
            dropObject.AddComponent<CircleCollider2D>();
            dropObject.AddComponent<SpriteRenderer>();
            return dropObject.AddComponent<ExperienceDropRuntime>();
        }
    }
}
