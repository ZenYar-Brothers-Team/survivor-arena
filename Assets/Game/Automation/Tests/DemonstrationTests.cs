using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Game.Automation.Tests
{
    public sealed class DemonstrationTests
    {
        private static DemonstrationConfigData Settings(int samples = 100, int queue = 16) =>
            new DemonstrationConfigData { SchemaVersion = 1, SampleIntervalSeconds = 0.1f,
                MaxSamples = samples, MaxFileMegabytes = 1, QueueCapacity = queue, MaxEntitiesPerCollection = 32 };

        [Test]
        public void Clock_UsesPrePhysicsTimeAndCountsUnsampledSteps()
        {
            var clock = new DemonstrationClock(0.1f);
            var selected = new List<long>();
            for (var i = 0; i < 12; i++)
            {
                if (clock.Advance(0.02f)) selected.Add(clock.StepIndex);
                Assert.AreEqual(i * 0.02, clock.StepStartSeconds, 0.000001);
            }
            CollectionAssert.AreEqual(new long[] { 0, 5, 10 }, selected);
            Assert.Throws<ArgumentOutOfRangeException>(() => clock.Advance(float.NaN));
            Assert.AreEqual(11, clock.StepIndex, "Rejected time must not advance the clock.");
        }

        [Test]
        public void Settings_MissingNonFiniteAndOutOfRangeFailClosed()
        {
            Assert.Throws<ArgumentException>(() => new DemonstrationConfigData().Validate());
            var config = Settings(); config.SampleIntervalSeconds = float.PositiveInfinity;
            Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
            config = Settings(); config.MaxEntitiesPerCollection = 2049;
            Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
        }

        [Test]
        public async Task Writer_DrainsInOrderWithUtf8BudgetAndOneFooter()
        {
            var output = new StringWriter();
            var commits = new List<bool>();
            using var writer = new DemonstrationWriter(Settings(), new JObject { ["type"] = "header" },
                () => output, commits.Add);
            Assert.IsTrue(writer.TryWriteSample(new JObject { ["label"] = "опыт" }));
            Assert.IsTrue(writer.TryWriteSample(new JObject { ["label"] = "second" }));
            var completion = writer.Complete(new JObject { ["reason"] = "Defeat" });
            Assert.AreSame(completion, writer.Complete(new JObject { ["reason"] = "duplicate" }));
            Assert.IsFalse(writer.TryWriteSample(new JObject()));
            await completion;
            var lines = output.ToString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(JObject.Parse).ToArray();
            CollectionAssert.AreEqual(new[] { "header", "sample", "sample", "end" }, lines.Select(x => (string)x["type"]));
            Assert.AreEqual(0, (int)lines[1]["sampleIndex"]);
            Assert.AreEqual(1, (int)lines[2]["sampleIndex"]);
            Assert.AreEqual("Defeat", (string)lines[3]["reason"]);
            Assert.AreEqual(2, (int)lines[3]["samples"]);
            Assert.IsTrue(writer.RecordingComplete);
            CollectionAssert.AreEqual(new[] { true }, commits);
            Assert.AreEqual(Encoding.UTF8.GetByteCount(output.ToString()), writer.WrittenBytes);
        }

        [Test]
        public async Task Writer_SampleLimitKeepsIncompleteFooter()
        {
            var output = new StringWriter();
            using var writer = new DemonstrationWriter(Settings(samples: 1), new JObject(), () => output, _ => { });
            Assert.IsTrue(writer.TryWriteSample(new JObject()));
            Assert.IsFalse(writer.TryWriteSample(new JObject()));
            await writer.Complete(new JObject());
            Assert.AreEqual("sampleLimit", writer.Error);
            Assert.AreEqual(1, writer.WrittenSamples);
            Assert.AreEqual(1, writer.RejectedSamples);
            Assert.IsFalse(writer.RecordingComplete);
            StringAssert.Contains("\"recordingComplete\":false", output.ToString());
        }

        [Test]
        public async Task Writer_ByteLimitCountsUtf8NotCharacters()
        {
            using var writer = new DemonstrationWriter(Settings(), new JObject(), () => new StringWriter(), _ => { });
            Assert.IsFalse(writer.TryWriteSample(new JObject { ["value"] = new string('€', 400000) }));
            await writer.Complete(new JObject());
            Assert.AreEqual("byteLimit", writer.Error);
            Assert.AreEqual(0, writer.WrittenSamples);
        }

        [Test]
        public async Task Writer_FullQueueNeverBlocksProducerOrSilentlyDrops()
        {
            using var gate = new ManualResetEventSlim();
            using var writer = new DemonstrationWriter(Settings(queue: 1), new JObject(), () =>
            {
                if (!gate.Wait(TimeSpan.FromSeconds(10))) throw new IOException("Fixture gate timeout");
                return new StringWriter();
            }, _ => { });
            try
            {
                Assert.IsTrue(writer.TryWriteSample(new JObject()));
                Assert.IsFalse(writer.TryWriteSample(new JObject()));
                Assert.AreEqual("queueFull", writer.Error);
            }
            finally { gate.Set(); }
            await writer.Complete(new JObject());
            Assert.IsFalse(writer.RecordingComplete);
            Assert.AreEqual(1, writer.WrittenSamples);
        }

        [Test]
        public async Task Writer_IoFailureIsReportedAndNeverCommitted()
        {
            var committed = false;
            using var writer = new DemonstrationWriter(Settings(), new JObject(),
                () => throw new IOException("fixture denied"), _ => committed = true);
            await writer.Complete(new JObject());
            StringAssert.Contains("writeFailure: fixture denied", writer.Error);
            Assert.IsFalse(committed);
            Assert.IsFalse(writer.RecordingComplete);
        }

        [Test]
        public async Task Writer_FilePromotedOnlyAfterCleanCompletionAndNeverOverwrites()
        {
            var folder = Path.Combine(Path.GetTempPath(), "demonstration-" + Guid.NewGuid().ToString("N"));
            var path = Path.Combine(folder, "demonstration.jsonl");
            try
            {
                using (var writer = DemonstrationWriter.CreateFile(path, Settings(), new JObject()))
                {
                    Assert.IsTrue(writer.TryWriteSample(new JObject()));
                    await writer.Complete(new JObject());
                    Assert.IsNull(writer.Error);
                }
                Assert.IsTrue(File.Exists(path));
                Assert.IsFalse(File.Exists(path + ".partial"));
                Assert.Throws<IOException>(() => DemonstrationWriter.CreateFile(path, Settings(), new JObject()));
                var partial = Path.Combine(folder, "interrupted.jsonl");
                using (var writer = DemonstrationWriter.CreateFile(partial, Settings(), new JObject()))
                {
                    writer.TryWriteSample(new JObject());
                    writer.Dispose();
                    await writer.Completion;
                }
                Assert.IsFalse(File.Exists(partial));
                Assert.IsTrue(File.Exists(partial + ".partial"));
            }
            finally { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
        }
    }
}
