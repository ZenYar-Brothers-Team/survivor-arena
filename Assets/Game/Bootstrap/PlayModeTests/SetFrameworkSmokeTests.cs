using System;
using System.Collections;
using System.Linq;
using Game.Character;
using Game.Progression;
using Game.Run;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
namespace Game.Bootstrap.PlayModeTests
{
    public sealed class SetFrameworkSmokeTests
    {
        [UnityTest]
        public IEnumerator SimultaneousSets_QueuedChoicesPauseProjectionAndShutdown()
        {
            SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            CharacterSelectionSmokeDriver.StartDefault(root);
            yield return null;
            var draft = Object.FindAnyObjectByType<LevelUpDraftRuntime>();
            var run = Object.FindAnyObjectByType<RunController>();
            var player = Object.FindAnyObjectByType<PlayerCharacterRuntime>();
            var ui = Object.FindAnyObjectByType<GameplayUiRoot>();
            try
            {
                foreach (var recipe in root.Catalog.Sets.SelectMany(s => s.Recipe))
                {
                    var definition = root.Catalog.BuildEntries.Single(d => d.Id == recipe.Id);
                    while (!draft.Build.TryGetEntry(recipe.Id, out var entry) || entry.Level < recipe.MinimumLevel) draft.Build.Apply(definition);
                }
                // Saturate ordinary slots/levels, so only fulfilled sets remain eligible.
                foreach (var definition in root.Catalog.BuildEntries.Where(d => d.Kind != BuildEntryKind.Set))
                    while (draft.Build.IsEligible(definition)) draft.Build.Apply(definition);
                for (var i = 0; i < 4; i++)
                    Assert.IsTrue(draft.RequestBook(Guid.NewGuid(), run.Model.RunId, new Game.Content.ContentId("FIXTURE-BOOK")));
                Assert.AreEqual(4, draft.PendingDraftCount);
                Assert.AreEqual(RunState.Paused, run.Model.State);
                for (var i = 0; i < 4; i++)
                {
                    Assert.AreEqual(BuildEntryKind.Set, draft.CurrentDraft.Options[0].Definition.Kind);
                    Assert.IsTrue(draft.Select(draft.CurrentDraft.Options[0].Definition.Id));
                    Assert.AreEqual(i + 1, draft.Sets.Count);
                    Assert.AreEqual(3 - i, draft.PendingDraftCount);
                    if (i < 3) Assert.AreEqual(RunState.Paused, run.Model.State);
                }
                Assert.AreEqual(RunState.Running, run.Model.State);
                Assert.Greater(player.Stats.MaxHealth, player.Stats.BaseStats.MaxHealth);
                yield return null;
                Assert.AreEqual(4, ui.Document.rootVisualElement.Q(GameplayUiElementIds.Sets).childCount);
                StringAssert.Contains("SET ACQUIRED", ui.Document.rootVisualElement.Q<Label>(GameplayUiElementIds.Notification).text);
                run.Model.Pause();
                var observations = draft.Sets.DevelopmentObservation;
                yield return new WaitForSecondsRealtime(.1f);
                Assert.AreEqual(observations, draft.Sets.DevelopmentObservation);
                StringAssert.Contains("proc", observations);
                // Deterministically exercise producer-first teardown, independent of Unity object order.
                player.Shutdown();
                Assert.IsFalse(root.IsInitialized);
                Assert.IsFalse(ui.IsInitialized);
                Assert.IsNull(player.Health);
                Assert.IsNull(draft.Sets);
                root.Shutdown();
            }
            finally { if (root != null) root.Shutdown(); }
        }
    }
}
