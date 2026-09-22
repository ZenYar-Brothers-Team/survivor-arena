using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Game.Run.Tests
{
    public sealed class RunOutcomeTests
    {
        [Test]
        public void Victory_CapturesOnceBeforeLegacyEvents_AndClampsTime()
        {
            var model = new RunModel(1f);
            var producer = new FakeRunOutcomeContributor("kills", () => new RunOutcomeContribution(kills: 4));
            model.RegisterOutcomeContributor(producer);
            var order = new List<string>();
            model.StateChanged += state =>
            {
                if (state != RunState.Won) return;
                Assert.AreEqual(4, model.Outcome.Contributions["kills"].Kills);
                order.Add("state");
            };
            model.Won += () => order.Add("won");
            model.Completed += outcome => order.Add("completed");
            model.Start();
            model.Tick(2f);
            var result = model.Outcome;
            model.Tick(2f);
            model.Kill();
            Assert.AreSame(result, model.Stop());
            Assert.AreEqual(1, producer.CaptureCount);
            Assert.AreEqual(1f, result.ElapsedSeconds);
            Assert.AreEqual(model.RunId, result.RunId);
            CollectionAssert.AreEqual(new[] { "state", "won", "completed" }, order);
        }

        [Test]
        public void DefeatAtZeroTime_AndUnavailableData_AreExplicit()
        {
            var model = new RunModel();
            model.Start();
            model.Kill();
            Assert.AreEqual(RunCompletionReason.Defeat, model.Outcome.Reason);
            Assert.AreEqual(0f, model.Outcome.ElapsedSeconds);
            Assert.IsEmpty(model.Outcome.Contributions);
            Assert.IsNull(new RunOutcomeContribution().Kills);
            Assert.IsNull(new RunOutcomeContribution().Build);
            Assert.IsEmpty(new RunOutcomeContribution(build: Array.Empty<RunBuildEntrySnapshot>()).Build);
        }

        [Test]
        public void ContributorData_IsCopied_AndFailedCaptureCannotPreventCompletion()
        {
            var build = new List<RunBuildEntrySnapshot> { new RunBuildEntrySnapshot("FIXTURE-ACTIVE-TEST", 2) };
            var data = new RunOutcomeContribution(level: 3, experience: 4f, build: build);
            build.Clear();
            var model = new RunModel();
            model.RegisterOutcomeContributor(new FakeRunOutcomeContributor("build", () => data));
            model.RegisterOutcomeContributor(new FakeRunOutcomeContributor("broken", () => throw new InvalidOperationException()));
            model.Stop(RunCompletionReason.Error);
            Assert.AreEqual(1, model.Outcome.Contributions["build"].Build.Count);
            CollectionAssert.AreEqual(new[] { "broken" }, model.Outcome.FailedContributors);
            Assert.IsFalse(model.Outcome.Contributions.ContainsKey("broken"));
            Assert.AreEqual(RunState.Stopped, model.State);
        }

        [TestCase(RunCompletionReason.Aborted)]
        [TestCase(RunCompletionReason.Retry)]
        [TestCase(RunCompletionReason.Error)]
        public void AdministrativeStop_FromPause_FreezesWithoutGameplayOutcome(RunCompletionReason reason)
        {
            var model = new RunModel();
            var gameplayEvents = 0;
            model.Won += () => gameplayEvents++;
            model.Lost += () => gameplayEvents++;
            model.Start();
            model.Tick(3f);
            model.Pause();
            model.Stop(reason);
            model.Resume();
            model.Tick(10f);
            Assert.AreEqual(3f, model.Elapsed);
            Assert.AreEqual(0, model.PauseReasonCount);
            Assert.AreEqual(reason, model.Outcome.Reason);
            Assert.AreEqual(0, gameplayEvents);
        }

        [Test]
        public void Registration_RejectsDuplicateAndTerminal_AndUnregistersByOwner()
        {
            var model = new RunModel();
            var first = new FakeRunOutcomeContributor("xp", () => new RunOutcomeContribution(level: 1));
            var other = new FakeRunOutcomeContributor("xp", () => new RunOutcomeContribution(level: 2));
            model.RegisterOutcomeContributor(first);
            Assert.Throws<ArgumentException>(() => model.RegisterOutcomeContributor(other));
            Assert.IsFalse(model.UnregisterOutcomeContributor(other));
            Assert.IsTrue(model.UnregisterOutcomeContributor(first));
            model.Stop();
            Assert.IsEmpty(model.Outcome.Contributions);
            Assert.Throws<InvalidOperationException>(() => model.RegisterOutcomeContributor(first));
        }

        [Test]
        public void ReentrantContributor_CannotAdvanceOrChangeCompletion()
        {
            var model = new RunModel(1f);
            model.RegisterOutcomeContributor(new FakeRunOutcomeContributor("reentrant", () =>
            {
                model.Tick(100f);
                model.Kill();
                model.Stop();
                model.Pause();
                return new RunOutcomeContribution();
            }));
            model.Start();
            model.Tick(1f);
            Assert.AreEqual(RunCompletionReason.Victory, model.Outcome.Reason);
            Assert.AreEqual(1f, model.Elapsed);
        }

        [Test]
        public void ControllerShutdownAndReinitialize_ChangesIdentityAndDropsOldSubscriptions()
        {
            var go = new GameObject("run-test");
            try
            {
                var controller = go.AddComponent<RunController>();
                if (!controller.IsInitialized) controller.Initialize();
                var first = controller.Model;
                var completed = 0;
                first.Completed += _ => completed++;
                first.Start();
                first.Pause();
                controller.Shutdown();
                controller.Shutdown();
                Assert.AreEqual(1, completed);
                Assert.AreSame(first.Outcome, controller.Model.Outcome);
                controller.Initialize();
                Assert.AreNotEqual(first.RunId, controller.Model.RunId);
                Assert.AreEqual(0, controller.Model.PauseReasonCount);
                Assert.AreEqual(RunState.NotStarted, controller.Model.State);
                Assert.IsNull(controller.Model.Outcome);
                controller.Model.Stop();
                Assert.AreEqual(1, completed);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
    }
}
