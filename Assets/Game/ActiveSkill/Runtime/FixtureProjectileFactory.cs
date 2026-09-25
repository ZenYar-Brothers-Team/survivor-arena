using System;
using Game.Pooling;
using Game.Run;
using UnityEngine;

namespace Game.ActiveSkill
{
    public static class FixtureProjectileFactory
    {
        public static FixtureProjectileRuntime Spawn(ActiveSkillProjectile projectile, RunController runController,
            Transform parent = null, GameObjectPool<FixtureProjectileRuntime> pool = null,
            Game.Enemy.ICombatTargetQuery retargetQuery = null)
        {
            if (runController == null) throw new ArgumentNullException(nameof(runController));
            var runtime = pool != null ? pool.Rent() : Create();
            try
            {
                runtime.transform.SetParent(parent, worldPositionStays: true);
                runtime.Initialize(projectile, runController, pool, retargetQuery);
                return runtime;
            }
            catch
            {
                if (pool != null) pool.Return(runtime);
                else if (Application.isPlaying) UnityEngine.Object.Destroy(runtime.gameObject);
                else UnityEngine.Object.DestroyImmediate(runtime.gameObject);
                throw;
            }
        }
        public static FixtureProjectileRuntime Create()
        {
            var obj = new GameObject("Fixture Active Skill Projectile");
            obj.AddComponent<Rigidbody2D>();
            obj.AddComponent<CircleCollider2D>();
            obj.AddComponent<SpriteRenderer>();
            return obj.AddComponent<FixtureProjectileRuntime>();
        }
    }
}
