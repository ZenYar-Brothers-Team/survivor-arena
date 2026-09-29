using System.Threading.Tasks;
using Game.Settings;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
namespace Game.UI.Tests
{
    public sealed class AppShellPresenterTests
    {
        [Test] public async Task Settings_Back_ReturnsToOwningScreenAndBlocksBackgroundActions()
        {
            var nav=new FakeAppNavigation();var view=new FakeAppShellView();var audio=new FakeAudioPreview();
            var settings=new SettingsService(SettingsConfig.Load(),new MemorySettingsStore(),new FakeVideoDevice());await settings.LoadAsync();
            using var presenter=new AppShellPresenter(nav,settings,audio,view);
            view.OpenSettings();view.Start();view.Stop();Assert.AreEqual(0,nav.Plays);Assert.AreEqual(0,nav.Quits);
            Assert.IsTrue(view.State.Settings);Assert.IsFalse(view.State.Menu);Assert.AreEqual("WASD",view.State.Bindings);
            view.CloseSettings();Assert.IsTrue(view.State.Menu);Assert.AreEqual(1,audio.Stops);
            nav.AtMainMenu=false;nav.AtManualPause=true;view.OpenSettings();view.CloseSettings();
            Assert.IsTrue(view.State.PauseActions);Assert.IsTrue(nav.AtManualPause);
        }
        [Test] public async Task DevelopmentUnlock_OnlyInDevelopmentTools_AtMainMenu()
        {
            var nav=new FakeAppNavigation();var view=new FakeAppShellView();
            var settings=new SettingsService(SettingsConfig.Load(),new MemorySettingsStore(),new FakeVideoDevice());await settings.LoadAsync();
            using var presenter=new AppShellPresenter(nav,settings,new FakeAudioPreview(),view);
            Assert.IsFalse(view.State.DevelopmentUnlock);view.UnlockAll();Assert.AreEqual(0,nav.DevelopmentUnlocks);
            nav.DevelopmentTools=true;presenter.Refresh();Assert.IsTrue(view.State.DevelopmentUnlock);
            view.UnlockAll();Assert.AreEqual(1,nav.DevelopmentUnlocks);
            view.OpenSettings();Assert.IsFalse(view.State.DevelopmentUnlock);view.UnlockAll();Assert.AreEqual(1,nav.DevelopmentUnlocks);
        }
        [Test] public async Task DevelopmentReset_NeedsSecondClick_AndDisarmsOutsideMainMenu()
        {
            var nav=new FakeAppNavigation();var view=new FakeAppShellView();
            var settings=new SettingsService(SettingsConfig.Load(),new MemorySettingsStore(),new FakeVideoDevice());await settings.LoadAsync();
            using var presenter=new AppShellPresenter(nav,settings,new FakeAudioPreview(),view);
            view.ResetProgress();view.ResetProgress();Assert.AreEqual(0,nav.DevelopmentResets,"Hidden without development tools.");
            nav.DevelopmentTools=true;presenter.Refresh();
            view.ResetProgress();Assert.IsTrue(view.State.DevelopmentResetArmed);Assert.AreEqual(0,nav.DevelopmentResets);
            view.ResetProgress();Assert.AreEqual(1,nav.DevelopmentResets);Assert.IsFalse(view.State.DevelopmentResetArmed);
            view.ResetProgress();view.OpenSettings();Assert.IsFalse(view.State.DevelopmentResetArmed);
            view.CloseSettings();view.ResetProgress();Assert.AreEqual(1,nav.DevelopmentResets,"Leaving the menu disarms the reset.");
            view.UnlockAll();Assert.IsFalse(view.State.DevelopmentResetArmed,"Another command disarms the reset.");
        }
        [Test] public void Assets_AllShellSemanticIdsExist()
        {
            var tree=Resources.Load<VisualTreeAsset>("UI/AppShell").CloneTree();
            foreach(var field in typeof(GameplayUiElementIds).GetFields())
                if(field.Name.StartsWith("Shell")||field.Name.StartsWith("Settings"))Assert.IsNotNull(tree.Q((string)field.GetValue(null)),field.Name);
            Assert.IsNotNull(Resources.Load<StyleSheet>("UI/AppShellStyles"));
            Assert.IsNotNull(Resources.Load<StyleSheet>("UI/SettingsStyles"));
            Assert.IsNotNull(Resources.Load<StyleSheet>("UI/FolioChromeStyles"));
            Assert.IsNotNull(tree.Q<DropdownField>(GameplayUiElementIds.SettingsWindow));
        }
        [Test] public async Task Settings_VideoConfirmation_UsesServiceCountdownAndReturnsToSettings()
        {
            var view = new FakeAppShellView(); var nav = new FakeAppNavigation();
            var settings = new SettingsService(SettingsConfig.Load(), new MemorySettingsStore(), new FakeVideoDevice());
            await settings.LoadAsync(); using var presenter = new AppShellPresenter(nav, settings, new FakeAudioPreview(), view);
            view.OpenSettings(); Assert.IsFalse(view.State.CanApplyVideo);
            settings.SetCandidate(settings.Video.SafeWindow); Assert.IsTrue(view.State.CanApplyVideo);
            await settings.ApplyVideoAsync(); Assert.IsTrue(view.State.Confirming);
            StringAssert.Contains("10 с.", view.State.VideoStatus); Assert.IsFalse(view.State.CanApplyVideo);
            settings.Tick(4); StringAssert.Contains("6 с.", view.State.VideoStatus);
            presenter.Back(); Assert.IsTrue(view.State.Settings); Assert.IsFalse(view.State.Confirming);
            Assert.AreEqual(settings.Video.Desktop, settings.Video.Current);
            nav.AtMainMenu = false; nav.AtManualPause = true; presenter.Refresh();
            Assert.IsTrue(view.State.SettingsFromPause);
        }
        [Test] public async Task Settings_MouseMovementToggle_UpdatesPersistedPreference()
        {
            var view=new FakeAppShellView();var settings=new SettingsService(SettingsConfig.Load(),new MemorySettingsStore(),new FakeVideoDevice());await settings.LoadAsync();
            using var presenter=new AppShellPresenter(new FakeAppNavigation(),settings,new FakeAudioPreview(),view);
            view.OpenSettings();view.SetMouseMovement(true);
            Assert.IsTrue(settings.Current.MouseMovement);Assert.IsTrue(view.State.Values.MouseMovement);
        }
    }
}
