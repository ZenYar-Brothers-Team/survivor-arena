using System;
using Game.Content;

namespace Game.Enemy
{
    public sealed class WaveCompositionEntry
    {
        public ContentRef<EnemyDefinition> Enemy { get; }
        public float Weight { get; }

        public WaveCompositionEntry(ContentId enemyId, float weight)
        {
            if (!enemyId.IsValid)
                throw new ArgumentException("Wave composition requires a valid enemy id.", nameof(enemyId));
            NumericValidation.ValidatePositive(weight, nameof(weight));

            Enemy = new ContentRef<EnemyDefinition>(enemyId);
            Weight = weight;
        }
    }
}
