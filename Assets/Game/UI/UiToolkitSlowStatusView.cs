using System;
using System.Collections.Generic;
using Game.Presentation;
using UnityEngine.UIElements;

namespace Game.UI
{
    public sealed class UiToolkitSlowStatusView : ISlowStatusView, IDisposable
    {
        private readonly VisualElement _section;
        private readonly Label _summary;
        private readonly Button _slowAll;
        private readonly Dictionary<SlowStatusStyle, Button> _styles = new Dictionary<SlowStatusStyle, Button>();
        private readonly Dictionary<Button, Action> _handlers = new Dictionary<Button, Action>();
        public event Action<SlowStatusStyle> StyleRequested;
        public event Action SlowAllRequested;

        public UiToolkitSlowStatusView(VisualElement root)
        {
            _section = root.Q(GameplayUiElementIds.SlowStatusSection);
            _summary = root.Q<Label>(GameplayUiElementIds.SlowStatusSummary);
            _slowAll = root.Q<Button>(GameplayUiElementIds.SlowStatusSlowAll);
            foreach (SlowStatusStyle style in Enum.GetValues(typeof(SlowStatusStyle)))
            {
                var button = root.Q<Button>(GameplayUiElementIds.SlowStatusStyle(style));
                var selected = style;
                Action handler = () => StyleRequested?.Invoke(selected);
                button.clicked += handler;
                _styles.Add(style, button);
                _handlers.Add(button, handler);
            }
            _slowAll.clicked += SlowAll;
        }

        public void Render(SlowStatusStyle style, bool development, string summary)
        {
            _section.style.display = development ? DisplayStyle.Flex : DisplayStyle.None;
            foreach (var pair in _styles) pair.Value.EnableInClassList("development-choice-selected", pair.Key == style);
            _summary.text = summary ?? "";
        }

        private void SlowAll() => SlowAllRequested?.Invoke();

        public void Dispose()
        {
            foreach (var pair in _handlers) pair.Key.clicked -= pair.Value;
            _slowAll.clicked -= SlowAll;
        }
    }
}
