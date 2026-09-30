using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
namespace Game.UI
{
    /// <summary>
    /// One visible notification at a time (UI/UX §14). Time advances only while the run is not paused.
    /// Consecutive level-ups collapse into the latest level instead of replaying stale numbers after drafts.
    /// </summary>
    public sealed class NotificationQueue
    {
        private const int MaximumPending = 8;
        private readonly List<NotificationMessage> _pending = new List<NotificationMessage>();
        private readonly float _duration;
        private float _remaining;
        public NotificationMessage Current { get; private set; }
        public event Action Changed;
        public NotificationQueue(float duration) { NumericValidation.ValidatePositive(duration, nameof(duration)); _duration = duration; }
        public void Push(NotificationMessage message)
        {
            if (message.IsEmpty) return;
            if (Current.IsEmpty) { Show(message); return; }
            if (message.Kind == NotificationKind.LevelUp)
            {
                if (Current.Kind == NotificationKind.LevelUp) { Show(message); return; }
                var queued = _pending.FindIndex(pending => pending.Kind == NotificationKind.LevelUp);
                if (queued >= 0) { _pending[queued] = message; return; }
            }
            if (_pending.Count == MaximumPending) _pending.RemoveAt(0);
            _pending.Add(message);
        }
        public void Tick(float deltaTime, bool paused)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            if (paused || Current.IsEmpty) return;
            _remaining -= deltaTime; if (_remaining > 0) return;
            if (_pending.Count > 0) { var next = _pending[0]; _pending.RemoveAt(0); Show(next); }
            else { Current = default; _remaining = 0; Changed?.Invoke(); }
        }
        public void Clear() { _pending.Clear(); Current = default; _remaining = 0; Changed?.Invoke(); }
        public IReadOnlyList<NotificationMessage> Pending => _pending.ToList().AsReadOnly();
        private void Show(NotificationMessage message) { Current = message; _remaining = _duration; Changed?.Invoke(); }
    }
}
