using NUnit.Framework;
namespace Game.UI.Tests
{
    public sealed class NotificationQueueTests
    {
        [Test] public void Queue_PausedAndBurst_OneMessageAndNoTimeAdvance()
        {
            var queue=new NotificationQueue(3);queue.Push("LEVEL UP");queue.Push("SET ACQUIRED");
            queue.Tick(20,true);Assert.AreEqual("LEVEL UP",queue.Current);
            queue.Tick(3,false);Assert.AreEqual("SET ACQUIRED",queue.Current);
            queue.Tick(3,false);Assert.AreEqual("",queue.Current);
            queue.Push("old");queue.Clear();queue.Tick(10,false);Assert.AreEqual("",queue.Current);
        }
    }
}
