using NUnit.Framework;

namespace Game.Run.Tests
{
    public class RunModelTests
    {
        [Test]
        public void Start_FromNotStarted_TransitionsToRunning()
        {
            var model = new RunModel();

            model.Start();

            Assert.AreEqual(RunState.Running, model.State);
        }

        [Test]
        public void Pause_StopsElapsedFromAdvancing()
        {
            var model = new RunModel(10f);
            model.Start();

            model.Tick(1f);
            model.Pause();
            model.Tick(1f);
            model.Tick(1f);

            Assert.AreEqual(RunState.Paused, model.State);
            Assert.AreEqual(1f, model.Elapsed, 0.0001f);
        }

        [Test]
        public void Resume_ContinuesAdvancingElapsed()
        {
            var model = new RunModel(10f);
            model.Start();
            model.Tick(1f);
            model.Pause();

            model.Resume();
            model.Tick(1f);

            Assert.AreEqual(RunState.Running, model.State);
            Assert.AreEqual(2f, model.Elapsed, 0.0001f);
        }

        [Test]
        public void Tick_ReachingDuration_TransitionsToWonAndRaisesEventOnce()
        {
            var model = new RunModel(0.1f);
            model.Start();
            var wonCount = 0;
            model.Won += () => wonCount++;

            model.Tick(0.05f);
            model.Tick(0.05f);
            model.Tick(0.05f);

            Assert.AreEqual(RunState.Won, model.State);
            Assert.AreEqual(1, wonCount);
        }

        [Test]
        public void Kill_BeforeDuration_TransitionsToLostAndRaisesEvent()
        {
            var model = new RunModel(10f);
            model.Start();
            var lostCount = 0;
            model.Lost += () => lostCount++;

            model.Tick(1f);
            model.Kill();

            Assert.AreEqual(RunState.Lost, model.State);
            Assert.AreEqual(1, lostCount);
        }

        [Test]
        public void AfterWon_FurtherCallsDoNotChangeStateOrRaiseEventsAgain()
        {
            var model = new RunModel(0.1f);
            model.Start();
            var wonCount = 0;
            model.Won += () => wonCount++;
            model.Tick(0.2f);

            model.Tick(1f);
            model.Pause();
            model.Kill();

            Assert.AreEqual(RunState.Won, model.State);
            Assert.AreEqual(1, wonCount);
        }

        [Test]
        public void AfterLost_FurtherCallsDoNotChangeStateOrRaiseEventsAgain()
        {
            var model = new RunModel(10f);
            model.Start();
            var lostCount = 0;
            model.Lost += () => lostCount++;
            model.Kill();

            model.Tick(1f);
            model.Pause();
            model.Kill();

            Assert.AreEqual(RunState.Lost, model.State);
            Assert.AreEqual(1, lostCount);
        }
    }
}
