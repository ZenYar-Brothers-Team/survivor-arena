using Game.Content;
namespace Game.UI
{
    /// <summary>A map that exists in the unlock catalog but has no playable definition yet; shown closed in Field Select.</summary>
    public sealed class FieldPreviewEntry
    {
        public ContentId Id { get; }
        public string LockReason { get; }
        public FieldPreviewEntry(ContentId id, string lockReason)
        { Id = id; LockReason = string.IsNullOrWhiteSpace(lockReason) ? "Ещё не готово" : lockReason; }
    }
}
