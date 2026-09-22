using System;
using System.Collections.Generic;
using System.Diagnostics;
using Game.Combat;
using Game.Content;
using Game.Run;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Game.Telemetry.Tests
{
    public sealed class RunTelemetryRecorderTests
    {
        [TestCase("Victory", "completed")]
        [TestCase("Defeat", "completed")]
        [TestCase("Aborted", "aborted")]
        [TestCase("Retry", "retry")]
        [TestCase("Error", "error")]
        public void Terminal_ReasonIsDistinct_AndSnapshotDoesNotChange(string reason, string completion)
        {
            var run = new RunModel(60); var recorder = TelemetryTestData.Recorder(run);
            run.StateChanged += recorder.StateChanged; run.Start(); run.Tick(10);
            recorder.Combat(TelemetryTestData.Damage(run));
            if (reason == "Victory") run.Tick(50);
            else if (reason == "Defeat") run.Kill();
            else run.Stop((RunCompletionReason)Enum.Parse(typeof(RunCompletionReason), reason));
            var first = recorder.Snapshot(null, true);
            recorder.Combat(TelemetryTestData.Damage(run)); run.Stop();
            Assert.AreSame(first, recorder.Snapshot(null, true));
            var data = JObject.Parse(first.Json);
            Assert.AreEqual(completion, (string)data["completionReason"]);
            Assert.AreEqual(reason, (string)data["outcome"]["reason"]);
            Assert.AreEqual(10, (double)data["appliedDamageDealt"]);
            Assert.AreEqual(100, (double)data["combat"][0]["attempted"]);
            Assert.AreEqual(90, (double)data["combat"][0]["overkill"]);
        }
        [Test]
        public void Pause_ExcludesWallTimeFromDps_AndEquippedTimeStartsOnAcquisition()
        {
            double clock = 0; var run = new RunModel(100);
            var recorder = new RunTelemetryRecorder(run, new TelemetryLimits(), TelemetryTestData.Provenance(), () => clock, DateTime.UtcNow);
            run.StateChanged += recorder.StateChanged; run.Start(); run.Tick(20); clock = 20;
            recorder.Equip(new ContentId("FIXTURE-SKILL"));
            run.Pause(); clock = 40; run.Tick(20); run.Resume();
            run.Tick(60); clock = 100; recorder.Combat(TelemetryTestData.Damage(run, 300, 300)); run.Stop();
            var data = JObject.Parse(recorder.Snapshot(null, true).Json);
            Assert.AreEqual(80, (double)data["runningSeconds"]);
            Assert.AreEqual(20, (double)data["pauseWallSeconds"]);
            Assert.AreEqual(100, (double)data["wallSeconds"]);
            Assert.AreEqual(3.75, (double)data["observedRunDps"]);
            Assert.AreEqual(5, (double)data["combat"][0]["observedEquippedDps"]);
        }
        [Test]
        public void UnknownAndZeroTime_AreNotInventedValues()
        {
            var run = new RunModel(); var recorder = TelemetryTestData.Recorder(run);
            recorder.Combat(new CombatResult(default, new CombatIdentity(Guid.NewGuid(), run.RunId, null, CombatEntityCategory.Player), new HealthChange(5, 5, 5, false)));
            var data = JObject.Parse(recorder.Snapshot(null, false).Json);
            Assert.AreEqual("incomplete", (string)data["completionReason"]);
            Assert.AreEqual("unknown", (string)data["combat"][0]["source"]);
            Assert.AreEqual(JTokenType.Null, data["combat"][0]["level"].Type);
            Assert.AreEqual(JTokenType.Null, data["observedRunDps"].Type);
            Assert.AreEqual(JTokenType.Null, data["producers"].Type);
        }
        [Test]
        public void BoundedBuffers_SaturateWithoutEvictingDedupIds_AndCountDroppedData()
        {
            var run = new RunModel(); var recorder = TelemetryTestData.Recorder(run, new TelemetryLimits(2, 1, 1));
            var id = Guid.NewGuid(); Assert.IsTrue(recorder.Claim(id)); Assert.IsFalse(recorder.Claim(id));
            Assert.IsFalse(recorder.Claim(Guid.NewGuid())); Assert.IsFalse(recorder.Claim(id));
            recorder.Marker("one"); recorder.Marker("two");
            recorder.Combat(TelemetryTestData.Damage(run)); recorder.Combat(TelemetryTestData.Damage(run, level: 2));
            var data = JObject.Parse(recorder.Snapshot(null, false).Json);
            Assert.AreEqual(1, recorder.DroppedEvents); Assert.AreEqual(1, recorder.DroppedKeys); Assert.AreEqual(1, recorder.DroppedIdentities);
            Assert.AreEqual(2, ((JArray)data["timeline"]).Count); Assert.AreEqual(1, ((JArray)data["combat"]).Count);
        }
        [Test]
        public void Provenance_ReorderedFilesHashIdentically_ContentAndOverridesChangeHash()
        {
            var a = new Dictionary<string, string> { ["a"] = "1", ["b"] = "2" };
            var b = new Dictionary<string, string> { ["b"] = "2", ["a"] = "1" };
            JObject Capture(Dictionary<string, string> files, int seed) => TelemetryProvenance.Capture(files, new { seed }, "commit", true, "test", "fixture");
            Assert.AreEqual((string)Capture(a, 1)["configHash"], (string)Capture(b, 1)["configHash"]);
            Assert.AreNotEqual((string)Capture(a, 1)["configHash"], (string)Capture(a, 2)["configHash"]);
            b["a"] = "3";
            Assert.AreNotEqual((string)Capture(a, 1)["configHash"], (string)Capture(b, 1)["configHash"]);
            Assert.IsTrue((bool)Capture(a, 1)["dirty"]);
        }
        [Test]
        public void Export_ByteLimitFailsExplicitly_WithoutCompletingTheRun()
        {
            var run = new RunModel(); var recorder = TelemetryTestData.Recorder(run, new TelemetryLimits(exportBytes: 1));
            Assert.Throws<InvalidOperationException>(() => recorder.Snapshot(null, false));
            Assert.IsNull(run.Outcome); Assert.IsFalse(recorder.IsSealed);
        }
        [Test]
        public void Combat_HundredThousandHits_AggregateWithoutHotPathAllocationsOrRngChanges()
        {
            var run = new RunModel(); var recorder = TelemetryTestData.Recorder(run);
            var hit = TelemetryTestData.Damage(run, 1, 1); recorder.Combat(hit);
            var random = UnityEngine.Random.state;
            var watch = Stopwatch.StartNew();
            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var i = 0; i < 100000; i++) recorder.Combat(hit);
            var bytes = GC.GetAllocatedBytesForCurrentThread() - before;
            watch.Stop();
            Assert.AreEqual(0, bytes); Assert.Less(watch.ElapsedMilliseconds, 2000, "Diagnostic regression guard, not a gameplay frame budget");
            Assert.AreEqual(random, UnityEngine.Random.state);
            var data = JObject.Parse(recorder.Snapshot(null, false).Json);
            Assert.AreEqual(100001, (double)data["appliedDamageDealt"]);
            TestContext.WriteLine($"100000 hits: {watch.Elapsed.TotalMilliseconds:0.###} ms, {bytes} B allocated");
        }
        [Test]
        public void ForeignRunDamage_IsIgnored()
        {
            var recorder = TelemetryTestData.Recorder(new RunModel());
            recorder.Combat(TelemetryTestData.Damage(new RunModel()));
            Assert.AreEqual(0, (double)JObject.Parse(recorder.Snapshot(null, false).Json)["appliedDamageDealt"]);
        }
        [Test]
        public void Snapshot_ContentIdDictionaryKeys_RetainOrdinalCase()
        {
            var recorder = TelemetryTestData.Recorder(new RunModel());
            var producers = new { activeSkillActivations = new Dictionary<string, int> { ["FIXTURE-SKILL-BOLT"] = 7 } };
            var data = JObject.Parse(recorder.Snapshot(producers, false).Json);
            Assert.AreEqual(7, (int)data["producers"]["activeSkillActivations"]["FIXTURE-SKILL-BOLT"]);
            Assert.IsNull(data["producers"]["activeSkillActivations"]["fixturE-SKILL-BOLT"]);
        }
    }
}
