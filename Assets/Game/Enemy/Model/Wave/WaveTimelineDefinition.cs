using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Enemy
{
    // Ordered phases that drive continuous spawning. The last phase holds until the
    // run ends, so a timeline shorter than the run still has defined behavior.
    public sealed class WaveTimelineDefinition : IContentDefinition, IReferencesContent
    {
        public ContentId Id { get; }
        public int Seed { get; }
        public float SpawnRadius { get; }
        public IReadOnlyList<WavePhaseDefinition> Phases { get; }
        public IReadOnlyList<WaveHookDefinition> Hooks { get; }
        /// <summary>Optional opening screen-edge spawn window; null keeps the spawn radius from the start.</summary>
        public WaveOpeningSpawnDefinition OpeningSpawn { get; }
        public float TotalDurationSeconds { get; }

        public WaveTimelineDefinition(
            ContentId id,
            int seed,
            float spawnRadius,
            IReadOnlyList<WavePhaseDefinition> phases,
            IReadOnlyList<WaveHookDefinition> hooks = null,
            WaveOpeningSpawnDefinition openingSpawn = null)
        {
            if (!id.IsValid)
                throw new ArgumentException("Wave timeline requires a valid id.", nameof(id));
            NumericValidation.ValidatePositive(spawnRadius, nameof(spawnRadius));
            if (phases == null || phases.Count == 0)
                throw new ArgumentException("Wave timeline requires at least one phase.", nameof(phases));

            var phaseCopy = new WavePhaseDefinition[phases.Count];
            var total = 0f;
            for (var i = 0; i < phaseCopy.Length; i++)
            {
                phaseCopy[i] = phases[i] ?? throw new ArgumentException(
                    "Wave timeline cannot contain null phases.", nameof(phases));
                for (var previous = 0; previous < i; previous++)
                {
                    if (phaseCopy[previous].Id == phaseCopy[i].Id)
                        throw new ArgumentException($"Duplicate wave phase id '{phaseCopy[i].Id}'.", nameof(phases));
                }
                total += phaseCopy[i].DurationSeconds;
            }

            var hookCopy = new List<WaveHookDefinition>(hooks?.Count ?? 0);
            if (hooks != null)
            {
                for (var i = 0; i < hooks.Count; i++)
                {
                    var hook = hooks[i] ?? throw new ArgumentException(
                        "Wave timeline cannot contain null hooks.", nameof(hooks));
                    for (var previous = 0; previous < hookCopy.Count; previous++)
                    {
                        if (hookCopy[previous].Kind == hook.Kind)
                            throw new ArgumentException($"Wave timeline declares hook '{hook.Kind}' more than once.", nameof(hooks));
                    }
                    hookCopy.Add(hook);
                }
                hookCopy.Sort((left, right) =>
                {
                    var time = left.TimeSeconds.CompareTo(right.TimeSeconds);
                    return time != 0 ? time : left.Kind.CompareTo(right.Kind);
                });
            }

            Id = id;
            Seed = seed;
            SpawnRadius = spawnRadius;
            Phases = phaseCopy;
            Hooks = hookCopy;
            OpeningSpawn = openingSpawn;
            TotalDurationSeconds = total;
        }

        public IEnumerable<ContentReference> GetReferencedContent()
        {
            for (var phaseIndex = 0; phaseIndex < Phases.Count; phaseIndex++)
            {
                var composition = Phases[phaseIndex].Composition;
                for (var i = 0; i < composition.Count; i++)
                    yield return composition[i].Enemy.ToReference();
            }
        }
    }
}
