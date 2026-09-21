using System.Collections;
using System.Linq;
using Game.Character;
using Game.Content;
using Game.Enemy;
using Game.Field;
using Game.Run;
using Game.Telemetry;
using Game.UI;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Game.Bootstrap.PlayModeTests
{
    public sealed class FieldSelectionSmokeTests
    {
        [UnityTest]
        public IEnumerator Selection_CancelBeforeLaunch_DoesNotEnableUninitializedGameplay()
        {
            ProfileSmokeScene.Load();
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            var run = Object.FindAnyObjectByType<RunController>();
            try
            {
                Submit(root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart));
                root.Shutdown();
                yield return null; yield return null;
                Assert.IsFalse(root.IsInitialized);
                Assert.IsNull(root.FieldSelectionDocument);
                Assert.IsFalse(run.IsInitialized);
                Assert.IsFalse(run.enabled);
                root.OpenCharacterSelection();
                CharacterSelectionSmokeDriver.StartDefault(root);
                yield return null;
                Assert.AreEqual(RunState.Running, run.Model.State);
                Assert.IsTrue(run.enabled);
            }
            finally { root.Shutdown(); }
        }
        [UnityTest]
        public IEnumerator Selection_BackLockedAlternateFieldAndReinitialization_UseFreshConfiguration()
        {
            ProfileSmokeScene.Load();
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            var run = Object.FindAnyObjectByType<RunController>();
            var player = Object.FindAnyObjectByType<PlayerCharacterRuntime>();
            var spawner = Object.FindAnyObjectByType<ContinuousFixtureEnemySpawner>();
            var baseline = EnemyRegistry.Count;
            try
            {
                Submit(root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart));
                yield return null;
                Assert.IsFalse(root.IsInitialized);
                Assert.AreEqual(RunState.NotStarted, run.Model.State);
                Assert.AreEqual(0, run.Model.Elapsed);
                var ui = root.FieldSelectionDocument.rootVisualElement;
                Assert.IsFalse(ui.Q<Button>(GameplayUiElementIds.FieldSelectCard("FIXTURE-FIELD-FOCUSED")).enabledSelf);
                Assert.IsFalse(root.TryStartField("FIXTURE-FIELD-FOCUSED"));
                Assert.IsNull(player.Stats);
                Assert.Greater(ui.Q<Button>(GameplayUiElementIds.FieldSelectStart).resolvedStyle.width, 0);
                Assert.Greater(ui.Q<Label>(GameplayUiElementIds.FieldSelectThumbnail).resolvedStyle.height, 0);
                Submit(ui.Q<Button>(GameplayUiElementIds.FieldSelectBack));
                Assert.AreEqual(root.Catalog.RunSetup.StartingCharacterId, root.Selection.SelectedId);

                var fields = root.Catalog.Fields.Roster.AllFields;
                root.OpenCharacterSelection(fieldAccess: new FixtureFieldAccessProvider(fields, fields.Select(field => field.Id)));
                Submit(root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart));
                Submit(root.FieldSelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.FieldSelectCard("FIXTURE-FIELD-FOCUSED")));
                Submit(root.FieldSelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.FieldSelectBack));
                Submit(root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart));
                Assert.AreEqual(new ContentId("FIXTURE-FIELD-FOCUSED"), root.FieldSelection.SelectedId);
                CharacterSelectionSmokeDriver.StartField(root);
                yield return null;
                Assert.AreEqual(new ContentId("FIXTURE-FIELD-FOCUSED"), run.Model.Selection.FieldId);
                Assert.AreEqual(new ContentId("FIXTURE-WAVE-FOCUSED"), spawner.Director.Timeline.Id);
                Assert.AreEqual(1, root.FieldConfiguration.Enemies.Count);
                Assert.AreEqual(1, root.FieldConfiguration.Bosses.Count);
                Assert.IsNull(root.FieldSelectionDocument);
                var report = JObject.Parse(((PlaytestSession)root.Playtest).Recorder.Snapshot(null, false).Json);
                Assert.AreEqual("FIXTURE-FIELD-FOCUSED", (string)report["provenance"]["resolved"]["field"]);
                Assert.AreEqual("FIXTURE-WAVE-FOCUSED", (string)report["provenance"]["resolved"]["timeline"]);
                var oldRun = run.Model;
                var oldDirector = spawner.Director;
                oldDirector.Advance(840, 840, true, 1000);
                Assert.IsNotNull(root.BossEncounters.FinalBoss);
                root.Shutdown();
                Assert.AreEqual(new ContentId("FIXTURE-FIELD-FOCUSED"), oldRun.Outcome.Selection.FieldId);
                Assert.AreEqual(baseline, EnemyRegistry.Count);
                player.transform.position = new Vector3(8, 8, 0);
                root.OpenCharacterSelection();
                CharacterSelectionSmokeDriver.StartDefault(root);
                yield return null;
                Assert.AreNotEqual(oldRun.RunId, run.Model.RunId);
                Assert.AreEqual(new ContentId("FIXTURE-FIELD-OPEN"), run.Model.Selection.FieldId);
                Assert.AreEqual(new ContentId("FIXTURE-WAVE-TIMELINE"), spawner.Director.Timeline.Id);
                Assert.AreEqual(2, root.FieldConfiguration.Bosses.Count);
                Assert.IsNull(root.BossEncounters.FinalBoss);
                Assert.Less(Vector2.Distance(player.transform.position, GameObject.Find("SpawnPoint").transform.position), .01f);
                oldDirector.Advance(840, 0, true, 1000);
                Assert.IsNull(root.BossEncounters.FinalBoss);
                spawner.Director.Advance(840, 840, true, 1000);
                Assert.IsNotNull(root.BossEncounters.FinalBoss);
            }
            finally { root.Shutdown(); }
            Assert.AreEqual(baseline, EnemyRegistry.Count);
        }
        private static void Submit(Button button)
        {
            Assert.IsNotNull(button);
            using var submit = NavigationSubmitEvent.GetPooled();
            submit.target = button; button.SendEvent(submit);
        }
    }
}
