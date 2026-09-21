using System.Collections;
using System.Linq;
using Game.Character;
using Game.Content;
using Game.Enemy;
using Game.Pickup;
using Game.Progression;
using Game.Run;
using Game.Telemetry;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Game.Bootstrap.PlayModeTests
{
    public sealed class WorldPickupSmokeTests
    {
        [UnityTest]
        public IEnumerator WorldPickups_ContactHealingBookPauseHudTelemetryAndReinit()
        {
            SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            CharacterSelectionSmokeDriver.StartDefault(root); yield return null;
            var player = Object.FindAnyObjectByType<PlayerCharacterRuntime>();
            var run = Object.FindAnyObjectByType<RunController>();
            var draft = Object.FindAnyObjectByType<LevelUpDraftRuntime>();
            var xp = Object.FindAnyObjectByType<PlayerExperienceRuntime>();
            var ui = Object.FindAnyObjectByType<GameplayUiRoot>().Document.rootVisualElement;
            try
            {
                var rightEdge = GameObject.Find("Wall_Right").GetComponent<BoxCollider2D>().bounds.min.x;
                var outside = root.Pickups.Spawn(root.Catalog.Pickups.Potion, new Vector2(rightEdge + 100, 0));
                Assert.Less(outside.transform.position.x, rightEdge);
                var inside = root.Pickups.Spawn(root.Catalog.Pickups.Book, new Vector2(3, 0));
                Assert.Greater(Vector2.Distance(inside.transform.position, new Vector2(3, 0)), .5f);
                Assert.AreNotEqual(outside.GetComponentInChildren<TextMesh>().text, inside.GetComponentInChildren<TextMesh>().text);
                Assert.AreEqual("VisualRoot", outside.GetComponentInChildren<TextMesh>().gameObject.name);
                Assert.IsNull(outside.GetComponent<Collider2D>());
                player.Health.TakeDamage(30);
                var before = player.Health.CurrentHealth;
                var potion = root.Pickups.Spawn(root.Catalog.Pickups.Potion, player.transform.position);
                var potionId = potion.Life.Identity.DropId;
                yield return null; yield return null;
                Assert.AreEqual(before + 20, player.Health.CurrentHealth, .01f);
                Assert.IsFalse(root.Pickups.TryCollect(potion, potionId));
                StringAssert.Contains("Potion +20 HP", ui.Q<Label>(GameplayUiElementIds.PickupFeedback).text);
                Assert.Greater(ui.Q<Label>(GameplayUiElementIds.PickupFeedback).resolvedStyle.height, 0);
                var level = xp.Progression.Level;
                var book = root.Pickups.Spawn(root.Catalog.Pickups.Book, player.transform.position);
                var bookId = book.Life.Identity.DropId;
                var deferred = root.Pickups.Spawn(root.Catalog.Pickups.Potion, player.transform.position);
                var deferredId = deferred.Life.Identity.DropId;
                yield return null;
                Assert.IsTrue(draft.IsDraftOpen); Assert.AreEqual(bookId, draft.CurrentRequest.PickupId);
                StringAssert.Contains("BOOK", ui.Q<Label>(GameplayUiElementIds.DraftHeading).text);
                Assert.AreEqual(RunState.Paused, run.Model.State); Assert.AreEqual(level, xp.Progression.Level);
                Assert.IsFalse(root.Pickups.TryCollect(deferred, deferredId));
                var collected = root.Pickups.Snapshot.Collected;
                yield return new WaitForSecondsRealtime(.05f);
                Assert.AreEqual(collected, root.Pickups.Snapshot.Collected);
                draft.Select(draft.CurrentDraft.Options[0].Definition.Id); yield return null;
                Assert.AreEqual(collected + 1, root.Pickups.Snapshot.Collected);
                var report = ((PlaytestSession)root.Playtest).Recorder.Snapshot(null, false).Json;
                StringAssert.Contains("pickup.Potion.Collected", report); StringAssert.Contains("pickup.Book.Collected", report);
                StringAssert.Contains(bookId.ToString("N"), report);
                var oldRun = run.Model.RunId;
                root.Shutdown(); Assert.AreEqual(0, root.Pickups.Snapshot.Active);
                root.OpenCharacterSelection(); CharacterSelectionSmokeDriver.StartDefault(root); yield return null;
                Assert.AreNotEqual(oldRun, run.Model.RunId); Assert.AreEqual(0, root.Pickups.Snapshot.Spawned);
                Assert.IsFalse(root.Pickups.TryCollect(deferred, deferredId));
            }
            finally { root.Shutdown(); }
        }
        [UnityTest]
        public IEnumerator WorldBook_SetBackfillThenEmptyCurrency_AndFullHpPotionProcsGuard()
        {
            SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            CharacterSelectionSmokeDriver.StartDefault(root); yield return null;
            var player = Object.FindAnyObjectByType<PlayerCharacterRuntime>();
            var draft = Object.FindAnyObjectByType<LevelUpDraftRuntime>();
            var run = Object.FindAnyObjectByType<RunController>();
            try
            {
                foreach (var recipe in root.Catalog.Sets.SelectMany(set => set.Recipe))
                {
                    var definition = root.Catalog.BuildEntries.Single(entry => entry.Id == recipe.Id);
                    while (!draft.Build.TryGetEntry(recipe.Id, out var entry) || entry.Level < recipe.MinimumLevel) draft.Build.Apply(definition);
                }
                foreach (var definition in root.Catalog.BuildEntries.Where(entry => entry.Kind != BuildEntryKind.Set))
                    while (draft.Build.IsEligible(definition)) draft.Build.Apply(definition);
                for (var i = 0; i < root.Catalog.Sets.Count; i++)
                {
                    root.Pickups.Spawn(root.Catalog.Pickups.Book, player.transform.position); root.Pickups.Tick(0);
                    Assert.IsTrue(draft.IsDraftOpen); Assert.AreEqual(0, draft.BookCurrency);
                    Assert.AreEqual(BuildEntryKind.Set, draft.CurrentDraft.Options[0].Definition.Kind);
                    Assert.IsTrue(draft.Select(draft.CurrentDraft.Options[0].Definition.Id));
                }
                var book = root.Pickups.Spawn(root.Catalog.Pickups.Book, player.transform.position); var id = book.Life.Identity.DropId;
                root.Pickups.Tick(0); Assert.IsFalse(draft.IsDraftOpen);
                Assert.AreEqual(root.Catalog.RunSetup.Draft.EmptyBookCurrency, draft.BookCurrency);
                Assert.IsFalse(root.Pickups.TryCollect(book, id)); Assert.AreEqual(RunState.Running, run.Model.State);
                player.Heal(player.Health.MaxHealth);
                var potion = root.Pickups.Spawn(root.Catalog.Pickups.Potion, player.transform.position); root.Pickups.Tick(0);
                StringAssert.Contains("FIXTURE-SET-GUARD: proc 1", draft.Sets.DevelopmentObservation);
                Assert.AreEqual(player.Health.MaxHealth, player.Health.CurrentHealth);
            }
            finally { root.Shutdown(); }
        }
    }
}
