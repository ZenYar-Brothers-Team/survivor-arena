using System.Collections;
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
    public sealed class WaveBurstSmokeTests
    {
        [UnityTest]
        public IEnumerator Gameplay_BurstPauseHudTerminalAndRestart_UseRealTimeline()
        {
            SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
            yield return null;
            yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            CharacterSelectionSmokeDriver.StartDefault(root);
            var run = Object.FindAnyObjectByType<RunController>();
            var spawner = Object.FindAnyObjectByType<ContinuousFixtureEnemySpawner>();
            var ui = Object.FindAnyObjectByType<GameplayUiRoot>();
            try
            {
                // Manually drive the public adapter while the real run remains alive;
                // disable Update to keep the controlled timeline deterministic.
                spawner.enabled = false;
                Assert.AreEqual(8, spawner.Tick(40, 40, true));
                run.TogglePause();
                Assert.AreEqual(0, spawner.Tick(45, 5, run.Model.State == RunState.Running));
                Assert.AreEqual(0, spawner.Director.CurrentPhaseIndex);
                yield return new WaitForSecondsRealtime(.02f);
                run.TogglePause();
                Assert.AreEqual(18, spawner.Tick(45, 5, run.Model.State == RunState.Running));
                Assert.AreEqual(26, spawner.AliveCount, "18 burst plus the full previous regular cap.");
                Assert.AreEqual(18, spawner.LastSpawnOutcome.Actual);
                Assert.AreEqual(0, spawner.Tick(45.5f, .5f, true));
                yield return null;
                var document = ui.Document.rootVisualElement;
                StringAssert.Contains("PRESSURE BURST", document.Q<Label>(GameplayUiElementIds.WaveLabel).text);
                var summary = document.Q<Label>(GameplayUiElementIds.WaveObservation).text;
                StringAssert.Contains("ignores cap", summary);
                StringAssert.Contains("requested 18 actual 18 suppressed 0 deferred 0", summary);
                Assert.AreEqual(DisplayStyle.None, document.Q<VisualElement>(GameplayUiElementIds.DevelopmentPanel).style.display.value);
                run.Model.Stop();
                Assert.AreEqual(0, spawner.Tick(200, 155, run.Model.State == RunState.Running));
                Assert.AreEqual(26, spawner.AliveCount, "Terminal freezes enemies; owner Shutdown performs cleanup.");
                root.Shutdown();
                Assert.AreEqual(0, spawner.AliveCount);
                root.OpenCharacterSelection();
                yield return null;
                CharacterSelectionSmokeDriver.StartDefault(root);
                Assert.AreEqual(0, spawner.Director.CurrentPhaseIndex);
                Assert.AreEqual(18, spawner.Tick(45, 45, true));
                Assert.AreEqual(18, spawner.LastSpawnOutcome.Actual);
            }
            finally { root.Shutdown(); }
            yield return null;
        }
    }
}
