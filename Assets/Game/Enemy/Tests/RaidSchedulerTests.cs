using NUnit.Framework;

namespace Game.Enemy.Tests
{
    public sealed class RaidSchedulerTests
    {
        private static RaidScheduler Scheduler(int seed = 7) => new RaidScheduler(TestProfile.Create(), new System.Random(seed));

        // A scheduler that has already run one roundup, so the random wait is in effect.
        private static RaidScheduler AfterFirstRoundup(int seed = 7)
        {
            var scheduler = Scheduler(seed);
            scheduler.Start(RaidTemplateKind.Ring);
            scheduler.End();
            return scheduler;
        }

        [Test]
        public void Constructor_FirstRoundupHasNoWait()
        {
            Assert.AreEqual(0f, Scheduler().WaitRemainingSeconds);
        }

        [Test]
        public void Tick_FirstCrowd_FiresImmediately()
        {
            Assert.IsTrue(Scheduler().Tick(.5f, true, 40));
        }

        [Test]
        public void Tick_BelowThreshold_NeverFires()
        {
            var scheduler = Scheduler();

            for (var i = 0; i < 100; i++) Assert.IsFalse(scheduler.Tick(1f, true, 39));

            Assert.IsFalse(scheduler.CrowdPresent);
        }

        [Test]
        public void End_WaitIsRandomWithinConfiguredInterval()
        {
            for (var seed = 0; seed < 50; seed++)
            {
                var wait = AfterFirstRoundup(seed).WaitRemainingSeconds;
                Assert.GreaterOrEqual(wait, 60f);
                Assert.LessOrEqual(wait, 120f);
            }
        }

        [Test]
        public void Tick_AfterARoundup_WithoutCrowdDoesNotRunTheCountdown()
        {
            var scheduler = AfterFirstRoundup();
            var before = scheduler.WaitRemainingSeconds;

            for (var i = 0; i < 200; i++) Assert.IsFalse(scheduler.Tick(1f, true, 39));

            Assert.AreEqual(before, scheduler.WaitRemainingSeconds);
        }

        [Test]
        public void Tick_AfterARoundup_WithCrowdCountsDownAndFiresWhenItReachesZero()
        {
            var scheduler = AfterFirstRoundup();
            var fired = false;
            var elapsed = 0f;

            while (!fired && elapsed < 400f)
            {
                fired = scheduler.Tick(1f, true, 40);
                elapsed += 1f;
            }

            Assert.IsTrue(fired);
            Assert.GreaterOrEqual(elapsed, 60f);
            Assert.LessOrEqual(elapsed, 121f);
        }

        [Test]
        public void Tick_WhenPaused_DoesNotAdvance()
        {
            var scheduler = AfterFirstRoundup();
            var before = scheduler.WaitRemainingSeconds;

            scheduler.Tick(30f, false, 500);

            Assert.AreEqual(before, scheduler.WaitRemainingSeconds);
        }

        [Test]
        public void Start_RunsHardTimeoutThenRestartsTheWaitOnlyAfterTheEnd()
        {
            var scheduler = AfterFirstRoundup();
            var waitBeforeStart = scheduler.WaitRemainingSeconds;

            Assert.IsTrue(scheduler.Start(RaidTemplateKind.Ring));
            Assert.AreEqual(15f, scheduler.ActiveRemainingSeconds);
            scheduler.Tick(14f, true, 500);
            Assert.IsTrue(scheduler.IsActive);
            Assert.AreEqual(waitBeforeStart, scheduler.WaitRemainingSeconds, "The wait must not move while a roundup runs.");

            scheduler.Tick(1f, true, 500);

            Assert.IsFalse(scheduler.IsActive);
            Assert.GreaterOrEqual(scheduler.WaitRemainingSeconds, 60f);
            Assert.LessOrEqual(scheduler.WaitRemainingSeconds, 120f);
        }

        [Test]
        public void Start_WhileActive_IsRefused()
        {
            var scheduler = Scheduler();
            scheduler.Start(RaidTemplateKind.Ring);

            Assert.IsFalse(scheduler.Start(RaidTemplateKind.Ring));
        }

        [Test]
        public void Tick_WhileActive_NeverFiresAgain()
        {
            var scheduler = Scheduler();
            scheduler.Start(RaidTemplateKind.Ring);

            Assert.IsFalse(scheduler.Tick(5f, true, 500));
        }
    }
}
