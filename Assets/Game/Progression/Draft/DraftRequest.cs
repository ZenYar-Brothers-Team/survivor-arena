using System;
using Game.Content;

namespace Game.Progression
{
    /// <summary>An accepted reward. Its identity survives every offer revision.</summary>
    public sealed class DraftRequest
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Guid RunId { get; }
        public DraftOrigin Origin { get; }
        public int? EarnedLevel { get; }
        public Guid? PickupId { get; }
        public ContentId? SourceContentId { get; }

        private DraftRequest(Guid runId, DraftOrigin origin, int? level, Guid? pickup, ContentId? source)
        {
            if (runId == Guid.Empty) throw new ArgumentException("Run identity is required.", nameof(runId));
            RunId = runId;
            Origin = origin;
            EarnedLevel = level;
            PickupId = pickup;
            SourceContentId = source;
        }

        public static DraftRequest ForLevel(Guid runId, int level)
        {
            NumericValidation.ValidateCount(level, nameof(level));
            return new DraftRequest(runId, DraftOrigin.LevelUp, level, null, null);
        }

        public static DraftRequest ForBook(Guid runId, Guid pickup, ContentId source)
        {
            if (pickup == Guid.Empty) throw new ArgumentException("Pickup identity is required.", nameof(pickup));
            if (!source.IsValid) throw new ArgumentException("Source content is required.", nameof(source));
            return new DraftRequest(runId, DraftOrigin.Book, null, pickup, source);
        }
    }
}
