using System.Collections;
using System.Linq;
using Game.Character;
using Game.Content;
using Game.Enemy;
using Game.Presentation;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class BodyContactSmokeTests
    {
        [UnityTest]
        public IEnumerator Circles_EightApproachDirections_DamageOnlyAfterContact()
        {
            var baseline = EnemyRegistry.Count;
            var root = new GameObject("contact smoke");
            try
            {
                var run = root.AddComponent<RunController>();
                run.Model.Start();
                var visuals = FixtureSpriteCatalog.CreateFor(new ContentId[] {
                    "FIXTURE-CHARACTER-AGILE-VISUAL-BODY", "FIXTURE-ENEMY-SEEKER-VISUAL" });
                var actor = new GameObject("player");
                actor.transform.SetParent(root.transform);
                var player = actor.AddComponent<PlayerCharacterRuntime>();
                player.Initialize(new CharacterBaseStats(1000, 0), run);
                var playerCircle = actor.AddComponent<CircleCollider2D>();
                visuals[0].Contact.Apply(playerCircle);
                var playerBody = actor.AddComponent<Rigidbody2D>();
                playerBody.gravityScale = 0;
                playerBody.constraints = RigidbodyConstraints2D.FreezeAll;
                var profile = FixtureSpriteMotionProfileCatalog.Create().Single(p => p.Id.ToString() == "FIXTURE-MOTION-VILLAGER");
                var config = FixtureEnemyCatalog.Create().First();
                // Test-only no-knockback enemy isolates the contact boundary from displacement.
                var definition = new EnemyDefinition("FIXTURE-CONTACT-TEST", 100, 1, 0, 1, 10,
                    visual: config.Visual);
                var enemy = EnemyFactory.Spawn(definition, Vector2.right * 3, actor.transform, run,
                    root.transform, visuals[1].Sprite, motionProfile: profile, contact: visuals[1].Contact);
                var enemyBody = enemy.GetComponent<Rigidbody2D>();
                enemyBody.constraints = RigidbodyConstraints2D.FreezeAll;
                var radiusSum = playerCircle.radius + enemy.GetComponent<CircleCollider2D>().radius;
                Assert.Less(radiusSum, .9f, "Old sideways contact distance was .9 world units.");
                for (var i = 0; i < 8; i++)
                {
                    var angle = i * Mathf.PI / 4;
                    var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    enemyBody.position = direction * (radiusSum + .08f);
                    Physics2D.SyncTransforms();
                    yield return new WaitForFixedUpdate();
                    yield return new WaitForFixedUpdate();
                    var health = player.Health.CurrentHealth;
                    yield return new WaitForFixedUpdate();
                    Assert.AreEqual(health, player.Health.CurrentHealth, "Separated direction " + i);
                    Assert.Greater(playerCircle.Distance(enemy.GetComponent<CircleCollider2D>()).distance, 0);
                    enemyBody.position = direction * (radiusSum - .025f);
                    Physics2D.SyncTransforms();
                    yield return new WaitForFixedUpdate();
                    yield return new WaitForFixedUpdate();
                    Assert.AreEqual(health - 1, player.Health.CurrentHealth, "Contact direction " + i);
                }
            }
            finally { Object.Destroy(root); }
            yield return null;
            Assert.AreEqual(baseline, EnemyRegistry.Count);
        }
    }
}
