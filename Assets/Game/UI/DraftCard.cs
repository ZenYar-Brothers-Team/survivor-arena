using System;
using System.Text;
using UnityEngine.UIElements;

namespace Game.UI
{
    public sealed class DraftCard : ContentCard
    {
        public string Details { get; }
        public DraftCard(DraftOptionViewState option, Action selected) : base(
            new ContentCardViewState(option.Title, option.Detail, option.Detail, option.Icon, option.IsEnabled), selected)
        {
            AddToClassList("draft-option-select");
            EnableInClassList("draft-card-set", option.IsSet);
            EnableInClassList("draft-card-related", option.Recipes.Count > 0);
            if (!string.IsNullOrWhiteSpace(option.TypeLabel))
            {
                var typeClass = option.TypeLabel.ToLowerInvariant() switch
                {
                    "active" => "draft-type-active",
                    "passive" => "draft-type-passive",
                    "set" => "draft-type-set",
                    _ => "draft-type-other"
                };
                AddToClassList(typeClass);
                var typeLabel = new Label(option.TypeLabel.ToUpperInvariant())
                {
                    name = GameplayUiElementIds.CardType,
                    pickingMode = PickingMode.Ignore
                };
                typeLabel.AddToClassList("content-card-type");
                Insert(0, typeLabel);
            }
            var detail = new StringBuilder(option.Detail);
            if (option.IsSet) AddText("SET · FREE SET SLOT", GameplayUiElementIds.CardStatus, "content-card-status");
            var completes = false;
            for (var i = 0; i < option.Recipes.Count; i++)
            {
                var recipe = option.Recipes[i];
                completes |= recipe.CompletesRecipe;
                if (i < 2) AddText(recipe.Summary, GameplayUiElementIds.CardRecipe(i), "card-recipe");
                detail.Append("\n\n").Append(recipe.Detail);
            }
            if (option.Recipes.Count > 2) AddText($"+{option.Recipes.Count - 2} more", GameplayUiElementIds.CardMore, "card-recipe");
            EnableInClassList("draft-card-completing", completes);
            Details = detail.ToString();
            tooltip = Details;
        }
    }
}
