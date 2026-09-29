using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Game.Automation;
using Game.Bootstrap.Automation;
using Game.Meta;
using Game.Movement;
using Game.Run;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class DemonstrationRecordingTests
    {
        private GameplayCompositionRoot _root;
        private AutomationRunHost _host;
        private RunController _run;
        private ExperimentConfig _config;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var store = new MemoryProfileStore();
            ProductionSmokeScene.Load(store, openSelection: false);
            yield return null;
            yield return null;
            _root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            _run = Object.FindAnyObjectByType<RunController>();
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../scripts/balance/examples/human-demonstration.json"));
            var data = JObject.Parse(File.ReadAllText(path));
            data["demonstration"]["sampleIntervalSeconds"] = 0.02f;
            if (TestContext.CurrentContext.Test.Name.Contains("Overflow")) data["demonstration"]["maxSamples"] = 1;
            var output = Path.Combine(Application.temporaryCachePath, "demonstration-fixture-" + Guid.NewGuid().ToString("N"));
            _config = new ExperimentConfigLoader(MetaCatalog.Load(), new[] { "FIELD-001" }, output)
                .Parse(data.ToString(), Path.GetDirectoryName(path));
            _host = _root.gameObject.AddComponent<AutomationRunHost>();
            _host.Initialize(_root, _config, store);
            for (var i = 0; i < 40 && _host.State != AutomationRunState.Running; i++) yield return null;
            Assert.AreEqual(AutomationRunState.Running, _host.State, _host.TerminalReason);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_host != null && !_host.IsFinished)
            {
                _host.RequestStop("fixtureCleanup");
                for (var i = 0; i < 100 && !_host.IsFinished; i++) yield return null;
            }
            if (_host != null) Object.Destroy(_host);
            if (_root != null && _root.IsInitialized) _root.Shutdown();
            yield return null;
        }

        private string Folder => Path.Combine(_config.OutputDirectory, "chains", "chain-0001", "runs", _run.Model.RunId.ToString("N"));

        [UnityTest]
        public IEnumerator Human_LeavesNativeInputAndPauseUntouched_RecordsAlignedAnalogAndFlushes()
        {
            var mover = Object.FindAnyObjectByType<PlayerMover>();
            var source = typeof(PlayerMover).GetField("_inputSource", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNull(source.GetValue(mover), "Host must not install a bot on a human run.");
            Assert.IsTrue(_run.Model.IsPausedBy(RunPauseReasons.Manual));
            yield return new WaitForSecondsRealtime(0.12f);
            Assert.AreEqual(0, _host.DemonstrationSamples);
            // Technical input injection after asserting native ownership; not an expert demonstration.
            var fixtureInput = new BotDirectionSource();
            fixtureInput.SetDirection(new Vector2(0.5f, -0.25f));
            mover.ConfigureInputSource(fixtureInput);
            var before = mover.GetComponent<Rigidbody2D>().position;
            _run.Model.ReleasePause(RunPauseReasons.Manual);
            for (var i = 0; i < 60 && _host.DemonstrationSamples < 3; i++) yield return new WaitForFixedUpdate();
            Assert.GreaterOrEqual(_host.DemonstrationSamples, 3, _host.TerminalReason);
            _run.Model.RequestPause(RunPauseReasons.Manual);
            var pausedSamples = _host.DemonstrationSamples;
            yield return new WaitForSecondsRealtime(0.12f);
            Assert.AreEqual(pausedSamples, _host.DemonstrationSamples);
            _run.Model.ReleasePause(RunPauseReasons.Manual);
            _run.Model.Kill();
            for (var i = 0; i < 100 && !_host.IsFinished; i++) yield return null;
            Assert.AreEqual(AutomationRunState.Completed, _host.State, _host.TerminalReason);
            var rows = File.ReadAllLines(Path.Combine(Folder, "demonstration.jsonl")).Select(JObject.Parse).ToArray();
            Assert.AreEqual("human", (string)rows[0]["controller"]);
            Assert.AreEqual(0, (int)rows[1]["physicsStep"]);
            Assert.AreEqual(0, (double)rows[1]["physicsSeconds"]);
            Assert.AreEqual(0.5f, (float)rows[1]["action"][0], 0.00001f);
            Assert.AreEqual(-0.25f, (float)rows[1]["action"][1], 0.00001f);
            Assert.AreEqual(before.x, (float)rows[1]["observation"]["player"]["position"][0], 0.00001f);
            Assert.Greater(((JArray)rows[1]["observation"]["skills"]).Count, 0);
            Assert.GreaterOrEqual((float)rows[1]["observation"]["skills"][0]["cooldownRemainingSeconds"], 0);
            Assert.IsTrue((bool)rows.Last()["recordingComplete"]);
            Assert.AreEqual(pausedSamples, (int)rows.Last()["samples"]);
            Assert.AreEqual("Defeat", (string)rows.Last()["outcome"]);
            Assert.IsFalse(File.Exists(Path.Combine(Folder, "demonstration.jsonl.partial")));
            Object.Destroy(_host);
            yield return null;
            Assert.IsNull(source.GetValue(mover));
        }

        [UnityTest]
        public IEnumerator Overflow_StopsHostAndRetainsPartialFile()
        {
            _run.Model.ReleasePause(RunPauseReasons.Manual);
            for (var i = 0; i < 200 && !_host.IsFinished; i++) yield return null;
            Assert.AreEqual(AutomationRunState.Failed, _host.State, _host.TerminalReason);
            StringAssert.Contains("sampleLimit", _host.TerminalReason);
            Assert.IsFalse(File.Exists(Path.Combine(Folder, "demonstration.jsonl")));
            var rows = File.ReadAllLines(Path.Combine(Folder, "demonstration.jsonl.partial")).Select(JObject.Parse).ToArray();
            Assert.IsFalse((bool)rows.Last()["recordingComplete"]);
            Assert.AreEqual(1, (int)rows.Last()["samples"]);
            Assert.AreEqual(1, (int)rows.Last()["rejectedSamples"]);
        }

        [UnityTest]
        public IEnumerator WindowClose_DrainsSamplesBeforeFinishedEventAndKeepsRecordingClosed()
        {
            _run.Model.ReleasePause(RunPauseReasons.Manual);
            for (var i = 0; i < 60 && _host.DemonstrationSamples < 2; i++) yield return new WaitForFixedUpdate();
            var fileExistedAtFinish = false;
            _host.Finished += _ => fileExistedAtFinish = File.Exists(Path.Combine(Folder, "demonstration.jsonl"));
            // Invoke the registered veto without asking the test player/Editor to close.
            var quit = typeof(AutomationRunHost).GetMethod("WantsToQuit", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsFalse((bool)quit.Invoke(_host, null));
            _host.RequestStop("duplicateStopWhileDraining");
            Assert.AreEqual("windowClose", _host.TerminalReason);
            for (var i = 0; i < 100 && !_host.IsFinished; i++) yield return null;
            Assert.AreEqual(AutomationRunState.Stopped, _host.State, _host.TerminalReason);
            Assert.IsTrue(fileExistedAtFinish);
            Assert.IsTrue((bool)quit.Invoke(_host, null));
            Assert.AreEqual(RunCompletionReason.Aborted, _host.Outcome.Reason);
            var report = JObject.Parse(File.ReadAllText(Path.Combine(Folder, "automation.json")));
            Assert.IsTrue((bool)report["demonstration"]["recordingComplete"]);
            Assert.AreEqual("incomplete", (string)report["completionReason"], "Complete recording is not a natural W/L.");
            var session = (DemonstrationRecordingSession)typeof(AutomationRunHost)
                .GetField("_demonstration", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_host);
            session.SetError("lateNonRecordingFailure");
            Assert.IsNull(session.Error, "Closed recording must retain the same result as its footer.");
        }
    }
}
