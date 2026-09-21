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
