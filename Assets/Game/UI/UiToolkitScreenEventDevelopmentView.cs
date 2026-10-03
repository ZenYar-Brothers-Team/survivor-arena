using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Game.UI
{
    public sealed class UiToolkitScreenEventDevelopmentView : IScreenEventDevelopmentView, IDisposable
    {
        private readonly VisualElement _list;
        private readonly Label _summary;
        private readonly Button _tab;
        private readonly List<(Button button, Action handler)> _buttons = new List<(Button, Action)>();
        public event Action<string> StartRequested;

        public UiToolkitScreenEventDevelopmentView(VisualElement root)
        {
            _list = root.Q(GameplayUiElementIds.ScreenEventList);
            _summary = root.Q<Label>(GameplayUiElementIds.ScreenEventSummary);
            _tab = root.Q<Button>(GameplayUiElementIds.DevelopmentEventsTab);
        }

        public void SetEntries(IReadOnlyList<ScreenEventDevelopmentEntry> entries)
        {
            Clear();
            foreach (var entry in entries)
            {
                var id = entry.Id;
                Action handler = () => StartRequested?.Invoke(id);
                var button = new Button(handler)
                {
                    name = GameplayUiElementIds.ScreenEventStart(id), text = entry.Rare ? entry.Name + " (редкое)" : entry.Name, focusable = false
                };
                button.AddToClassList("compact-button");
                _list.Add(button);
                _buttons.Add((button, handler));
            }
        }

        public void Render(string summary, bool launchEnabled, bool visible)
        {
            _summary.text = summary ?? "";
            _tab.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            foreach (var (button, _) in _buttons) button.SetEnabled(launchEnabled);
        }

        private void Clear()
        {
            foreach (var (button, _) in _buttons) button.RemoveFromHierarchy();
            _buttons.Clear();
        }

        public void Dispose() => Clear();
    }
}
