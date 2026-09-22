using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Content;

namespace Game.UI
{
    public readonly struct DraftOptionViewState
    {
        public ContentId Id { get; }
        public string Title { get; }
        public string Detail { get; }
        public bool IsEnabled { get; }
        public Sprite Icon { get; }
        public bool IsSet { get; }
        public IReadOnlyList<RecipeProjectionViewState> Recipes { get; }

        public DraftOptionViewState(ContentId id, string title, string detail, bool isEnabled = true, Sprite icon = null, bool isSet = false,
            IReadOnlyList<RecipeProjectionViewState> recipes = null)
        {
            Id = id;
            Icon = icon;
            IsSet = isSet;
            Recipes = new List<RecipeProjectionViewState>(recipes ?? Array.Empty<RecipeProjectionViewState>()).AsReadOnly();
            IsEnabled = isEnabled;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Detail = detail ?? throw new ArgumentNullException(nameof(detail));
        }
    }
}
