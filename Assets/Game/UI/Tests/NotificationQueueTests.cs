using NUnit.Framework;
namespace Game.UI.Tests
{
    public sealed class NotificationQueueTests
    {
        private static NotificationMessage Set(string name) => new NotificationMessage(NotificationKind.SetAcquired, name);

        [Test] public void Queue_PausedAndBurst_OneMessageAndNoTimeAdvance()
        {
            var queue=new NotificationQueue(3);queue.Push(NotificationMessage.LevelUp(2));queue.Push(Set("A"));
            queue.Tick(20,true);Assert.AreEqual(NotificationKind.LevelUp,queue.Current.Kind);
            queue.Tick(3,false);Assert.AreEqual(Set("A"),queue.Current);
            queue.Tick(3,false);Assert.IsTrue(queue.Current.IsEmpty);
            queue.Push(Set("old"));queue.Clear();queue.Tick(10,false);Assert.IsTrue(queue.Current.IsEmpty);
        }

        [Test] public void Push_ConsecutiveLevelUps_ShowOnlyLatestLevel()
        {
            var queue=new NotificationQueue(3);
            queue.Push(NotificationMessage.LevelUp(2));queue.Push(NotificationMessage.LevelUp(3));
            Assert.AreEqual(3,queue.Current.Value);
            queue.Push(Set("A"));queue.Push(NotificationMessage.LevelUp(4));queue.Push(NotificationMessage.LevelUp(5));
            Assert.AreEqual(5,queue.Current.Value,"The visible level-up is refreshed, not queued again.");
            Assert.AreEqual(1,queue.Pending.Count);
            queue.Tick(3,false);Assert.AreEqual(Set("A"),queue.Current);
            queue.Push(NotificationMessage.LevelUp(6));queue.Push(NotificationMessage.LevelUp(7));
            Assert.AreEqual(1,queue.Pending.Count);Assert.AreEqual(7,queue.Pending[0].Value);
        }

        [Test] public void Copy_EveryKind_HasTitleAndLevelDetail()
        {
            foreach (NotificationKind kind in System.Enum.GetValues(typeof(NotificationKind)))
                if (kind != NotificationKind.None) Assert.IsNotEmpty(NotificationCopy.Title(new NotificationMessage(kind)), kind.ToString());
            Assert.AreEqual("Новый уровень",NotificationCopy.Title(NotificationMessage.LevelUp(12)));
            Assert.AreEqual("Уровень 12",NotificationCopy.Detail(NotificationMessage.LevelUp(12)));
            Assert.AreEqual("A",NotificationCopy.Detail(Set("A")));
        }
    }
}
