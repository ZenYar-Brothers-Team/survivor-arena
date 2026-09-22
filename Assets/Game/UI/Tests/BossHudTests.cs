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
        public void FinalBoss_NotificationExpiresOnRunTimeAndBarCleansUp()
        {
            var root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Resources/UI/GameplayUi.uxml").CloneTree();
            using var view = new UiToolkitGameplayView(root);
            view.SetDevelopmentControlsVisible(false);
            var boss = new BossViewState(Guid.NewGuid(), "Commander", 150, 500);
            HudViewState Hud(float elapsed, BossViewState state) => new HudViewState(80, 100, .5f, 1, elapsed,
                new WaveViewState(1, 1, "Final", WavePhaseTag.Elite), boss: state);
            view.RenderHud(Hud(800, boss));
            var bar = root.Q<ProgressBar>(GameplayUiElementIds.BossBar);
            var notification = root.Q<Label>(GameplayUiElementIds.Notification);
            Assert.AreEqual(DisplayStyle.Flex, bar.style.display.value);
            Assert.AreEqual(30, bar.value);
            StringAssert.Contains("Commander", bar.title);
            Assert.AreEqual("BOSS INCOMING", notification.text);
            view.RenderHud(Hud(800, boss));
            Assert.AreEqual(DisplayStyle.Flex, notification.style.display.value, "Pause keeps notification.");
            view.RenderHud(Hud(804, boss));
            Assert.AreEqual(DisplayStyle.None, notification.style.display.value);
            view.RenderHud(Hud(805, default));
            Assert.AreEqual(DisplayStyle.None, bar.style.display.value);
            Assert.AreEqual("13:25", root.Q<Label>(GameplayUiElementIds.TimerLabel).text);
        }
    }
}
