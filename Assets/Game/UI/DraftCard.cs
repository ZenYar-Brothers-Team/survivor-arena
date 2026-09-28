using System;
using UnityEngine.UIElements;

namespace Game.UI
{
    // Inspection and confirmation are separate, including keyboard submit.
    public sealed class DraftCard : VisualElement
    {
        public Button InspectButton { get; }
        public Button ConfirmButton { get; }
        public string Details { get; }
        private readonly bool _available;

        public DraftCard(DraftOptionViewState option, Action inspected, Action confirmed = null)
        {
            _available = option.IsEnabled;
            Details = option.Detail;
            AddToClassList("draft-card");
            EnableInClassList("draft-card-set", option.IsSet);
            EnableInClassList("draft-card-related", option.Recipes.Count > 0);
            var type = option.TypeLabel.ToLowerInvariant();
            AddToClassList("draft-type-" + type);
            InspectButton = new ContentCard(new ContentCardViewState(option.Title,
                option.Detail, icon: option.Icon, isEnabled: option.IsEnabled), inspected);
            InspectButton.AddToClassList("draft-option-select");
            InspectButton.tooltip = string.Empty;
            var typeLabel = new Label(type switch { "active" => "Активное", "passive" => "Пассивное", "set" => "Сет", _ => "" })
                { name = GameplayUiElementIds.CardType, pickingMode = PickingMode.Ignore };
            typeLabel.AddToClassList("content-card-type");
            if (string.IsNullOrEmpty(typeLabel.text)) typeLabel.style.display = DisplayStyle.None;
            var header = new VisualElement { name = GameplayUiElementIds.CardHeader, pickingMode = PickingMode.Ignore };
            header.AddToClassList("draft-card-header");
            var icon = InspectButton.Q<Image>(GameplayUiElementIds.CardIcon);
            header.Add(icon);
            var meta = new VisualElement { pickingMode = PickingMode.Ignore };
            meta.AddToClassList("draft-card-meta");
            meta.Add(typeLabel);
            var level = new Label(option.LevelLabel) { name = GameplayUiElementIds.CardLevel, pickingMode = PickingMode.Ignore };
            level.AddToClassList("card-level");
            meta.Add(level);
            header.Add(meta);
            InspectButton.Insert(0, header);
            var summary = InspectButton.Q<Label>(GameplayUiElementIds.CardSummary);
            summary.RemoveFromHierarchy();
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.AddToClassList("draft-summary-scroll"); scroll.Add(summary); InspectButton.Add(scroll);
            var count = new Label(option.IsSet ? "Не занимает слот" : $"Связанных сетов: {option.Recipes.Count}")
                { name = GameplayUiElementIds.CardMore, pickingMode = PickingMode.Ignore };
            count.AddToClassList("card-recipe"); InspectButton.Add(count);
            Add(InspectButton);
            ConfirmButton = new Button(confirmed) { text = "Выбрать" };
            ConfirmButton.AddToClassList("folio-button"); ConfirmButton.AddToClassList("draft-confirm");
            Add(ConfirmButton);
            SetInspected(false, false);
        }

        public void SetInspected(bool inspected, bool banish)
        {
            EnableInClassList("draft-card-inspected", inspected);
            ConfirmButton.text = banish ? "Исключить" : "Выбрать";
            ConfirmButton.SetEnabled(_available && inspected);
        }
    }
}
