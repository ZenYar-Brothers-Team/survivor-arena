using System;
using Game.Content;

namespace Game.Enemy
{
    public readonly struct EnemyDamageRequest
    {
        public ContentId SourceId { get; }
        public float Amount { get; }

        public EnemyDamageRequest(ContentId sourceId, float amount)
        {
            if (!sourceId.IsValid)
                throw new ArgumentException("Enemy damage requires a valid source content id.", nameof(sourceId));
            NumericValidation.ValidateNonNegative(amount, nameof(amount));

            SourceId = sourceId;
            Amount = amount;
        }
    }
}
