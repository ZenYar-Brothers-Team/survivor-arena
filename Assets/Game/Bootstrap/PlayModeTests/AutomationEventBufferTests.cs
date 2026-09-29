using System;
using Game.Bootstrap.Automation;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class AutomationEventBufferTests
    {
        [Test]
        public void Overflow_KeepsFirstEventsAndCountsDroppedWithoutCorruptingSnapshot()
        {
            var buffer = new AutomationEventBuffer(2);
            buffer.Add(new JObject { ["id"] = "first" });
            buffer.Add(new JObject { ["id"] = "second" });
            buffer.Add(new JObject { ["id"] = "third" });

            var snapshot = buffer.Snapshot();
            Assert.AreEqual(2, snapshot.Count);
            Assert.AreEqual("first", (string)snapshot[0]["id"]);
            Assert.AreEqual("second", (string)snapshot[1]["id"]);
            Assert.AreEqual(1, buffer.DroppedCount);
            ((JObject)snapshot[0])["id"] = "changed";
            Assert.AreEqual("first", (string)buffer.Snapshot()[0]["id"]);
        }

        [Test]
        public void CapacityAndNullItemAreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AutomationEventBuffer(0));
            var buffer = new AutomationEventBuffer(1);
            Assert.Throws<ArgumentNullException>(() => buffer.Add(null));
        }
    }
}
