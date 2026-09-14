using System.Collections;
using Game.Progression;
using Game.Run;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class GameplaySmokeTests
    {
        [UnityTest]
        public IEnumerator GameplayScene_ComposesLevelsUpAndResumes()
        {
            SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
            yield return null;
            yield return null;

            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            var run = Object.FindAnyObjectByType<RunController>();
            var experience = Object.FindAnyObjectByType<PlayerExperienceRuntime>();
            var draft = Object.FindAnyObjectByType<LevelUpDraftRuntime>();
            var gameplayUi = Object.FindAnyObjectByType<GameplayUiRoot>();

            Assert.IsNotNull(root);
            Assert.IsTrue(root.IsInitialized);
            Assert.IsNotNull(gameplayUi);
            Assert.IsTrue(gameplayUi.IsInitialized);
            Assert.IsNotNull(gameplayUi.Document.panelSettings.themeStyleSheet);
            var healthBar = gameplayUi.Document.rootVisualElement.Q<ProgressBar>(GameplayUiElementIds.HealthBar);
            Assert.IsNotNull(healthBar);
            Assert.AreEqual(DisplayStyle.Flex, healthBar.resolvedStyle.display);
            Assert.Greater(healthBar.resolvedStyle.width, 0f);
            Assert.Greater(healthBar.resolvedStyle.height, 0f);
            var activeSlots = gameplayUi.Document.rootVisualElement.Q<VisualElement>(GameplayUiElementIds.ActiveSlots);
            var passiveSlots = gameplayUi.Document.rootVisualElement.Q<VisualElement>(GameplayUiElementIds.PassiveSlots);
            Assert.AreEqual(PlayerBuild.ActiveSlotCapacity, activeSlots.childCount);
            Assert.AreEqual(PlayerBuild.PassiveSlotCapacity, passiveSlots.childCount);
            var draftOverlay = gameplayUi.Document.rootVisualElement.Q<VisualElement>(GameplayUiElementIds.DraftOverlay);
            Assert.AreEqual(DisplayStyle.None, draftOverlay.style.display.value);
            Assert.AreEqual(RunState.Running, run.Model.State);
            Assert.AreEqual(2, draft.RemainingRerolls);
            Assert.AreEqual(2, draft.RemainingBanishes);

            experience.AddPickedUpExperience(5f);
            Assert.IsTrue(draft.IsDraftOpen);
            Assert.AreEqual(DisplayStyle.Flex, draftOverlay.style.display.value);
            Assert.AreEqual(
                draft.CurrentDraft.Options.Count,
                gameplayUi.Document.rootVisualElement.Q<VisualElement>(GameplayUiElementIds.DraftOptions).childCount);
            Assert.IsTrue(run.Model.IsPausedBy(RunPauseReasons.LevelUpDraft));
            DraftOption? passiveOption = null;
            for (var i = 0; i < draft.CurrentDraft.Options.Count; i++)
            {
                if (draft.CurrentDraft.Options[i].Definition.Kind != BuildEntryKind.PassiveItem)
                    continue;
                passiveOption = draft.CurrentDraft.Options[i];
                break;
            }
            Assert.IsTrue(passiveOption.HasValue);
            Assert.IsTrue(draft.Select(passiveOption.Value.Definition.Id));
            Assert.IsFalse(draft.IsDraftOpen);
            Assert.AreEqual(DisplayStyle.None, draftOverlay.style.display.value);
            Assert.AreEqual(RunState.Running, run.Model.State);
            Assert.AreEqual(1, Object.FindAnyObjectByType<PlayerPassiveSetRuntime>().PassiveCount);
            Assert.AreNotEqual("—", passiveSlots[0].Q<Label>().text);

            experience.AddPickedUpExperience(10f);
            Assert.IsTrue(draft.IsDraftOpen);
            Assert.IsTrue(draft.Reroll());
            Assert.AreEqual(1, draft.RemainingRerolls);
            var banishedId = draft.CurrentDraft.Options[0].Definition.Id;
            Assert.IsTrue(draft.Banish(banishedId));
            Assert.AreEqual(1, draft.RemainingBanishes);
            foreach (var option in draft.CurrentDraft.Options)
                Assert.AreNotEqual(banishedId, option.Definition.Id);
            Assert.IsTrue(draft.Select(draft.CurrentDraft.Options[0].Definition.Id));
            Assert.AreEqual(RunState.Running, run.Model.State);
        }
    }
}
