using System;
using UnityEngine.UIElements;

namespace Game.UI
{
    public class ContentCard : Button
    {
        public ContentCard(ContentCardViewState state, Action selected = null) : base(selected)
        {
            AddToClassList("content-card");
            EnableInClassList("content-card-selected", state.IsSelected);
            EnableInClassList("content-card-locked", state.IsLocked);
            SetEnabled(state.IsEnabled && !state.IsLocked);
            tooltip = state.Detail;
            var icon = new Image { sprite = state.Icon, name = GameplayUiElementIds.CardIcon, pickingMode = PickingMode.Ignore };
            icon.AddToClassList("content-card-icon");
            Add(icon);
            AddText(state.Title, GameplayUiElementIds.CardTitle, "content-card-title");
            AddText(state.Summary, GameplayUiElementIds.CardSummary, "content-card-summary");
            if (state.IsLocked) AddText("LOCKED", GameplayUiElementIds.CardStatus, "content-card-status");
            else if (state.IsSelected) AddText("SELECTED", GameplayUiElementIds.CardStatus, "content-card-status");
        }

        protected void AddText(string value, string id, string className)
        {
            var label = new Label(value) { name = id, pickingMode = PickingMode.Ignore };
            label.AddToClassList(className);
            Add(label);
        }
    }
}
