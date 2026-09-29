using System.Collections;
using Game.Character;
using Game.Run;
using Game.UI;
using Game.Settings;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Game.Bootstrap.PlayModeTests
{
    public sealed class SettingsPresentationSmokeTests
    {
        [UnityTest] public IEnumerator SettingsFolio_TwoResolutions_ConfirmationAndFocus()
        {
            ProfileSmokeScene.Load(false); yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            RenderTexture target = null;
            try
            {
                var document = root.ShellDocument; var ui = document.rootVisualElement;
                UiFoundationSmokeTests.Submit(ui.Q<Button>(GameplayUiElementIds.ShellSettings)); yield return null;
                Assert.IsFalse(root.Settings.Current.MouseMovement);
                Assert.IsFalse(ui.Q<Toggle>(GameplayUiElementIds.SettingsMouseMovement).value);
                Assert.AreEqual(new Color32(31, 25, 37, 255), (Color32)Camera.main.backgroundColor);
                Assert.IsFalse(GameObject.Find("Obstacle_Fixture").GetComponent<SpriteRenderer>().enabled);
                foreach (var size in new[] { new Vector2Int(1920,1080), new Vector2Int(1280,720) })
                {
                    target = new RenderTexture(size.x,size.y,24); document.panelSettings.targetTexture = target;
                    yield return null; yield return null;
                    var backdrop = ui.Q(className: "folio-backdrop-visual");
                    Assert.IsNotNull(backdrop);
                    Assert.That(backdrop.worldBound.width, Is.EqualTo(size.x).Within(1));
                    Assert.That(backdrop.worldBound.height, Is.EqualTo(size.y).Within(1));
                    Assert.IsNotNull(ui.Q(className: "settings-folio").Q(className: "folio-panel-texture"));
                    var scroll = ui.Q<ScrollView>(GameplayUiElementIds.SettingsScroll);
                    Assert.LessOrEqual(scroll.verticalScroller.highValue, 1, "Settings should fit without scrolling.");
                    Assert.GreaterOrEqual(ui.Q<DropdownField>(GameplayUiElementIds.SettingsWindow).Q(className: "unity-base-field__input").worldBound.width, 220);
                    Assert.GreaterOrEqual(ui.Q<Toggle>(GameplayUiElementIds.SettingsMouseMovement).Q(className: "unity-toggle__input").worldBound.width, 22);
                    foreach (var id in new[] { GameplayUiElementIds.SettingsBack, GameplayUiElementIds.SettingsMaster,
                        GameplayUiElementIds.SettingsResolution, GameplayUiElementIds.SettingsBindings })
                    {
                        var bounds = ui.Q(id).worldBound;
                        Assert.Greater(bounds.width, 0); Assert.GreaterOrEqual(bounds.xMin,0); Assert.GreaterOrEqual(bounds.yMin,0);
                        Assert.LessOrEqual(bounds.xMax,size.x+1); Assert.LessOrEqual(bounds.yMax,size.y+1);
                    }
                    ui.Q<Slider>(GameplayUiElementIds.SettingsMaster).value = 25;
                    Assert.AreEqual("25%",ui.Q<Label>(GameplayUiElementIds.SettingsMasterValue).text);
                    Assert.IsFalse(ui.Q<Button>(GameplayUiElementIds.SettingsApply).enabledInHierarchy);
                    UiFoundationSmokeTests.Capture(target,$"settings-r1-{size.x}x{size.y}");
                    ui.Q<DropdownField>(GameplayUiElementIds.SettingsWindow).value = "В окне";
                    Assert.IsTrue(ui.Q<Button>(GameplayUiElementIds.SettingsApply).enabledInHierarchy);
                    UiFoundationSmokeTests.Submit(ui.Q<Button>(GameplayUiElementIds.SettingsApply)); yield return null;
                    Assert.AreEqual(VideoPreviewState.Confirming,root.Settings.PreviewState);
                    Assert.IsFalse(ui.Q<Button>(GameplayUiElementIds.SettingsBack).enabledInHierarchy);
                    Assert.AreSame(ui.Q<Button>(GameplayUiElementIds.SettingsRevert),ui.panel.focusController.focusedElement);
                    UiFoundationSmokeTests.Capture(target,$"settings-r1-confirm-{size.x}x{size.y}");
                    UiFoundationSmokeTests.Submit(ui.Q<Button>(GameplayUiElementIds.SettingsRevert)); yield return null;
                    Assert.AreEqual(VideoPreviewState.Idle,root.Settings.PreviewState);
                    Assert.AreSame(ui.Q<DropdownField>(GameplayUiElementIds.SettingsWindow),ui.panel.focusController.focusedElement);
                    document.panelSettings.targetTexture = null; Object.Destroy(target); target = null;
                }
            }
            finally
            {
                if(root.ShellDocument != null)root.ShellDocument.panelSettings.targetTexture = null;
                if(target != null)Object.Destroy(target);
                root.Shutdown();
            }
        }
        [UnityTest] public IEnumerator Shake_Damage_PreservesCameraAnchorAndResetsOnPauseOffAndEnd()
        {
            ProfileSmokeScene.Load();yield return null;yield return null;
            var root=Object.FindAnyObjectByType<GameplayCompositionRoot>();
            try
            {
                CharacterSelectionSmokeDriver.StartDefault(root);yield return null;
                var player=Object.FindAnyObjectByType<PlayerCharacterRuntime>();var run=Object.FindAnyObjectByType<RunController>();
                var shake=root.GetComponent<CameraShakeRuntime>();var camera=Camera.main;var anchor=camera.transform.position;
                player.Health.TakeDamage(1);yield return null;
                Assert.Greater(shake.Offset.magnitude,0);Assert.LessOrEqual(shake.Offset.magnitude,.005f*2*camera.orthographicSize);
                Assert.AreEqual(anchor,camera.transform.position);run.Model.Pause();Assert.AreEqual(Vector2.zero,shake.Offset);
                run.Model.Resume();yield return null;Assert.AreEqual(Vector2.zero,shake.Offset);
                shake.enabled=false;player.Health.TakeDamage(1);shake.enabled=true;yield return null;
                Assert.AreEqual(Vector2.zero,shake.Offset);
                player.Health.TakeDamage(1);root.Settings.SetShake(false);Assert.AreEqual(Vector2.zero,shake.Offset);
                root.Settings.SetShake(true);player.Health.TakeDamage(player.Health.MaxHealth*10);yield return null;
                Assert.AreEqual(Vector2.zero,shake.Offset);Assert.AreEqual(RunState.Lost,run.Model.State);
            }
            finally { root.Shutdown(); }
        }
        [UnityTest] public IEnumerator Audio_Settings_RouteMasterOnceWithIndependentChannels()
        {
            ProfileSmokeScene.Load(false);yield return null;yield return null;
            var root=Object.FindAnyObjectByType<GameplayCompositionRoot>();
            var sources=root.GetComponentsInChildren<AudioSource>();Assert.AreEqual(4,sources.Length);
            root.Settings.SetAudio(.5f,.2f,.8f);
            Assert.AreEqual(.02f,sources[0].volume,.0001f);Assert.AreEqual(.08f,sources[1].volume,.0001f);
            Assert.AreEqual(.1f,sources[2].volume,.0001f);Assert.AreEqual(.4f,sources[3].volume,.0001f);
            Assert.Greater(sources[0].clip.samples,0);Assert.Greater(sources[1].clip.samples,0);
            root.Settings.SetAudio(0,1,1);foreach(var source in sources)Assert.AreEqual(0,source.volume);
            root.Shutdown();
        }
    }
}
