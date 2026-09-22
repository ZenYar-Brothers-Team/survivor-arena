using System.Collections;
using System.Linq;
using Game.Enemy;
using Game.Run;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class BossEncounterSmokeTests
    {
        [UnityTest]
        public IEnumerator Gameplay_BossBarTelegraphPauseAndTerminalCleanup()
        {
            ProfileSmokeScene.Load();
            yield return null;
            yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            CharacterSelectionSmokeDriver.StartDefault(root);
            yield return null;
            try
            {
                var run = Object.FindAnyObjectByType<RunController>();
                var spawner = Object.FindAnyObjectByType<ContinuousFixtureEnemySpawner>();
                var ui = Object.FindAnyObjectByType<GameplayUiRoot>().Document.rootVisualElement;
                var bar = ui.Q<ProgressBar>(GameplayUiElementIds.BossBar);
                var timeline = spawner.Director.Timeline;
                run.Model.Tick(timeline.Hooks.Single(h => h.Kind == WaveHookKind.MidBoss).TimeSeconds - run.Model.Elapsed);
                yield return null;
                yield return null;
                Assert.IsNull(root.BossEncounters.FinalBoss);
                Assert.AreEqual(DisplayStyle.None, bar.resolvedStyle.display);
                Assert.IsTrue(root.BossEncounters.DevelopmentObservation.Contains("MidBoss"));
                run.Model.Tick(timeline.Hooks.Single(h => h.Kind == WaveHookKind.FinalBoss).TimeSeconds - run.Model.Elapsed);
                yield return null;
                yield return new WaitForFixedUpdate();
                yield return null;
                var boss = root.BossEncounters.FinalBoss;
                Assert.IsNotNull(boss);
                Assert.AreEqual(DisplayStyle.Flex, bar.resolvedStyle.display);
                Assert.Greater(bar.resolvedStyle.width, 0);
                StringAssert.Contains(root.BossEncounters.FinalDefinition.DisplayName, bar.title);
                Assert.IsTrue(boss.GetComponent<LineRenderer>().enabled);
                var timer = ui.Q<Label>(GameplayUiElementIds.TimerLabel);
                Assert.Greater(timer.resolvedStyle.height, 0);
                Assert.Less(timer.worldBound.yMax, bar.worldBound.yMin, "Timer remains unobstructed.");
                run.TogglePause();
                yield return new WaitForFixedUpdate();
                var remaining = boss.AttackPhaseRemaining;
                var position = boss.Position;
                var telegraphEnd = boss.GetComponent<LineRenderer>().GetPosition(1);
                yield return new WaitForSecondsRealtime(.1f);
                Assert.AreEqual(remaining, boss.AttackPhaseRemaining);
                Assert.AreEqual(position, boss.Position);
                Assert.AreEqual(telegraphEnd, boss.GetComponent<LineRenderer>().GetPosition(1));
                run.TogglePause();
                boss.TakeDamage(boss.Health.MaxHealth * .8f);
                yield return new WaitForFixedUpdate();
                yield return null;
                Assert.AreEqual(2, boss.BossCombat.PhaseIndex);
                Assert.LessOrEqual(bar.value, 20.01f);
                run.Model.Tick(run.Model.Duration);
                yield return null;
                Assert.AreEqual(RunState.Won, run.Model.State);
                Assert.IsNull(root.BossEncounters.FinalBoss);
                Assert.AreEqual(DisplayStyle.None, bar.resolvedStyle.display);
                Assert.IsEmpty(root.BossEncounters.GetComponentsInChildren<EnemyProjectileRuntime>());
            }
            finally { root.Shutdown(); }
        }
    }
}
