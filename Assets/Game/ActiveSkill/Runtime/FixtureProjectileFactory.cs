using System;
using Game.Run;
using UnityEngine;

namespace Game.ActiveSkill
{
    public static class FixtureProjectileFactory
    {
        public static FixtureProjectileRuntime Spawn(
            ActiveSkillProjectile projectile,
            RunController runController,
            Transform parent = null)
        {
            if (runController == null)
                throw new ArgumentNullException(nameof(runController));

            var projectileObject = new GameObject("Fixture Active Skill Projectile");
            projectileObject.transform.SetParent(parent, worldPositionStays: true);
            projectileObject.AddComponent<Rigidbody2D>();
            projectileObject.AddComponent<CircleCollider2D>();
            projectileObject.AddComponent<SpriteRenderer>();

            var runtime = projectileObject.AddComponent<FixtureProjectileRuntime>();
            runtime.Initialize(projectile, runController);
            return runtime;
        }
    }
}
