using System.Collections;
using Game.Character;
using Game.Content;
using Game.Progression;
using Game.Run;
using Game.UI;
using Game.Telemetry;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Game.Bootstrap.PlayModeTests
{
    public sealed class CharacterSelectionSmokeTests
    {
        [UnityTest]
        public IEnumerator Selection_LockedCannotStart_AlternateLoadoutAndReinitAreClean()
        {
            SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            var run = Object.FindAnyObjectByType<RunController>();
            var player = Object.FindAnyObjectByType<PlayerCharacterRuntime>();
            var draft = Object.FindAnyObjectByType<LevelUpDraftRuntime>();
            try
            {
                Assert.IsFalse(root.IsInitialized);
                Assert.AreEqual(RunState.NotStarted, run.Model.State);
                Assert.AreEqual(0, run.Model.Elapsed);
                Assert.IsNull(player.Stats);
                var locked = root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectCard("FIXTURE-CHARACTER-STURDY"));
                Assert.IsFalse(locked.enabledSelf);
                Assert.IsFalse(root.TryStartCharacter("FIXTURE-CHARACTER-STURDY"));
                Assert.IsNull(draft.Build);

                var access = new FixtureCharacterAccessProvider(new ContentId[] { "FIXTURE-CHARACTER-AGILE", "FIXTURE-CHARACTER-STURDY" });
                root.OpenCharacterSelection(access);
                yield return null;
                var card = root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectCard("FIXTURE-CHARACTER-STURDY"));
                Assert.IsTrue(card.enabledSelf);
                Submit(card);
                Assert.AreEqual(new ContentId("FIXTURE-CHARACTER-STURDY"), root.Selection.SelectedId);
                var start = root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart);
                Submit(start);
                CharacterSelectionSmokeDriver.StartField(root);
                yield return null;
                Assert.AreEqual(RunState.Running, run.Model.State);
                Assert.IsNull(root.SelectionDocument);
                Assert.AreEqual(new ContentId("FIXTURE-CHARACTER-STURDY"), draft.Character.Id);
                Assert.AreEqual(125, player.Stats.MaxHealth);
                Assert.AreEqual(2.5f, player.Stats.MovementSpeed);
                Assert.AreEqual(1, draft.Build.ActiveCount);
                Assert.IsTrue(draft.Build.TryGetEntry("FIXTURE-SKILL-RING", out var entry));
                Assert.AreEqual(1, entry.Level);
                Assert.AreEqual(6, PlayerBuild.ActiveSlotCapacity);
                var telemetry = (PlaytestSession)root.Playtest;
                var report = JObject.Parse(telemetry.Recorder.Snapshot(null, false).Json);
                Assert.AreEqual("FIXTURE-CHARACTER-STURDY", (string)report["provenance"]["resolved"]["character"]);
                player.Stats.SetModifier("FIXTURE-OVERLAY", new CharacterStatModifier(maxHealthMultiplierBonus: .2f));
                Assert.AreEqual(150f, player.Stats.MaxHealth, .001f);
                player.Stats.RemoveModifier("FIXTURE-OVERLAY");
                Assert.AreEqual(125, player.Stats.MaxHealth);
                player.Stats.SetModifier("FIXTURE-STALE", new CharacterStatModifier(maxHealthMultiplierBonus: .5f));
                var previousRun = run.Model.RunId;
                root.Shutdown();
                root.OpenCharacterSelection();
                yield return null;
                CharacterSelectionSmokeDriver.StartDefault(root);
                yield return null;
                Assert.AreNotEqual(previousRun, run.Model.RunId);
                Assert.AreEqual(100, player.Stats.MaxHealth);
                Assert.AreEqual(3, player.Stats.MovementSpeed);
                Assert.AreEqual(0, player.Stats.ModifierCount);
                Assert.AreEqual(1, draft.Build.ActiveCount);
                Assert.IsTrue(draft.Build.TryGetEntry("FIXTURE-SKILL-BOLT", out _));
                Assert.IsFalse(draft.Build.TryGetEntry("FIXTURE-SKILL-RING", out _));
            }
            finally { root.Shutdown(); }
        }
        private static void Submit(Button button)
        {
            using var submit = NavigationSubmitEvent.GetPooled();
            submit.target = button;
            button.SendEvent(submit);
        }
    }
}
