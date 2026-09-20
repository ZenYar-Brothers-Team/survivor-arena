using System;
using Game.Content;

namespace Game.UI
{
    public readonly struct DraftOptionViewState
    {
        public ContentId Id { get; }
        public string Title { get; }
        public string Detail { get; }
        public bool IsEnabled { get; }

        public DraftOptionViewState(ContentId id, string title, string detail, bool isEnabled = true)
        {
            Id = id;
            IsEnabled = isEnabled;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Detail = detail ?? throw new ArgumentNullException(nameof(detail));
        }
    }
}
