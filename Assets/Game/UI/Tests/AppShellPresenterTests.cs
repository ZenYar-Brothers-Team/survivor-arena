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
        [Test] public void Assets_AllShellSemanticIdsExist()
        {
            var tree=Resources.Load<VisualTreeAsset>("UI/AppShell").CloneTree();
            foreach(var field in typeof(GameplayUiElementIds).GetFields())
                if(field.Name.StartsWith("Shell")||field.Name.StartsWith("Settings"))Assert.IsNotNull(tree.Q((string)field.GetValue(null)),field.Name);
            Assert.IsNotNull(Resources.Load<StyleSheet>("UI/AppShellStyles"));
        }
    }
}
