using System;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>A per-spawn movement override. Unclaimed probability keeps the definition's primary movement.</summary>
    public sealed class EnemyMovementVariant
    {
        public float Chance { get; }
        public EnemyMovementProfile Movement { get; }

        public EnemyMovementVariant(float chance, EnemyMovementProfile movement)
        {
            NumericValidation.ValidateNonNegative(chance, nameof(chance));
            if (chance > 1f) throw new ArgumentOutOfRangeException(nameof(chance));
            Chance = chance;
            Movement = movement ?? throw new ArgumentNullException(nameof(movement));
        }
    }
}
