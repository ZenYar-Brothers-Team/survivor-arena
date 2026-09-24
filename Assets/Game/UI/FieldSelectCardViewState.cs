using Game.Content;
using UnityEngine;
namespace Game.UI
{
    public sealed class FieldSelectCardViewState
    {
        public ContentId Id { get; }
        public ContentCardViewState Card { get; }
        public string ThumbnailPlaceholder { get; }
        public Sprite Thumbnail { get; }
        public FieldSelectCardViewState(ContentId id, ContentCardViewState card, string thumbnailPlaceholder, Sprite thumbnail = null)
        { Id = id; Card = card; ThumbnailPlaceholder = thumbnailPlaceholder; Thumbnail = thumbnail; }
    }
}
