using System;
using Game.Content;

namespace Game.UI
{
    public readonly struct DraftOptionViewState
    {
        public ContentId Id { get; }
        public string Title { get; }
        public string Detail { get; }

        public DraftOptionViewState(ContentId id, string title, string detail)
        {
            Id = id;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Detail = detail ?? throw new ArgumentNullException(nameof(detail));
        }
    }
}
