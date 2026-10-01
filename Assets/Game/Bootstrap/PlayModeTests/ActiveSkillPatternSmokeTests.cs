using System.Collections;
using System.Linq;
using Game.ActiveSkill;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class ActiveSkillPatternSmokeTests
    {
        [UnityTest]
        public IEnumerator LowTierSetAttacks_LoadWorldVisualsPauseAndClearOnRunEnd()
        {
            var owner = new GameObject("Low-tier world visual smoke owner");
            var controller = owner.AddComponent<RunController>();
            controller.Model.Start();
            var catalog = RuntimeContentCatalog.CreateProduction();
            var launcher = new SceneProjectileLauncher(controller);
            var executor = new SceneActiveSkillEffectExecutor(controller, launcher,
                contentRegistry: catalog.Registry, worldEffectProfiles: catalog.SkillWorldEffects);
            try
            {
                foreach (var name in new[] { "SET-021-ATTACK", "SET-022-ATTACK" })
                {
                    var template = catalog.SetAttackTemplates.Single(s => s.Id.ToString() == name);
                    var level = template.GetLevel(1);
                    executor.Schedule(new ActiveSkillActivation(template.Id, 1, Vector2.one * 10000f, Vector2.up,
                        null, level.BaseDamage, level, owner.transform, random: new System.Random(7)));
                }
                executor.Tick(0f, true);
                Assert.AreEqual(1, launcher.ActiveCount);
                var stone = Object.FindObjectsByType<FixtureProjectileRuntime>(FindObjectsSortMode.None)
                    .Single(p => p.SourceId.ToString() == "SET-021-ATTACK");
                var renderer = stone.GetComponentsInChildren<SpriteRenderer>().Single(r => r.enabled);
                Assert.IsNotNull(renderer.sprite);
                StringAssert.Contains("skill-001-projectile", renderer.sprite.name, "Production stone art, not the placeholder.");
                Assert.Greater(executor.ActiveWorldEffectShapeCount, 0, "Production cone profile reaches the executor.");
                controller.Model.Pause();
                var count = executor.ActiveWorldEffectShapeCount;
                var position = stone.Position;
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                executor.Tick(1f, false);
                Assert.AreEqual(position, stone.Position);
                Assert.AreEqual(count, executor.ActiveWorldEffectShapeCount);
                controller.Model.Stop();
                executor.Tick(0f, false);
                Assert.AreEqual(0, executor.ActiveWorldEffectShapeCount);
                Assert.AreEqual(0, launcher.ActiveCount);
                Assert.IsFalse(stone.gameObject.activeSelf);
                Assert.IsFalse(renderer.enabled);
            }
            finally
            {
                executor.Dispose();
                Object.Destroy(owner);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator RandomDeceleratingProjectiles_MoveFreezeAndReturnToPoolOnRunEnd()
        {
            var owner = new GameObject("IP08 smoke owner");
            var controller = owner.AddComponent<RunController>();
            controller.Model.Start();
            var launcher = new SceneProjectileLauncher(controller);
            var executor = new SceneActiveSkillEffectExecutor(controller, launcher);
            try
            {
                var definition = FixtureActiveSkillCatalog.Create().Single(s => s.Id.ToString() == "FIXTURE-SKILL-DECELERATING");
                var level = definition.GetLevel(3);
                executor.Schedule(new ActiveSkillActivation(definition.Id, 3, Vector2.one * 10000f, Vector2.right,
                    null, level.BaseDamage, level, owner.transform, random: new System.Random(level.Targeting.RandomSeed.Value)));
                executor.Tick(0f, true);
                Assert.AreEqual(2, launcher.ActiveCount);
                var projectiles = Object.FindObjectsByType<FixtureProjectileRuntime>(FindObjectsSortMode.None)
                    .Where(p => p.SourceId == definition.Id).ToArray();
                Assert.AreEqual(2, projectiles.Length);
                Assert.AreNotEqual(projectiles[0].Direction, projectiles[1].Direction);
                var origin = projectiles[0].Position;
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                Assert.Greater(Vector2.Distance(origin, projectiles[0].Position), 0f);
                controller.Model.Pause();
                var paused = projectiles[0].Position;
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                Assert.AreEqual(paused, projectiles[0].Position);
                controller.Model.Resume();
                controller.Model.Kill();
                executor.Tick(0f, false);
                Assert.AreEqual(0, launcher.ActiveCount);
                Assert.AreEqual(2, launcher.InactiveCount);
                Assert.IsFalse(projectiles[0].GetComponent<Collider2D>().enabled);
                Assert.IsFalse(projectiles[0].gameObject.activeSelf);
            }
            finally
            {
                executor.Dispose();
                Object.Destroy(owner);
            }
            yield return null;
        }
    }
}
