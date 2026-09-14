using System;
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
            Transform parent = null)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (runController == null)
                throw new ArgumentNullException(nameof(runController));

            var dropObject = new GameObject("Experience Drop");
            dropObject.transform.SetParent(parent, worldPositionStays: false);
            dropObject.transform.position = position;
            dropObject.AddComponent<CircleCollider2D>();
            dropObject.AddComponent<SpriteRenderer>();

            var runtime = dropObject.AddComponent<ExperienceDropRuntime>();
            runtime.Initialize(amount, lifetime, target, runController);
            return runtime;
        }
    }
}
