using System;
using System.Collections.Generic;

namespace Game.UI
{
    // IP-11 supplies ordered projections and component/threshold semantics. Never inferred by the view.
    public sealed class RecipeProjectionViewState
    {
        public string Title { get; }
        public int Current { get; }
        public int Projected { get; }
        public int Required { get; }
        public bool CompletesRecipe { get; }
        public bool IsAcquired { get; }
        public IReadOnlyList<string> Components { get; }
        public RecipeProjectionViewState(string title, int current, int projected, int required,
            bool completesRecipe, bool isAcquired, IReadOnlyList<string> components)
        {
            Title = title ?? string.Empty;
            Current = current;
            Projected = projected;
            Required = required;
            CompletesRecipe = completesRecipe;
            IsAcquired = isAcquired;
            Components = new List<string>(components ?? Array.Empty<string>()).AsReadOnly();
        }
        public string Summary => IsAcquired ? $"{Title} — ACQUIRED" : CompletesRecipe
            ? $"COMPLETES RECIPE: {Title} ({Current}/{Required} → {Projected}/{Required}) · not yet acquired"
            : Projected == Current ? $"{Title} {Current}/{Required} · requirement unchanged"
            : $"SET PROGRESS: {Title} {Current}/{Required} → {Projected}/{Required}";
        public string Detail => Summary + "\n" + string.Join("\n", Components);
    }
}
