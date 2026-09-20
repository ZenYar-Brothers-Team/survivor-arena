using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Enemy
{
    public sealed class WavePhaseDefinition
    {
        public ContentId Id { get; }
        public string DisplayName { get; }
        public WavePhaseTag Tag { get; }
        public float DurationSeconds { get; }
        public float SpawnIntervalSeconds { get; }
        public int MaxAliveEnemies { get; }
        public IReadOnlyList<WaveCompositionEntry> Composition { get; }
        public WaveEnemyModifiers Modifiers { get; }

        public WavePhaseDefinition(
            ContentId id,
            string displayName,
            WavePhaseTag tag,
            float durationSeconds,
            float spawnIntervalSeconds,
            int maxAliveEnemies,
            IReadOnlyList<WaveCompositionEntry> composition,
            WaveEnemyModifiers modifiers = null)
        {
            if (!id.IsValid)
                throw new ArgumentException("Wave phase requires a valid id.", nameof(id));
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Wave phase requires a display name for the HUD.", nameof(displayName));
            if (!Enum.IsDefined(typeof(WavePhaseTag), tag))
                throw new ArgumentOutOfRangeException(nameof(tag));
            NumericValidation.ValidatePositive(durationSeconds, nameof(durationSeconds));
            NumericValidation.ValidatePositive(spawnIntervalSeconds, nameof(spawnIntervalSeconds));
            NumericValidation.ValidateCount(maxAliveEnemies, nameof(maxAliveEnemies));
            if (composition == null || composition.Count == 0)
                throw new ArgumentException("Wave phase requires at least one composition entry.", nameof(composition));

            var copy = new WaveCompositionEntry[composition.Count];
            for (var i = 0; i < copy.Length; i++)
            {
                copy[i] = composition[i] ?? throw new ArgumentException(
                    "Wave composition cannot contain null entries.", nameof(composition));
                for (var previous = 0; previous < i; previous++)
                {
                    if (copy[previous].Enemy.Id == copy[i].Enemy.Id)
                        throw new ArgumentException(
                            $"Wave phase '{id}' lists enemy '{copy[i].Enemy.Id}' more than once.", nameof(composition));
                }
            }

            Id = id;
            DisplayName = displayName;
            Tag = tag;
            DurationSeconds = durationSeconds;
            SpawnIntervalSeconds = spawnIntervalSeconds;
            MaxAliveEnemies = maxAliveEnemies;
            Composition = copy;
            Modifiers = modifiers ?? WaveEnemyModifiers.Identity;
        }
    }
}
