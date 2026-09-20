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
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), "Damage must be finite and non-negative.");

            SourceId = sourceId;
            Amount = amount;
        }
    }
}
