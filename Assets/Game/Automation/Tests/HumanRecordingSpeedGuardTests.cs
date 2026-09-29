using Game.Run;
using NUnit.Framework;

namespace Game.Automation.Tests
{
    public sealed class HumanRecordingSpeedGuardTests
    {
        [Test]
        public void SpeedChange_DuringHumanRecording_ReturnsToOneBeforeCallEnds()
        {
            var run = new RunModel();
            run.Start();
            using (new HumanRecordingSpeedGuard(run))
            {
                Assert.IsTrue(run.SetSpeed(5));
                Assert.AreEqual(1, run.SpeedMultiplier);
            }
            Assert.IsTrue(run.SetSpeed(2));
            Assert.AreEqual(2, run.SpeedMultiplier, "Guard unsubscribes when the recording ends.");
        }
    }
}
