using System;
using Game.Content;
namespace Game.Pickup
{
    public readonly struct PickupIdentity
    {
        public Guid DropId { get; }
        public Guid RunId { get; }
        public Guid? SourceLifeId { get; }
        public ContentId? SourceContentId { get; }
        public long Sequence { get; }
        public PickupIdentity(Guid dropId, Guid runId, long sequence, Guid? sourceLifeId = null, ContentId? sourceContentId = null)
        {
            if (dropId == Guid.Empty || runId == Guid.Empty) throw new ArgumentException("Drop/run identity is required.");
            NumericValidation.ValidateNonNegative(sequence, nameof(sequence));
            DropId = dropId; RunId = runId; Sequence = sequence; SourceLifeId = sourceLifeId; SourceContentId = sourceContentId;
        }
    }
}
