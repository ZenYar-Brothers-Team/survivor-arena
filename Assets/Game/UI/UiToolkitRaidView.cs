using System;
using System.Collections.Generic;
using Game.Enemy;
using UnityEngine.UIElements;

namespace Game.UI
{
    public sealed class UiToolkitRaidView : IRaidView, IDisposable
    {
        private readonly VisualElement _list;
        private readonly Label _summary;
        private readonly Button _tab;
        private readonly List<(Button button, Action handler)> _buttons = new List<(Button, Action)>();
        public event Action<RaidTemplateKind> StartRequested;

        public UiToolkitRaidView(VisualElement root)
        {
            _list = root.Q(GameplayUiElementIds.RaidTemplateList);
            _summary = root.Q<Label>(GameplayUiElementIds.RaidSummary);
            _tab = root.Q<Button>(GameplayUiElementIds.DevelopmentRaidTab);
        }

        public void SetTemplates(IReadOnlyList<RaidTemplateKind> templates)
        {
            Clear();
            foreach (var kind in templates)
            {
                var selected = kind;
                Action handler = () => StartRequested?.Invoke(selected);
                var button = new Button(handler) { name = GameplayUiElementIds.RaidStart(kind), text = Label(kind), focusable = false };
                button.AddToClassList("compact-button");
                _list.Add(button);
                _buttons.Add((button, handler));
            }
        }

        public void Render(string summary, bool launchEnabled, bool development)
        {
            _summary.text = summary ?? "";
            _tab.style.display = development ? DisplayStyle.Flex : DisplayStyle.None;
            foreach (var (button, _) in _buttons) button.SetEnabled(launchEnabled);
        }

        private static string Label(RaidTemplateKind kind) => kind switch
        {
            RaidTemplateKind.Ring => "Кольцо (15 с)",
            RaidTemplateKind.Wall => "Стена (20 с)",
            RaidTemplateKind.Contraction => "Сжимающееся кольцо (20 с)",
            RaidTemplateKind.Pincer => "Клещи (20 с)",
            _ => kind.ToString()
        };

        private void Clear()
        {
            foreach (var (button, _) in _buttons) button.RemoveFromHierarchy();
            _buttons.Clear();
        }

        public void Dispose() => Clear();
    }
}
