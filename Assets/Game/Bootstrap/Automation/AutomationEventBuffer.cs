using System;
using Newtonsoft.Json.Linq;

namespace Game.Bootstrap.Automation
{
    /// <summary>Bounded typed event history; totals are owned by the recorder.</summary>
    public sealed class AutomationEventBuffer
    {
        private readonly int _capacity;
        private readonly JArray _events = new JArray();

        public int DroppedCount { get; private set; }

        public AutomationEventBuffer(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _capacity = capacity;
        }

        public void Add(JObject item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (_events.Count < _capacity) _events.Add(item);
            else DroppedCount++;
        }

        public JArray Snapshot() => (JArray)_events.DeepClone();
    }
}
