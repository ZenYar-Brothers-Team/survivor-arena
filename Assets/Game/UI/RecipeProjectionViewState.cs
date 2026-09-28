using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    // IP-11 supplies ordered projections and component/threshold semantics. Never inferred by the view.
    public sealed class RecipeProjectionViewState
    {
        public string Title { get; }
        public int Current { get; }
        public int Projected { get; }
        public int Required { get; }
        public int OwnedComponents { get; }
        public bool CompletesRecipe { get; }
        public bool IsAcquired { get; }
        public IReadOnlyList<string> Components { get; }
        public IReadOnlyList<RecipeComponentViewState> ComponentStates { get; }
        public string Effect { get; }
        public Sprite Icon { get; }
        public RecipeProjectionViewState(string title, int current, int projected, int required,
            bool completesRecipe, bool isAcquired, IReadOnlyList<string> components, string effect = "", Sprite icon = null,
            int? ownedComponents = null, IReadOnlyList<RecipeComponentViewState> componentStates = null)
        {
            Effect = effect ?? string.Empty;
            Icon = icon;
            Title = title ?? string.Empty;
            Current = current;
            Projected = projected;
            Required = required;
            OwnedComponents = ownedComponents ?? current;
            CompletesRecipe = completesRecipe;
            IsAcquired = isAcquired;
            Components = new List<string>(components ?? Array.Empty<string>()).AsReadOnly();
            ComponentStates = new List<RecipeComponentViewState>(componentStates ?? Array.Empty<RecipeComponentViewState>()).AsReadOnly();
        }
        public string Status => IsAcquired ? "Получен" : CompletesRecipe ? "Завершит рецепт" :
            Current >= Required ? "Рецепт готов" : "В процессе";
        public string Progress => $"{OwnedComponents}/{Required}";
        public string Summary => $"{Title} · {Progress}";
        public string Detail => Summary + "\n" + string.Join("\n", Components);
    }
}
