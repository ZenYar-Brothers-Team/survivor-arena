using System.Collections.Generic;
using Game.Enemy;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    /// <summary>DECISION-0056: enemies live on their own layer and skill area queries see only that layer.</summary>
    public sealed class EnemyPhysicsLayerTests
    {
        [Test]
        public void SpawnedEnemy_IsOnEnemyLayer_PlayerIsNot()
        {
            using var context = new SkillFrameworkTestContext();
            var enemy = context.Enemy(Vector2.zero);

            Assert.AreEqual(EnemyPhysicsLayer.Index, enemy.gameObject.layer);
            Assert.AreNotEqual(EnemyPhysicsLayer.Index, context.Player.gameObject.layer);
        }

        [Test]
        public void AreaQuery_IgnoresNonEnemyTriggers_AndDamageIsUnchanged()
        {
            using var context = new SkillFrameworkTestContext();
            var enemy = context.Enemy(new Vector2(1f, 0f));
            // XP drops are Default-layer triggers sized by the XP pickup radius; many of them used to reach every AoE query.
            var clutter = new List<GameObject>();
            try
            {
                for (var i = 0; i < 100; i++)
                {
                    var drop = new GameObject("XP clutter");
                    drop.transform.position = new Vector3(i % 10 * .2f - 1f, i / 10 * .2f - 1f, 0f);
                    var collider = drop.AddComponent<CircleCollider2D>();
                    collider.isTrigger = true;
                    collider.radius = 2.5f;
                    clutter.Add(drop);
                }
                Physics2D.SyncTransforms();

                var results = new List<Collider2D>();
                Physics2D.OverlapCircle(Vector2.zero, 2f, EnemyPhysicsLayer.CreateQueryFilter(), results);
                Assert.AreEqual(1, results.Count, "Only the enemy collider reaches the component lookup.");
                Assert.AreSame(enemy.gameObject, results[0].gameObject);

                var hits = EnemyDamageArea.Apply(Vector2.zero, 2f, new EnemyDamageRequest("FIXTURE-LAYER", 10f));
                Assert.AreEqual(1, hits);
                Assert.AreEqual(90f, enemy.Health.CurrentHealth, 1e-4f);
            }
            finally
            {
                foreach (var drop in clutter) Object.DestroyImmediate(drop);
            }
        }
    }
}
