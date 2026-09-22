using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
namespace Game.UI
{
    public sealed class UiToolkitTravelerView : ITravelerView, IDisposable
    {
        private readonly VisualElement _overlay;
        private readonly Label _observation;
        private readonly Button _spawn;
        private readonly Dictionary<Guid, VisualElement> _items = new Dictionary<Guid, VisualElement>();
        public event Action SpawnRequested;
        public UiToolkitTravelerView(VisualElement root)
        {
            _overlay = root.Q(GameplayUiElementIds.TravelerOverlay);
            _observation = root.Q<Label>(GameplayUiElementIds.TravelerObservation);
            _spawn = root.Q<Button>(GameplayUiElementIds.SpawnTraveler);
            _spawn.clicked += Spawn;
        }
        public void Render(IReadOnlyList<TravelerHudItem> travelers, string observation, bool development)
        {
            var live = new HashSet<Guid>(travelers.Select(item => item.LifeId));
            foreach (var id in _items.Keys.ToArray()) if (!live.Contains(id)) { _items[id].RemoveFromHierarchy(); _items.Remove(id); }
            foreach (var item in travelers)
            {
                if (!_items.TryGetValue(item.LifeId, out var element))
                {
                    element = new VisualElement { name = "traveler-" + item.LifeId.ToString("N"), pickingMode = PickingMode.Ignore };
                    element.AddToClassList("traveler-item");
                    var label = new Label { name = "traveler-label", pickingMode = PickingMode.Ignore }; element.Add(label);
                    var bar = new ProgressBar { name = "traveler-health", lowValue = 0, highValue = 1, pickingMode = PickingMode.Ignore }; element.Add(bar);
                    _items.Add(item.LifeId, element); _overlay.Add(element);
                }
                element.style.left = Length.Percent(item.Position.x * 100);
                element.style.top = Length.Percent(item.Position.y * 100);
                element.Q<Label>().text = (item.Offscreen ? item.Arrow + " " : "◆ ") + item.Name;
                element.Q<ProgressBar>().value = item.HealthFraction;
                element.EnableInClassList("traveler-pointer", item.Offscreen);
            }
            _observation.text = observation;
            _observation.style.display = _spawn.style.display = development ? DisplayStyle.Flex : DisplayStyle.None;
        }
        private void Spawn() => SpawnRequested?.Invoke();
        public void Dispose() { _spawn.clicked -= Spawn; _overlay.Clear(); _items.Clear(); }
    }
}
