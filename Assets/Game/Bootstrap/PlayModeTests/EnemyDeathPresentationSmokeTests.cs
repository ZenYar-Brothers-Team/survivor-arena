using System.Collections;
using Game.Enemy;
using Game.Presentation;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class EnemyDeathPresentationSmokeTests
    {
        [UnityTest]
        public IEnumerator Death_StopsInPlace_PausesPresentation_AndDespawnsAfterSharedEffect()
        {
            var baseline = EnemyRegistry.Count;
            var root = new GameObject("death presentation smoke");
            var target = new GameObject("target");
            target.transform.SetParent(root.transform);
            var run = root.AddComponent<RunController>();
            run.Model.Start();
            var profile = FixtureEnemyDeathPresentationCatalog.Create();
            var enemy = EnemyFactory.Spawn(new EnemyDefinition("FIXTURE-DEATH", 1, 1, 0, 0, 1),
                new Vector2(2, 3), target.transform, run, root.transform,
                deathPresentation: profile);
            var position = enemy.transform.position;
            var died = 0;
            var despawned = 0;
            enemy.Died += _ => died++;
            enemy.Despawned += _ => despawned++;

            enemy.TakeDamage(1);

            Assert.AreEqual(1, died);
            Assert.AreEqual(0, despawned);
            Assert.AreEqual(position, enemy.transform.position);
            Assert.IsFalse(enemy.GetComponent<Rigidbody2D>().simulated);
            Assert.IsFalse(enemy.GetComponent<CircleCollider2D>().enabled);
            Assert.IsTrue(enemy.GetComponent<EnemyDeathPresentationRuntime>().IsPlaying);
            Assert.AreEqual(baseline, EnemyRegistry.Count, "Dead enemy must stop being targetable immediately.");
            run.TogglePause();
            yield return new WaitForSeconds(profile.TotalDurationSeconds + .05f);
            Assert.IsTrue(enemy != null, "Pause must freeze the death presentation.");
            Assert.AreEqual(position, enemy.transform.position, "Death must not add a push or displacement.");
            run.TogglePause();
            yield return new WaitForSeconds(profile.TotalDurationSeconds + .05f);
            Assert.AreEqual(1, despawned);
            Assert.IsTrue(enemy == null);
            Object.Destroy(root);
            yield return null;
            Assert.AreEqual(baseline, EnemyRegistry.Count);
        }
    }
}
