using System;
using Game.Content;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// Boss summon (DECISION-0066, family F3). Markers appear evenly around the player at <see cref="SpawnDistance"/>;
    /// after <see cref="TelegraphSeconds"/> an ordinary <see cref="Enemy"/> steps out of each. A boss life never keeps
    /// more than <see cref="MaxAlive"/> of its summons alive or pending. Summons are ordinary enemies: they give XP,
    /// do not use the regular enemy cap and outlive the boss.
    /// </summary>
    public sealed class BossSummonProfile
    {
        public EnemyDefinition Enemy { get; }
        public int Count { get; }
        public float SpawnDistance { get; }
        public int MaxAlive { get; }
        public float TelegraphSeconds { get; }
        public Color MarkerColor { get; }

        public BossSummonProfile(EnemyDefinition enemy, int count, float spawnDistance, int maxAlive, float telegraphSeconds,
            Color markerColor)
        {
            Enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
            NumericValidation.ValidateCount(count, nameof(count));
            NumericValidation.ValidatePositive(spawnDistance, nameof(spawnDistance));
            NumericValidation.ValidateCount(maxAlive, nameof(maxAlive));
            if (count > maxAlive) throw new ArgumentException("One call cannot exceed the alive limit.", nameof(count));
            NumericValidation.ValidatePositive(telegraphSeconds, nameof(telegraphSeconds));
            Count = count;
            SpawnDistance = spawnDistance;
            MaxAlive = maxAlive;
            TelegraphSeconds = telegraphSeconds;
            MarkerColor = markerColor;
        }
    }
}
