using System.Collections;
using System.Linq;
using Game.Enemy;
using Game.Presentation;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class EnemyBodySmokeTests
    {
        [UnityTest]
        public IEnumerator GameplaySpawner_UsesApprovedVillager_AndResetsPooledBody()
        {
            ProfileSmokeScene.Load();
            yield return null;
            yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            CharacterSelectionSmokeDriver.StartDefault(root);
            yield return null;
            var spawner = Object.FindAnyObjectByType<ContinuousFixtureEnemySpawner>();
            var run = Object.FindAnyObjectByType<RunController>();
            spawner.Tick(run.Model.Elapsed, 2, true);
            yield return new WaitForFixedUpdate();
            var enemy = spawner.GetComponentsInChildren<EnemyRuntime>()
                .First(e => e.ContentId.ToString() == "FIXTURE-ENEMY-SEEKER");
            var life = enemy.LifeId;
            var rig = enemy.GetComponentInChildren<SpritePresentationRig>();
            Assert.IsNotNull(rig);
            Assert.AreEqual("enemy-001-body", rig.BodyRenderer.sprite.name);
            Assert.IsFalse(enemy.GetComponent<SpriteRenderer>().enabled);
            var radius = enemy.GetComponent<CircleCollider2D>().radius;
            run.TogglePause();
            yield return null;
            var position = rig.BodyRoot.localPosition;
            var scale = rig.BodyRoot.localScale;
            yield return new WaitForSecondsRealtime(.1f);
            Assert.AreEqual(position, rig.BodyRoot.localPosition);
            Assert.AreEqual(scale, rig.BodyRoot.localScale);
            run.TogglePause();
            enemy.TakeDamage(10000);
            Assert.IsFalse(enemy.gameObject.activeSelf);
            Assert.IsNull(rig.BodyRenderer.sprite);
            spawner.Tick(run.Model.Elapsed, 2, true);
            Assert.IsTrue(enemy.gameObject.activeSelf);
            Assert.AreNotEqual(life, enemy.LifeId);
            Assert.AreEqual("enemy-001-body", rig.BodyRenderer.sprite.name);
            Assert.AreEqual(Color.white, rig.BodyRenderer.color);
            Assert.AreEqual(radius, enemy.GetComponent<CircleCollider2D>().radius);
            root.Shutdown();
        }
    }
}
