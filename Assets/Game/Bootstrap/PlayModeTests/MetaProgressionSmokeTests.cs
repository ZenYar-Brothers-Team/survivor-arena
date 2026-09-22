using System.Collections;
using Game.Character;
using Game.Progression;
using Game.Run;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Game.Bootstrap.PlayModeTests
{
    public sealed class MetaProgressionSmokeTests
    {
        [UnityTest] public IEnumerator Result_Purchase_Retry_AppliesUpgradeAndKeepsOneReward()
        {
            ProfileSmokeScene.Load();yield return null;yield return null;
            var root=Object.FindAnyObjectByType<GameplayCompositionRoot>();
            var run=Object.FindAnyObjectByType<RunController>();var player=Object.FindAnyObjectByType<PlayerCharacterRuntime>();
            var xp=Object.FindAnyObjectByType<PlayerExperienceRuntime>();
            try
            {
                CharacterSelectionSmokeDriver.StartDefault(root);yield return null;
                var previous=run.Model.RunId;var character=run.Model.Selection.CharacterId;var field=run.Model.Selection.FieldId;
                var baseHp=player.Stats.MaxHealth;
                xp.AddInterventionExperience(300);var level=xp.Progression.Level;
                run.Model.Pause();
                Submit(root.ShellDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.ShellQuit));
                yield return null;
                Assert.AreEqual(5L*level,root.Profile.Currency);Assert.IsTrue(root.Profile.CanStart);
                StringAssert.Contains("Level reward",root.ProfileDocument.rootVisualElement.Q<Label>(GameplayUiElementIds.MetaSummary).text);
                Submit(root.ProfileDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.MetaRetry));yield return null;
                Assert.AreNotEqual(previous,run.Model.RunId);Assert.AreEqual(character,run.Model.Selection.CharacterId);Assert.AreEqual(field,run.Model.Selection.FieldId);
                run.Model.Stop();yield return null;
                Submit(root.ProfileDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.MetaSelection));yield return null;
                Submit(root.ShellDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.ShellMeta));yield return null;
                var buy=root.ProfileDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.MetaBuy("META-001"));Assert.IsTrue(buy.enabledSelf);Submit(buy);yield return null;
                Assert.AreEqual(1,root.Profile.Level("META-001"));
                var balance=root.Profile.Currency;
                Submit(root.ProfileDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.MetaClose));yield return null;
                root.Play(); CharacterSelectionSmokeDriver.StartDefault(root);yield return null;
                Assert.AreNotEqual(previous,run.Model.RunId);Assert.AreEqual(character,run.Model.Selection.CharacterId);Assert.AreEqual(field,run.Model.Selection.FieldId);
                Assert.AreEqual(balance,root.Profile.Currency);Assert.AreEqual(baseHp*1.05f,player.Stats.MaxHealth,.001f);
                Assert.AreEqual(player.Stats.MaxHealth,player.Health.CurrentHealth,.001f);
                Assert.AreEqual(RunState.Running,run.Model.State);
                Assert.IsNull(root.SelectionDocument);Assert.IsNull(root.FieldSelectionDocument);
            }
            finally { root.Shutdown(); }
        }
        [UnityTest] public IEnumerator ProfileAndSelectionViewDestroyedFirst_ShutdownIsSafe()
        {
            ProfileSmokeScene.Load();yield return null;yield return null;
            var root=Object.FindAnyObjectByType<GameplayCompositionRoot>();
            Object.DestroyImmediate(root.ProfileDocument.gameObject);
            Object.DestroyImmediate(root.SelectionDocument.gameObject);
            root.Shutdown();
            Object.DestroyImmediate(root.gameObject);
            yield return null;
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator FieldViewDestroyedFirst_ShutdownIsSafe()
        {
            ProfileSmokeScene.Load();yield return null;yield return null;
            var root=Object.FindAnyObjectByType<GameplayCompositionRoot>();
            Submit(root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart));
            Object.DestroyImmediate(root.FieldSelectionDocument.gameObject);
            root.Shutdown();
            yield return null;
            LogAssert.NoUnexpectedReceived();
        }
        private static void Submit(Button button)
        { Assert.IsNotNull(button);Assert.IsTrue(button.enabledSelf);using var submit=NavigationSubmitEvent.GetPooled();submit.target=button;button.SendEvent(submit); }
    }
}
