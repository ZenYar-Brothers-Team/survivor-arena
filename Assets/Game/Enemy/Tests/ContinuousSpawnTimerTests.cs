using NUnit.Framework;

namespace Game.Enemy.Tests
{
    public class ContinuousSpawnTimerTests
    {
        [Test]
        public void Tick_EmitsSpawnAtEveryInterval()
        {
            var timer = new ContinuousSpawnTimer(1f);

            Assert.AreEqual(0, timer.Tick(0.75f, isRunning: true));
            Assert.AreEqual(1, timer.Tick(0.25f, isRunning: true));
            Assert.AreEqual(2, timer.Tick(2.4f, isRunning: true));
            Assert.AreEqual(0, timer.Tick(0.5f, isRunning: true));
            Assert.AreEqual(1, timer.Tick(0.11f, isRunning: true));
        }

        [Test]
        public void PauseAndEnd_DoNotAdvanceSpawnTime()
        {
            var timer = new ContinuousSpawnTimer(1f);

            Assert.AreEqual(0, timer.Tick(0.75f, isRunning: true));
            Assert.AreEqual(0, timer.Tick(10f, isRunning: false));
            Assert.AreEqual(1, timer.Tick(0.25f, isRunning: true));
        }
    }
}
