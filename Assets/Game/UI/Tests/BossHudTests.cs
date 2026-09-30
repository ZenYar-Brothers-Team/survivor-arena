using System;
using Game.Enemy;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace Game.UI.Tests
{
    public sealed class BossHudTests
    {
        [Test]
        public void BossBar_VisibleWhileBossAliveAndCleansUp()
        {
            var root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Resources/UI/GameplayUi.uxml").CloneTree();
            using var view = new UiToolkitGameplayView(root);
            view.SetDevelopmentControlsVisible(false);
            var boss = new BossViewState(Guid.NewGuid(), "Commander", 150, 500);
            HudViewState Hud(float elapsed, BossViewState state) => new HudViewState(80, 100, .5f, 1, elapsed,
                new WaveViewState(1, 1, "Final", WavePhaseTag.Elite), boss: state, runDurationSeconds: 900f);
            view.RenderHud(Hud(800, boss));
            var bar = root.Q<ProgressBar>(GameplayUiElementIds.BossBar);
            Assert.AreEqual(DisplayStyle.Flex, bar.style.display.value);
            Assert.AreEqual(30, bar.value);
            Assert.AreEqual("Commander", bar.title, "DECISION-0110: no HP numbers on health bars.");
            Assert.IsNull(root.Q("hud-notification"), "DECISION-0107: the shell toast is the only notification surface.");
            view.RenderHud(Hud(805, default));
            Assert.AreEqual(DisplayStyle.None, bar.style.display.value);
            Assert.AreEqual("01:35", root.Q<Label>(GameplayUiElementIds.TimerLabel).text);
            view.RenderHud(Hud(900, default));
            Assert.AreEqual("00:00", root.Q<Label>(GameplayUiElementIds.TimerLabel).text);
            view.RenderHud(Hud(901, default));
            Assert.AreEqual("00:00", root.Q<Label>(GameplayUiElementIds.TimerLabel).text);
        }
    }
}
