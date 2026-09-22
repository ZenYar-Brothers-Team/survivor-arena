using System.Collections;
using Game.Run;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Game.Bootstrap.PlayModeTests
{
    public sealed class AppShellSmokeTests
    {
        private static void Click(GameplayCompositionRoot root,string id)
        {
            var button=root.ShellDocument.rootVisualElement.Q<Button>(id); Assert.IsNotNull(button); Assert.IsTrue(button.enabledInHierarchy);
            using var submit=NavigationSubmitEvent.GetPooled();submit.target=button;button.SendEvent(submit);
        }
        [UnityTest] public IEnumerator Menu_Settings_RunPause_Settings_Quit_Results_Retry()
        {
            ProfileSmokeScene.Load(false);yield return null;yield return null;
            var root=Object.FindAnyObjectByType<GameplayCompositionRoot>();var run=Object.FindAnyObjectByType<RunController>();
            try
            {
                Assert.IsTrue(root.AtMainMenu);Assert.IsNull(root.SelectionDocument);
                Click(root,GameplayUiElementIds.ShellSettings);
                var ui=root.ShellDocument.rootVisualElement;
                Assert.AreEqual(DisplayStyle.Flex,ui.Q(GameplayUiElementIds.SettingsBody).resolvedStyle.display);
                ui.Q<Slider>(GameplayUiElementIds.SettingsMaster).value=25f;
                ui.Q<Toggle>(GameplayUiElementIds.SettingsShake).value=false;
                Click(root,GameplayUiElementIds.SettingsBack);yield return null;
                Assert.IsFalse(root.Settings.Dirty);Assert.AreEqual(.25f,root.Settings.Current.Master);Assert.IsFalse(root.Settings.Current.Shake);
                Click(root,GameplayUiElementIds.ShellPlay);CharacterSelectionSmokeDriver.StartDefault(root);yield return null;
                var id=run.Model.RunId;run.Model.Pause();run.Model.RequestPause("other-owner");
                Click(root,GameplayUiElementIds.ShellPauseSettings);
                Click(root,GameplayUiElementIds.SettingsBack);
                Assert.AreEqual(2,run.Model.PauseReasonCount);Assert.AreEqual(RunState.Paused,run.Model.State);
                Click(root,GameplayUiElementIds.ShellQuit);yield return null;
                Assert.IsTrue(root.Profile.CanStart);Assert.IsNotNull(run.Model.Outcome);
                var retry=root.ProfileDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.MetaRetry);
                using(var submit=NavigationSubmitEvent.GetPooled()) {submit.target=retry;retry.SendEvent(submit);}
                yield return null;Assert.AreNotEqual(id,run.Model.RunId);Assert.AreEqual(RunState.Running,run.Model.State);
                Assert.AreEqual(.25f,root.Settings.Current.Master);Assert.IsFalse(root.Settings.Current.Shake);
            }
            finally { root.Shutdown(); }
        }
    }
}
