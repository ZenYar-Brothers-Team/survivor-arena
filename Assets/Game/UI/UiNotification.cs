using System;
using UnityEngine.UIElements;

namespace Game.UI
{
    // A single non-blocking slot; producers decide which event to show. Tick uses pause-aware time.
    public sealed class UiNotification
    {
        private readonly Label _label;
        private float _remaining;
        public UiNotification(Label label)
        {
            _label = label ?? throw new ArgumentNullException(nameof(label));
            _label.pickingMode = PickingMode.Ignore;
            _label.focusable = false;
            _label.style.display = DisplayStyle.None;
        }
        public void Show(string text, float seconds = 3f)
        {
            _label.text = text;
            _remaining = Math.Max(0f, seconds);
            _label.style.display = _remaining > 0f ? DisplayStyle.Flex : DisplayStyle.None;
        }
        public void Tick(float deltaSeconds)
        {
            _remaining = Math.Max(0f, _remaining - Math.Max(0f, deltaSeconds));
            if (_remaining == 0f) _label.style.display = DisplayStyle.None;
        }
    }
}
