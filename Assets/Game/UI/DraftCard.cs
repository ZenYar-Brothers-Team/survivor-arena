using System;
using UnityEngine.UIElements;

namespace Game.UI
{
    // Hover inspects a card; a click selects the option. Cards are not keyboard-focusable, so keys never move the inspection or the choice.
    public sealed class DraftCard : VisualElement
    {
        public Button SelectButton { get; }
        public string Details { get; }

        public DraftCard(DraftOptionViewState option, Action inspected, Action selected = null)
        {
            Details = option.Detail;
            AddToClassList("draft-card");
            EnableInClassList("draft-card-set", option.IsSet);
            EnableInClassList("draft-card-related", option.Recipes.Count > 0);
            var type = option.TypeLabel.ToLowerInvariant();
            AddToClassList("draft-type-" + type);
            SelectButton = new ContentCard(new ContentCardViewState(option.Title,
                option.Detail, icon: option.Icon, isEnabled: option.IsEnabled), selected);
            SelectButton.AddToClassList("draft-option-select");
            SelectButton.tooltip = string.Empty;
            SelectButton.focusable = false;
            SelectButton.RegisterCallback<PointerEnterEvent>(_ => inspected?.Invoke());
            var typeLabel = new Label(type switch { "active" => "Активное", "passive" => "Пассивное", "set" => "Сет", _ => "" })
                { name = GameplayUiElementIds.CardType, pickingMode = PickingMode.Ignore };
            typeLabel.AddToClassList("content-card-type");
            if (string.IsNullOrEmpty(typeLabel.text)) typeLabel.style.display = DisplayStyle.None;
            var header = new VisualElement { name = GameplayUiElementIds.CardHeader, pickingMode = PickingMode.Ignore };
            header.AddToClassList("draft-card-header");
            var icon = SelectButton.Q<Image>(GameplayUiElementIds.CardIcon);
            header.Add(icon);
            var meta = new VisualElement { pickingMode = PickingMode.Ignore };
            meta.AddToClassList("draft-card-meta");
            meta.Add(typeLabel);
            var level = new Label(option.LevelLabel) { name = GameplayUiElementIds.CardLevel, pickingMode = PickingMode.Ignore };
            level.AddToClassList("card-level");
            meta.Add(level);
            header.Add(meta);
            SelectButton.Insert(0, header);
            var summary = SelectButton.Q<Label>(GameplayUiElementIds.CardSummary);
            summary.RemoveFromHierarchy();
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.AddToClassList("draft-summary-scroll"); scroll.Add(summary); SelectButton.Add(scroll);
            var count = new Label(option.IsSet ? "Не занимает слот" : $"Связанных сетов: {option.Recipes.Count}")
                { name = GameplayUiElementIds.CardMore, pickingMode = PickingMode.Ignore };
            count.AddToClassList("card-recipe"); SelectButton.Add(count);
            Add(SelectButton);
            SetInspected(false);
        }

        public void SetInspected(bool inspected)
        {
            EnableInClassList("draft-card-inspected", inspected);
        }
    }
}
