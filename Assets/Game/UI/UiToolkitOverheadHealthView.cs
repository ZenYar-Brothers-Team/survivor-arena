using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Game.UI
{
    public sealed class UiToolkitOverheadHealthView : IOverheadHealthView, IDisposable
    {
        private readonly VisualElement _overlay;
        private readonly Dictionary<Guid, ProgressBar> _bars = new Dictionary<Guid, ProgressBar>();
        private readonly HashSet<Guid> _live = new HashSet<Guid>();
        private readonly List<Guid> _expired = new List<Guid>();

        public UiToolkitOverheadHealthView(VisualElement root)
        {
            _overlay = root.Q(GameplayUiElementIds.OverheadHealthOverlay);
        }

        public void Render(IReadOnlyList<OverheadHealthBarItem> bars)
        {
            _live.Clear();
            foreach (var item in bars)
            {
                _live.Add(item.LifeId);
                if (!_bars.TryGetValue(item.LifeId, out var bar))
                {
                    bar = new ProgressBar { name = GameplayUiElementIds.OverheadHealthBar(item.LifeId), lowValue = 0, highValue = 1, title = "", pickingMode = PickingMode.Ignore };
                    bar.AddToClassList("overhead-health-bar");
                    _bars.Add(item.LifeId, bar);
                    _overlay.Add(bar);
                }
                bar.style.left = Length.Percent(item.Position.x * 100);
                bar.style.top = Length.Percent(item.Position.y * 100);
                bar.value = item.Health01;
            }
            _expired.Clear();
            foreach (var id in _bars.Keys) if (!_live.Contains(id)) _expired.Add(id);
            foreach (var id in _expired) { _bars[id].RemoveFromHierarchy(); _bars.Remove(id); }
        }

        public void Dispose() { _overlay.Clear(); _bars.Clear(); }
    }
}
