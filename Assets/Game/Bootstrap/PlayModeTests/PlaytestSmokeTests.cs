using System.Collections;
using System.IO;
using Game.Character;
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
    public sealed class PlaytestSmokeTests
    {
        [UnityTest]
        public IEnumerator Gameplay_LethalHitExportsLinkedPacket_AndPlaytestUiStaysCollapsed()
        {
            ProfileSmokeScene.Load();
            yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            CharacterSelectionSmokeDriver.StartDefault(root);
            yield return null;
            var session = root.Playtest as PlaytestSession;
            Assert.IsNotNull(session, "Recorder composition must succeed in Editor");
            var ui = Object.FindAnyObjectByType<GameplayUiRoot>().Document.rootVisualElement;
            Assert.AreEqual(DisplayStyle.None, ui.Q<VisualElement>(GameplayUiElementIds.DevelopmentPanel).style.display.value);
            Assert.IsNotNull(ui.Q<Button>(GameplayUiElementIds.DevelopmentPlaytestTab));
            Assert.IsNotNull(ui.Q<TextField>(GameplayUiElementIds.PlaytestNote));
            session.AddMarker("Synthetic smoke; not a human manual run");
            Object.FindAnyObjectByType<PlayerCharacterRuntime>().TakeDamage(100000);
            yield return null;
            for (var i = 0; i < 300 && !session.PendingExport.IsCompleted; i++) yield return null;
            Assert.IsTrue(session.PendingExport.IsCompleted, "Export did not complete");
            Assert.IsFalse(session.PendingExport.IsFaulted, session.PendingExport.Exception?.ToString());
            session.Tick();
            StringAssert.Contains("Saved:", session.Summary);
            var folder = Path.Combine(Application.persistentDataPath, "Playtests", session.Recorder.ReportId);
            var data = JObject.Parse(File.ReadAllText(Path.Combine(folder, "run.json")));
            Assert.AreEqual("completed", (string)data["completionReason"]);
            Assert.AreEqual("Defeat", (string)data["outcome"]["reason"]);
            Assert.Greater((double)data["appliedDamageTaken"], 0);
            Assert.AreEqual(Object.FindAnyObjectByType<RunController>().Model.RunId.ToString("N"), (string)data["runId"]);
            StringAssert.Contains(session.Recorder.ReportId, File.ReadAllText(Path.Combine(folder, "feedback.md")));
            File.AppendAllText(Path.Combine(folder, "feedback.md"), "\nSynthetic comment retained");
            session.Export(); session.Tick();
            for (var i = 0; i < 300 && !session.PendingExport.IsCompleted; i++) yield return null;
            Assert.IsTrue(session.PendingExport.IsCompleted);
            Assert.IsFalse(session.PendingExport.IsFaulted);
            StringAssert.Contains("Synthetic comment retained", File.ReadAllText(Path.Combine(folder, "feedback.md")));
            Assert.IsNotNull(Object.FindAnyObjectByType<RunController>().Model.Outcome.Contributions["draft"].Sets);
            TestContext.WriteLine("Synthetic telemetry packet: " + folder);
            var run = Object.FindAnyObjectByType<RunController>();
            var outcome = run.Model.Outcome;
            var player = Object.FindAnyObjectByType<PlayerCharacterRuntime>();
            var uiRoot = Object.FindAnyObjectByType<GameplayUiRoot>();
            root.Shutdown(); root.Shutdown();
            Assert.AreSame(outcome, run.Model.Outcome);
            Assert.IsFalse(uiRoot.IsInitialized);
            Assert.IsNull(player.Health);
            Assert.IsFalse(root.IsInitialized);
        }
    }
}
