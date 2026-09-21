using Game.Content;
namespace Game.UI
{
    public sealed class FieldSelectCardViewState
    {
        public ContentId Id { get; }
        public ContentCardViewState Card { get; }
        public string ThumbnailPlaceholder { get; }
        public FieldSelectCardViewState(ContentId id, ContentCardViewState card, string thumbnailPlaceholder)
        { Id = id; Card = card; ThumbnailPlaceholder = thumbnailPlaceholder; }
    }
}
