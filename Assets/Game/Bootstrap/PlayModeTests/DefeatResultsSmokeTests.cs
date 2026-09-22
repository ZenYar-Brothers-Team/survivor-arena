using System.Collections;
using Game.Character;
using Game.Run;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Game.Bootstrap.PlayModeTests
{
    public sealed class DefeatResultsSmokeTests
    {
        [UnityTest] public IEnumerator Defeat_SavedResult_IsAboveHudAndRetryStartsFreshRun()
        {
            ProfileSmokeScene.Load();yield return null;yield return null;
            var root=Object.FindAnyObjectByType<GameplayCompositionRoot>();
            var run=Object.FindAnyObjectByType<RunController>();
            var player=Object.FindAnyObjectByType<PlayerCharacterRuntime>();
            try
            {
                CharacterSelectionSmokeDriver.StartDefault(root);yield return null;
                var previous=run.Model.RunId;var selection=run.Model.Selection;
                player.TakeDamage(player.Health.MaxHealth*10);yield return null;yield return null;
                Assert.AreEqual(RunState.Lost,run.Model.State);
                Assert.IsTrue(root.ProfileSaveTask.IsCompletedSuccessfully);
                Assert.IsTrue(root.Profile.CanStart);
                var result=root.ProfileDocument;
                var retry=result.rootVisualElement.Q<Button>(GameplayUiElementIds.MetaRetry);
                var menu=result.rootVisualElement.Q<Button>(GameplayUiElementIds.MetaSelection);
                Assert.IsTrue(retry.enabledInHierarchy);Assert.IsTrue(menu.enabledInHierarchy);
                var hud=Object.FindAnyObjectByType<GameplayUiRoot>().Document;
                // UIDocument order only sorts inside one panel. The HUD's terminal overlay must
                // be behind Results for both rendering and pointer input across independent panels.
                Assert.Greater(result.panelSettings.sortingOrder,hud.panelSettings.sortingOrder,
                    "HUD terminal overlay intercepts Results: independent panels need explicit ordering.");
                Assert.Greater(root.ShellDocument.panelSettings.sortingOrder,result.panelSettings.sortingOrder);
                Assert.Greater(retry.worldBound.width,0);
                Assert.IsNull(root.ShellDocument.rootVisualElement.panel.Pick(retry.worldBound.center),
                    "Hidden app shell intercepts the Results button.");
                var picked=result.rootVisualElement.panel.Pick(retry.worldBound.center);
                Assert.IsTrue(picked==retry||retry.Contains(picked),"Retry is not reachable by panel picking.");
                using(var submit=NavigationSubmitEvent.GetPooled()) {submit.target=retry;retry.SendEvent(submit);}
                yield return null;
                Assert.AreEqual(RunState.Running,run.Model.State);Assert.AreNotEqual(previous,run.Model.RunId);
                Assert.AreEqual(selection.CharacterId,run.Model.Selection.CharacterId);
                Assert.AreEqual(selection.FieldId,run.Model.Selection.FieldId);
                Assert.AreEqual(player.Health.MaxHealth,player.Health.CurrentHealth);
                player.TakeDamage(player.Health.MaxHealth*10);yield return null;
                menu=root.ProfileDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.MetaSelection);
                using(var submit=NavigationSubmitEvent.GetPooled()) {submit.target=menu;menu.SendEvent(submit);}
                yield return null;Assert.IsTrue(root.AtMainMenu);Assert.IsFalse(root.IsInitialized);
            }
            finally {root.Shutdown();}
        }
    }
}
