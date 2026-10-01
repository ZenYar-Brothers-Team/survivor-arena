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
        public int Difficulty { get; }
        public string LockReason { get; }
        /// <summary>Development test field: shown in its own section below the designed fields.</summary>
        public bool IsTest { get; }
        public FieldSelectCardViewState(ContentId id, ContentCardViewState card, string thumbnailPlaceholder, Sprite thumbnail = null,
            int difficulty = 1, string lockReason = null, bool isTest = false)
        { Id = id; Card = card; ThumbnailPlaceholder = thumbnailPlaceholder; Thumbnail = thumbnail; Difficulty = difficulty; LockReason = lockReason; IsTest = isTest; }
    }
}
