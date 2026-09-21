using System.Collections;
using System.Linq;
using Game.Character;
using Game.Combat;
using Game.Enemy;
using Game.Presentation;
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
            var player = Object.FindAnyObjectByType<PlayerCharacterRuntime>();
            var presentation = Object.FindAnyObjectByType<SpritePresentationRuntime>();

            Assert.IsNotNull(root);
            Assert.IsTrue(root.IsInitialized);
            Assert.IsNotNull(gameplayUi);
            Assert.IsNotNull(player);
            Assert.IsNotNull(presentation);
            Assert.IsTrue(gameplayUi.IsInitialized);
            var timer = gameplayUi.Document.rootVisualElement.Q<Label>(GameplayUiElementIds.TimerLabel);
            Assert.AreEqual("00:00", timer.text);
            Assert.IsTrue(presentation.IsInitialized);
            var developmentToggle = gameplayUi.Document.rootVisualElement.Q<Button>(
                GameplayUiElementIds.DevelopmentToggleButton);
            var developmentPanel = gameplayUi.Document.rootVisualElement.Q<VisualElement>(
                GameplayUiElementIds.DevelopmentPanel);
            Assert.AreEqual(DisplayStyle.Flex, developmentToggle.style.display.value);
            Assert.AreEqual(DisplayStyle.None, developmentPanel.style.display.value);
            var waveLabel = gameplayUi.Document.rootVisualElement.Q<Label>(GameplayUiElementIds.WaveLabel);
            StringAssert.StartsWith("WAVE 1/", waveLabel.text);
            StringAssert.Contains("ORDINARY", waveLabel.text);
            var waveObservation = gameplayUi.Document.rootVisualElement.Q<Label>(GameplayUiElementIds.WaveObservation);
            StringAssert.Contains("Spawn every", waveObservation.text);
            var bodyRenderer = GameObject.Find("BodyRoot").GetComponent<SpriteRenderer>();
            Assert.AreEqual("fixture-character-agile-body", bodyRenderer.sprite.name);
            presentation.SetPreviewMotion(SpritePresentationPreviewMotion.Left);
            yield return null;
            Assert.IsTrue(bodyRenderer.flipX);
            var pausedPosition = bodyRenderer.transform.localPosition;
            var pausedRotation = bodyRenderer.transform.localRotation;
            var pausedScale = bodyRenderer.transform.localScale;
            run.TogglePause();
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.AreEqual(pausedPosition, bodyRenderer.transform.localPosition);
            Assert.AreEqual(pausedRotation, bodyRenderer.transform.localRotation);
            Assert.AreEqual(pausedScale, bodyRenderer.transform.localScale);
            run.TogglePause();
            player.TakeDamage(1f);
            Assert.AreNotEqual(Color.white, bodyRenderer.color);
            presentation.ResetPresentation();
            Assert.AreEqual(SpritePresentationPreviewMotion.Live, presentation.PreviewMotion);
            Assert.IsFalse(bodyRenderer.flipX);
            Assert.IsNotNull(gameplayUi.Document.panelSettings.themeStyleSheet);
            var healthBar = gameplayUi.Document.rootVisualElement.Q<ProgressBar>(GameplayUiElementIds.HealthBar);
            Assert.IsNotNull(healthBar);
            Assert.AreEqual(DisplayStyle.Flex, healthBar.resolvedStyle.display);
            Assert.Greater(healthBar.resolvedStyle.width, 0f);
            Assert.Greater(healthBar.resolvedStyle.height, 0f);
            var activeSlots = gameplayUi.Document.rootVisualElement.Q<VisualElement>(GameplayUiElementIds.ActiveSlots);
            var passiveSlots = gameplayUi.Document.rootVisualElement.Q<VisualElement>(GameplayUiElementIds.PassiveSlots);
            var sets = gameplayUi.Document.rootVisualElement.Q<VisualElement>(GameplayUiElementIds.Sets);
            var setRecipeProgress = gameplayUi.Document.rootVisualElement.Q<VisualElement>(GameplayUiElementIds.SetRecipeProgress);
            var characterSelection = gameplayUi.Document.rootVisualElement.Q<VisualElement>(GameplayUiElementIds.CharacterSelection);
            Assert.AreEqual(PlayerBuild.ActiveSlotCapacity, activeSlots.childCount);
            Assert.AreEqual(PlayerBuild.PassiveSlotCapacity, passiveSlots.childCount);
            Assert.AreEqual(0, sets.childCount);
            Assert.AreEqual(root.Catalog.Sets.Count, setRecipeProgress.childCount);
            Assert.AreEqual(root.Catalog.Characters.UnlockedCharacters.Count, characterSelection.childCount);
            Assert.AreEqual(root.Catalog.Characters.UnlockedCharacters[0].Id, draft.Character.Id);
            var draftOverlay = gameplayUi.Document.rootVisualElement.Q<VisualElement>(GameplayUiElementIds.DraftOverlay);
            Assert.AreEqual(DisplayStyle.None, draftOverlay.style.display.value);
            Assert.AreEqual(RunState.Running, run.Model.State);
            Assert.AreEqual(2, draft.RemainingRerolls);
            Assert.AreEqual(2, draft.RemainingBanishes);
            // Exercise the real PlayerMover writer and paused controls through the composed scene.
            var beforeKnockback = player.transform.position;
            player.ApplyDamage(new CombatDamageRequest(default, 0f, new CombatControlProfile(1f, 0.5f), 1f, 0f));
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.Greater(player.transform.position.x, beforeKnockback.x);
            Assert.Greater(player.GetComponent<Rigidbody2D>().linearVelocity.x, 0f);
            run.TogglePause();
            var controlTime = player.Controls.KnockbackRemaining;
            yield return new WaitForSecondsRealtime(0.05f);
            Assert.AreEqual(controlTime, player.Controls.KnockbackRemaining);
            run.TogglePause();
            player.Controls.Reset();
            player.SetModifier("smoke-low-hp", new CharacterStatModifier(lowHealthDamageMaxBonus: 0.7f));
            player.TakeDamage(45f);
            yield return null;
            var statsObservation = gameplayUi.Document.rootVisualElement.Q<Label>(GameplayUiElementIds.StatsObservation);
            StringAssert.Contains("Action speed", statsObservation.text);
            StringAssert.Contains("low-HP damage x", statsObservation.text);
            StringAssert.Contains("Knockback remaining", statsObservation.text);
            Assert.Greater(player.Stats.LowHealthDamageMultiplier, 1f);
            player.Heal(100f);
            Assert.AreEqual(1f, player.Stats.LowHealthDamageMultiplier);
            player.RemoveModifier("smoke-low-hp");

            var pickup = ExperienceDropFactory.Spawn(0.25f,
                (Vector2)player.transform.position + Vector2.right * experience.PickupRadius * 1.5f,
                10f, experience, run, pool: experience.DropPool);
            yield return null;
            Assert.IsFalse(pickup.IsConsumed);
            player.SetModifier("smoke-radius", new CharacterStatModifier(pickupRadiusMultiplierBonus: 1f));
            yield return null;
            yield return null;
            Assert.IsTrue(pickup.IsConsumed);
            Assert.AreEqual(0.25f, experience.CollectedBase);
            player.RemoveModifier("smoke-radius");
            var xpObservation = gameplayUi.Document.rootVisualElement.Q<Label>(GameplayUiElementIds.ExperienceObservation);
            StringAssert.Contains("XP collected", xpObservation.text);
            StringAssert.Contains("recovered", xpObservation.text);

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
            var xpBeforeBook = experience.Progression.CurrentExperience;
            var levelBeforeBook = experience.Progression.Level;
            Assert.IsTrue(draft.RequestBook(System.Guid.NewGuid(), run.Model.RunId, new Game.Content.ContentId("FIXTURE-BOOK")));
            Assert.AreEqual("TRAVELER BOOK", gameplayUi.Document.rootVisualElement.Q<Label>(GameplayUiElementIds.DraftHeading).text);
            experience.AddInterventionExperience(10f);
            Assert.IsNotEmpty(gameplayUi.Document.rootVisualElement.Q<Label>(GameplayUiElementIds.DraftQueue).text);
            var bookRevision = draft.Revision;
            var bookOption = draft.CurrentDraft.Options[0].Definition.Id;
            Assert.IsTrue(draft.Select(bookOption, bookRevision));
            Assert.IsFalse(draft.Select(bookOption, bookRevision));
            Assert.AreEqual(DraftOrigin.LevelUp, draft.CurrentRequest.Origin);
            Assert.IsTrue(draft.Select(draft.CurrentDraft.Options[0].Definition.Id, draft.Revision));
            Assert.AreEqual(levelBeforeBook + 1, experience.Progression.Level);
            Assert.AreEqual(xpBeforeBook, experience.Progression.CurrentExperience, 0.0001f);
            Assert.GreaterOrEqual(Object.FindAnyObjectByType<PlayerPassiveSetRuntime>().PassiveCount, 1);
            Assert.AreNotEqual("—", passiveSlots[0].Q<Label>().text);
            Assert.IsNotEmpty(passiveSlots[0].Q<Label>().tooltip);

            experience.AddPickedUpExperience(15f);
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
            // IP-09 fixture state: acquire a dynamic passive through the existing build/runtime,
            // then let an ordinary selection publish the updated build to the real UI.
            var lowHealth = root.Catalog.Passives.Single(x => x.Id.ToString() == "FIXTURE-PASSIVE-LOW-HEALTH");
            draft.Build.Apply(lowHealth);
            Object.FindAnyObjectByType<PlayerPassiveSetRuntime>().Initialize(player, draft, root.Catalog.Passives);
            Assert.IsTrue(draft.RequestBook(System.Guid.NewGuid(), run.Model.RunId, new Game.Content.ContentId("FIXTURE-BOOK")));
            Assert.IsTrue(draft.Select(draft.CurrentDraft.Options[0].Definition.Id));
            player.Heal(player.Health.MaxHealth);
            var fullHealthDetail = passiveSlots.Children().OfType<Label>().Single(x => x.text.StartsWith(lowHealth.DisplayName)).tooltip;
            StringAssert.Contains("Current low-HP damage: x1", fullHealthDetail);
            player.TakeDamage(player.Health.MaxHealth * 0.9f / player.Stats.IncomingDamageMultiplier);
            var lowHealthDetail = passiveSlots.Children().OfType<Label>().Single(x => x.text.StartsWith(lowHealth.DisplayName)).tooltip;
            Assert.AreNotEqual(fullHealthDetail, lowHealthDetail);
            player.Heal(player.Health.MaxHealth);
            Assert.AreEqual(fullHealthDetail, passiveSlots.Children().OfType<Label>().Single(x => x.text.StartsWith(lowHealth.DisplayName)).tooltip);

            var enemySpawner = Object.FindAnyObjectByType<ContinuousFixtureEnemySpawner>();
            enemySpawner.Tick(run.Model.Elapsed, enemySpawner.Director.CurrentPhase.SpawnIntervalSeconds, true);
            var spawnedEnemy = enemySpawner.GetComponentInChildren<EnemyRuntime>();
            Assert.IsNotNull(spawnedEnemy);
            spawnedEnemy.TakeDamage(spawnedEnemy.Definition.MaxHealth);
            run.Model.Tick(run.Model.Duration);
            yield return null;
            Assert.AreEqual(RunState.Won, run.Model.State);
            Assert.AreEqual(RunCompletionReason.Victory, run.Model.Outcome.Reason);
            Assert.IsTrue(run.Model.Outcome.Contributions.ContainsKey("ordinary-enemy-kills"));
            Assert.GreaterOrEqual(run.Model.Outcome.Contributions["ordinary-enemy-kills"].Kills.Value, 1);
            Assert.AreEqual(experience.TotalAwarded, run.Model.Outcome.Contributions[experience.Key].ExperienceTotals.TotalAwarded);
            Assert.GreaterOrEqual(run.Model.Outcome.Contributions["draft"].DraftTotals.Selections, 4);
            Assert.IsNotNull(enemySpawner.LastLifeEvent);
            Assert.AreNotEqual(System.Guid.Empty, enemySpawner.LastLifeEvent.LifeId);
            Assert.AreEqual("15:00", timer.text);
            var completedRun = run.Model.Outcome;
            yield return null;
            Assert.AreSame(completedRun, run.Model.Outcome);
            Assert.AreEqual(run.Model.Duration, run.Model.Elapsed);
        }
    }
}
