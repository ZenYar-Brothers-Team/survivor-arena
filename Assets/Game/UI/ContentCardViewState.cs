using UnityEngine;

namespace Game.UI
{
    // Presentation-only contract; eligibility, effect text and icon resolution belong to the producer.
    public sealed class ContentCardViewState
    {
        public string Title { get; }
        public string Summary { get; }
        public string Detail { get; }
        public Sprite Icon { get; }
        public bool IsEnabled { get; }
        public bool IsSelected { get; }
        public bool IsLocked { get; }
        public ContentCardViewState(string title, string summary, string detail = "", Sprite icon = null,
            bool isEnabled = true, bool isSelected = false, bool isLocked = false)
        {
            Title = title ?? string.Empty;
            Summary = summary ?? string.Empty;
            Detail = detail ?? string.Empty;
            Icon = icon;
            IsEnabled = isEnabled;
            IsSelected = isSelected;
            IsLocked = isLocked;
        }
    }
}
