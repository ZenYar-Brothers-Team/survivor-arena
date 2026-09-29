using System;
using System.Collections;
using System.IO;
using Game.Automation;
using Game.Bootstrap.Automation;
using Game.Meta;
using Game.Content;
using Game.Progression;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Newtonsoft.Json.Linq;
using Object = UnityEngine.Object;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class AutomationRunHostTests
    {
        private GameplayCompositionRoot _root;
        private AutomationRunHost _host;
        private RunController _run;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            ProductionSmokeScene.Load(TestContext.CurrentContext.Test.Name.Contains("SaveFailure")
                ? new FailAfterFirstProfileStore() : null, openSelection: false);
            yield return null;
            yield return null;
            _root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            Assert.IsTrue(_root.AtMainMenu);
            _run = Object.FindAnyObjectByType<RunController>();
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../scripts/balance/examples/fresh.json"));
            var data = JObject.Parse(File.ReadAllText(path));
            if (TestContext.CurrentContext.Test.Name.Contains("ManualPauseTimeout"))
                data["transitionTimeoutSeconds"] = 0.2f;
            var outputRoot = Path.Combine(Application.temporaryCachePath, "automation-host-" + Guid.NewGuid().ToString("N"));
            var config = new ExperimentConfigLoader(MetaCatalog.Load(), new[] { "FIELD-001" }, outputRoot)
                .Parse(data.ToString(), Path.GetDirectoryName(path));
            _host = _root.gameObject.AddComponent<AutomationRunHost>();
            _host.Initialize(_root, config);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_host != null) Object.Destroy(_host);
            if (_root != null && _root.IsInitialized) _root.Shutdown();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Host_FixtureVictory_UsesAuthoritativeOutcomeAndSavedReceipt()
        {
            for (var i = 0; i < 20 && _host.State != AutomationRunState.Running; i++) yield return null;
            Assert.AreEqual(AutomationRunState.Running, _host.State, _host.TerminalReason);
            // Fixture-only time jump checks model/result wiring; the pilot uses naturally elapsed time.
            _run.Model.Tick(_run.Model.Duration);
            for (var i = 0; i < 40 && !_host.IsFinished; i++) yield return null;
            Assert.AreEqual(AutomationRunState.Completed, _host.State, _host.TerminalReason);
            Assert.AreEqual(RunCompletionReason.Victory, _host.Outcome.Reason);
            Assert.AreEqual(_host.Outcome.RunId.ToString(), _host.Receipt.RunId);
            Assert.Greater(_host.Receipt.Total, 0);
        }

        [UnityTest]
        public IEnumerator Host_FixtureLoss_IsNotInventedByBot()
        {
            for (var i = 0; i < 20 && _host.State != AutomationRunState.Running; i++) yield return null;
            Assert.AreEqual(AutomationRunState.Running, _host.State, _host.TerminalReason);
            _run.Model.Kill();
            for (var i = 0; i < 40 && !_host.IsFinished; i++) yield return null;
            Assert.AreEqual(AutomationRunState.Completed, _host.State, _host.TerminalReason);
            Assert.AreEqual(RunCompletionReason.Defeat, _host.Outcome.Reason);
            Assert.IsNotNull(_host.Receipt);
        }

        [UnityTest]
        public IEnumerator Host_RequestedStop_IsIncompleteNotLoss()
        {
            for (var i = 0; i < 20 && _host.State != AutomationRunState.Running; i++) yield return null;
            _host.RequestStop("testStop");
            for (var i = 0; i < 40 && !_host.IsFinished; i++) yield return null;
            Assert.AreEqual(AutomationRunState.Stopped, _host.State, _host.TerminalReason);
            Assert.AreEqual(RunCompletionReason.Aborted, _host.Outcome.Reason);
            Assert.AreEqual("testStop", _host.TerminalReason);
        }

        [UnityTest]
        public IEnumerator Host_QueuedBookAndStaleRevision_ResolvesOnlyCurrentLegalOptions()
        {
            for (var i = 0; i < 20 && _host.State != AutomationRunState.Running; i++) yield return null;
            var draft = Object.FindAnyObjectByType<LevelUpDraftRuntime>();
            Assert.IsTrue(draft.RequestBook(Guid.NewGuid(), _run.Model.RunId, new ContentId("PICKUP-002")));
            Assert.IsTrue(draft.RequestBook(Guid.NewGuid(), _run.Model.RunId, new ContentId("PICKUP-002")));
            var oldRevision = draft.Revision;
            var staleId = draft.CurrentDraft.Options[0].Definition.Id;
            Assert.IsTrue(draft.Reroll(oldRevision));
            Assert.IsFalse(draft.Select(staleId, oldRevision));
            for (var i = 0; i < 100 && (draft.IsDraftOpen || draft.PendingDraftCount > 0); i++) yield return null;
            Assert.IsFalse(draft.IsDraftOpen);
            Assert.AreEqual(0, draft.PendingDraftCount);
            Assert.GreaterOrEqual(draft.Totals.Selections, 2);
            Assert.AreEqual(RunState.Running, _run.Model.State);
        }

        [UnityTest]
        public IEnumerator Host_ManualPauseTimeout_StopsIncompleteWithoutResumingPause()
        {
            for (var i = 0; i < 20 && _host.State != AutomationRunState.Running; i++) yield return null;
            Assert.IsTrue(_run.Model.RequestPause(RunPauseReasons.Manual));
            yield return new WaitForSecondsRealtime(0.35f);
            for (var i = 0; i < 40 && !_host.IsFinished; i++) yield return null;
            Assert.AreEqual(AutomationRunState.Stopped, _host.State, _host.TerminalReason);
            Assert.AreEqual("manualPauseTimeout", _host.TerminalReason);
            Assert.AreEqual(RunCompletionReason.Aborted, _host.Outcome.Reason);
        }

        [UnityTest]
        public IEnumerator Host_SaveFailure_FailsRatherThanStartingAnotherRun()
        {
            for (var i = 0; i < 20 && _host.State != AutomationRunState.Running; i++) yield return null;
            _run.Model.Kill();
            for (var i = 0; i < 40 && !_host.IsFinished; i++) yield return null;
            Assert.AreEqual(AutomationRunState.Failed, _host.State);
            Assert.AreEqual("profileSaveFailed", _host.TerminalReason);
            Assert.IsNull(_host.Receipt);
        }
    }
}
