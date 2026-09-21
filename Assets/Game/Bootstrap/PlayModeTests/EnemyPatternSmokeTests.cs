using System.Collections;
using System.Linq;
using Game.Character;
using Game.Combat;
using Game.Enemy;
using Game.Pooling;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Game.Bootstrap.PlayModeTests
{
    public sealed class EnemyPatternSmokeTests
    {
        [UnityTest]
        public IEnumerator Patterns_TelegraphFirePauseAndRetainDeadShootersIdentity()
        {
            var baseline = EnemyRegistry.Count;
            var root = new GameObject("enemy pattern smoke");
            try
            {
                var run = root.AddComponent<RunController>();
                run.Model.Start();
                var target = new GameObject("target");
                target.transform.SetParent(root.transform);
                var player = target.AddComponent<PlayerCharacterRuntime>();
                player.Initialize(new CharacterBaseStats(1000, 0), run);
                target.AddComponent<CircleCollider2D>().radius = .4f;
                var pool = new GameObjectPool<EnemyProjectileRuntime>(EnemyProjectileFactory.CreateInstance, root.transform);
                var enemyPool = new GameObjectPool<EnemyRuntime>(EnemyFactory.CreateInstance, root.transform);
                var definitions = FixtureEnemyCatalog.Create().Where(d => d.Attack != null).ToArray();
                var enemies = definitions.Select((d, i) => EnemyFactory.Spawn(d, new Vector2(7, i * 2),
                    target.transform, run, root.transform, projectilePool: pool, pool: enemyPool,
                    category: i % 2 == 0 ? EnemyCategory.Boss : EnemyCategory.Traveler)).ToArray();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                Assert.IsTrue(enemies.All(e => e.AttackPhase == EnemyAttackPhase.Telegraphing));
                Assert.IsTrue(enemies.All(e => e.GetComponent<LineRenderer>().enabled));
                Assert.IsEmpty(root.GetComponentsInChildren<EnemyProjectileRuntime>());
                yield return new WaitForSeconds(.32f);
                var projectiles = root.GetComponentsInChildren<EnemyProjectileRuntime>();
                foreach (var pattern in definitions.Select(d => d.Attack.Pattern))
                    Assert.IsTrue(projectiles.Any(p => p.Profile.Pattern == pattern), pattern.ToString());
                var times = projectiles.Select(p => p.RemainingSeconds).ToArray();
                run.TogglePause();
                yield return new WaitForSecondsRealtime(.1f);
                for (var i = 0; i < projectiles.Length; i++)
                {
                    Assert.AreEqual(times[i], projectiles[i].RemainingSeconds);
                    Assert.AreEqual(Vector2.zero, projectiles[i].GetComponent<Rigidbody2D>().linearVelocity);
                }
                run.TogglePause();
                var shooter = enemies[0];
                var source = new CombatSource(shooter.Identity, shooter.ContentId, CombatSourceOrigin.EnemyProjectile);
                var shot = EnemyProjectileFactory.Spawn(new EnemyAttackProfile(EnemyProjectilePattern.Single, 2, 1, 3, 2),
                    Vector2.right, Vector2.left, player, run, root.transform, pool, source);
                shooter.TakeDamage(10000);
                var reused = EnemyFactory.Spawn(definitions[0], Vector2.right * 10, target.transform, run, root.transform,
                    pool: enemyPool, projectilePool: pool, category: EnemyCategory.Traveler);
                Assert.AreSame(shooter, reused);
                Assert.AreNotEqual(source.Owner.LifeId, reused.LifeId);
                CombatResult hit = default;
                var shotReleasedAtHit = false;
                player.CombatResolved += result => { hit = result; shotReleasedAtHit = !shot.IsActive; };
                yield return new WaitForSeconds(.3f);
                Assert.AreEqual(source.Owner.LifeId, hit.Source.Owner.LifeId);
                Assert.AreEqual(CombatEntityCategory.Boss, hit.Source.Owner.Category);
                Assert.AreEqual(2, hit.Health.Actual);
                Assert.IsTrue(shotReleasedAtHit, "The pool may already have rented the component for a different shot by the next frame.");
                run.Model.Stop();
                Assert.IsEmpty(root.GetComponentsInChildren<EnemyProjectileRuntime>());
            }
            finally { Object.Destroy(root); }
            yield return null;
            Assert.AreEqual(baseline, EnemyRegistry.Count);
        }
    }
}
