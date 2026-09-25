using System.Collections.Generic;
using Game.Enemy;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    /// <summary>
    /// Batched orbit blade damage (perf fix for "EnemyDamageArea.Apply" warnings): one physics query for the ring must
    /// hit exactly what one <see cref="EnemyDamageArea.Apply"/> per blade did.
    /// </summary>
    public sealed class EnemyDamageAreaCirclesTests
    {
        private const float BladeRadius = 0.3f;
        private static readonly Vector2[] BladeOffsets = { new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(-1f, 0f), new Vector2(0f, -1f) };
        // Enemy colliders have radius 0.5: (1.6, 0) touches only the +X blade through its collider, (0.75, 0.75)
        // overlaps the +X and +Y blades, (0, -1) sits on the -Y blade, (3, 3) is outside the ring.
        private static readonly Vector2[] EnemyOffsets = { new Vector2(1.6f, 0f), new Vector2(0.75f, 0.75f), new Vector2(0f, -1f), new Vector2(3f, 3f) };

        [Test]
        public void ApplyCircles_DamagesExactlyLikeOneApplyPerCircle()
        {
            using var context = new SkillFrameworkTestContext();
            var separate = new Vector2(-50f, 0f);
            var batched = new Vector2(50f, 0f);
            var separateEnemies = Spawn(context, separate);
            var batchedEnemies = Spawn(context, batched);
            Physics2D.SyncTransforms();
            var damage = new EnemyDamageRequest("FIXTURE-ORBIT-BATCH", 10f);

            var separateHits = 0;
            foreach (var offset in BladeOffsets) separateHits += EnemyDamageArea.Apply(separate + offset, BladeRadius, damage);
            var batchedHits = EnemyDamageArea.ApplyCircles(batched, 1f + BladeRadius, Centers(batched), BladeRadius, damage);

            Assert.AreEqual(separateHits, batchedHits);
            for (var i = 0; i < EnemyOffsets.Length; i++)
                Assert.AreEqual(separateEnemies[i].Health.CurrentHealth, batchedEnemies[i].Health.CurrentHealth, 1e-4f, $"enemy {i}");
        }

        [Test]
        public void ApplyCircles_EnemyUnderTwoNeighbouringBlades_TakesOneHitPerBlade()
        {
            using var context = new SkillFrameworkTestContext();
            var enemies = Spawn(context, Vector2.zero);
            Physics2D.SyncTransforms();

            EnemyDamageArea.ApplyCircles(Vector2.zero, 1f + BladeRadius, Centers(Vector2.zero), BladeRadius,
                new EnemyDamageRequest("FIXTURE-ORBIT-BATCH", 10f));

            Assert.AreEqual(90f, enemies[0].Health.CurrentHealth, 1e-4f, "Collider overlap counts, not the enemy center.");
            Assert.AreEqual(80f, enemies[1].Health.CurrentHealth, 1e-4f, "Two blades, two hits — as with two Apply calls.");
            Assert.AreEqual(90f, enemies[2].Health.CurrentHealth, 1e-4f);
            Assert.AreEqual(100f, enemies[3].Health.CurrentHealth, 1e-4f);
        }

        [Test]
        public void ApplyCircles_ZeroRadiusOrNoCircles_DealsNoDamage()
        {
            using var context = new SkillFrameworkTestContext();
            var enemy = context.Enemy(new Vector2(1f, 0f));
            Physics2D.SyncTransforms();
            var damage = new EnemyDamageRequest("FIXTURE-ORBIT-BATCH", 10f);
            Assert.AreEqual(0, EnemyDamageArea.ApplyCircles(Vector2.zero, 2f, Centers(Vector2.zero), 0f, damage));
            Assert.AreEqual(0, EnemyDamageArea.ApplyCircles(Vector2.zero, 2f, new List<Vector2>(), BladeRadius, damage));
            Assert.AreEqual(100f, enemy.Health.CurrentHealth, 1e-4f);
        }

        private static EnemyRuntime[] Spawn(SkillFrameworkTestContext context, Vector2 origin)
        {
            var enemies = new EnemyRuntime[EnemyOffsets.Length];
            for (var i = 0; i < EnemyOffsets.Length; i++) enemies[i] = context.Enemy(origin + EnemyOffsets[i]);
            return enemies;
        }

        private static List<Vector2> Centers(Vector2 origin)
        {
            var centers = new List<Vector2>();
            foreach (var offset in BladeOffsets) centers.Add(origin + offset);
            return centers;
        }
    }
}
