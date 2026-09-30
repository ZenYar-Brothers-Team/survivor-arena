using System.Collections;
using System.Linq;
using Game.Character;
using Game.Combat;
using Game.Content;
using Game.Enemy;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class RunAchievementSmokeTests
    {
        [UnityTest]
        public IEnumerator ProductionHitAndKill_SaveAttributedProgressOnce()
        {
            GameplayCompositionRoot root = null;
            try
            {
                ProductionSmokeScene.Load();
                yield return null;
                yield return null;
                root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                CharacterSelectionSmokeDriver.StartDefault(root);
                var run = Object.FindAnyObjectByType<RunController>();
                var player = Object.FindAnyObjectByType<PlayerCharacterRuntime>();
                EnemyRuntime enemy = null;
                for (var i = 0; i < 240 && enemy == null; i++)
                {
                    yield return new WaitForFixedUpdate();
                    enemy = Object.FindObjectsByType<EnemyRuntime>(FindObjectsSortMode.None).FirstOrDefault(e => e.IsAlive);
                }
                Assert.IsNotNull(enemy);
                var source = new CombatSource(player.Identity, new ContentId("SKILL-001"), CombatSourceOrigin.ActiveSkill);
                var actual = enemy.ApplyDamage(new EnemyDamageRequest(new CombatDamageRequest(source, 100000f)));
                Assert.Greater(actual, 0f);
                run.Model.Stop();
                var facts = run.Model.Outcome.Contributions["achievements"].Achievements;
                Assert.AreEqual(1, facts.OrdinaryKills);
                Assert.Greater(facts.DamageBySource["SKILL-001"], 0);
                for (var i = 0; i < 120 && root.Profile.LastReceipt == null; i++) yield return null;
                Assert.IsNotNull(root.Profile.LastReceipt);
                Assert.AreEqual(1, root.Profile.UnlockProgress("FIELD-002"));
            }
            finally
            {
                if (root != null && root.IsInitialized) root.Shutdown();
            }
        }
    }
}
