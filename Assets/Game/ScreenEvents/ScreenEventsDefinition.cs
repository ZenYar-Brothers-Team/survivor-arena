using System;
using System.Collections.Generic;
using Game.Content;
using Game.ScreenEvents.Json;

namespace Game.ScreenEvents
{
    /// <summary>
    /// Screen events of a field (DECISION-0157): events with no object on the map that start by schedule, warn in red, then hurt the
    /// player with a percentage of maximum health. Validates the schedule against the event list: stages start at zero in ascending order,
    /// every pool entry names a known event, every event appears in some stage, and each stage has a non-rare event so the pool never
    /// runs dry while rare events are cooling down.
    /// </summary>
    public sealed class ScreenEventsDefinition
    {
        public int ReferenceSeed { get; }
        public float PlayerHitRadius { get; }
        public float FirstDelaySeconds { get; }
        public float RareMinIntervalSeconds { get; }
        public bool SuspendWhileBossAlive { get; }
        public float FairnessSpeed { get; }
        public int FairnessAttempts { get; }
        public float FairnessCellSize { get; }
        public float WavePeriodSeconds { get; }
        public float IntensityTolerance { get; }
        public float RareMinIntensity { get; }
        public IReadOnlyList<ScreenEventStage> Stages { get; }
        public IReadOnlyDictionary<string, ScreenEventDefinition> Events { get; }

        public ScreenEventsDefinition(ScreenEventsData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            ReferenceSeed = data.ReferenceSeed ?? throw new ArgumentException("screenEvents.referenceSeed is required.");
            PlayerHitRadius = data.PlayerHitRadius ?? throw new ArgumentException("screenEvents.playerHitRadius is required.");
            FirstDelaySeconds = data.FirstDelaySeconds ?? throw new ArgumentException("screenEvents.firstDelaySeconds is required.");
            RareMinIntervalSeconds = data.RareMinIntervalSeconds ?? throw new ArgumentException("screenEvents.rareMinIntervalSeconds is required.");
            SuspendWhileBossAlive = data.SuspendWhileBossAlive ?? throw new ArgumentException("screenEvents.suspendWhileBossAlive is required.");
            WavePeriodSeconds = data.WavePeriodSeconds ?? throw new ArgumentException("screenEvents.wavePeriodSeconds is required.");
            IntensityTolerance = data.IntensityTolerance ?? throw new ArgumentException("screenEvents.intensityTolerance is required.");
            RareMinIntensity = data.RareMinIntensity ?? throw new ArgumentException("screenEvents.rareMinIntensity is required.");
            FairnessSpeed = data.FairnessSpeed ?? throw new ArgumentException("screenEvents.fairnessSpeed is required.");
            FairnessAttempts = data.FairnessAttempts ?? throw new ArgumentException("screenEvents.fairnessAttempts is required.");
            FairnessCellSize = data.FairnessCellSize ?? throw new ArgumentException("screenEvents.fairnessCellSize is required.");
            NumericValidation.ValidatePositive(FairnessSpeed, nameof(FairnessSpeed));
            NumericValidation.ValidateCount(FairnessAttempts, nameof(FairnessAttempts));
            NumericValidation.ValidatePositive(FairnessCellSize, nameof(FairnessCellSize));
            NumericValidation.ValidatePositive(WavePeriodSeconds, nameof(WavePeriodSeconds));
            NumericValidation.ValidatePositive(IntensityTolerance, nameof(IntensityTolerance));
            NumericValidation.ValidateRange(RareMinIntensity, 0f, 1f, nameof(RareMinIntensity));
            NumericValidation.ValidatePositive(PlayerHitRadius, nameof(PlayerHitRadius));
            NumericValidation.ValidateNonNegative(FirstDelaySeconds, nameof(FirstDelaySeconds));
            NumericValidation.ValidateNonNegative(RareMinIntervalSeconds, nameof(RareMinIntervalSeconds));

            var events = new Dictionary<string, ScreenEventDefinition>(StringComparer.Ordinal);
            foreach (var item in data.Events ?? throw new ArgumentException("screenEvents.events is required."))
            {
                var definition = new ScreenEventDefinition(item);
                if (!events.TryAdd(definition.Id.ToString(), definition))
                    throw new ArgumentException($"Duplicate screen event '{definition.Id}'.");
            }
            if (events.Count == 0) throw new ArgumentException("screenEvents.events cannot be empty.");
            Events = events;

            var stages = new List<ScreenEventStage>();
            var used = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in data.Stages ?? throw new ArgumentException("screenEvents.stages is required."))
            {
                var pool = new List<ScreenEventPoolEntry>();
                foreach (var entry in item.Pool ?? throw new ArgumentException("A stage pool is required."))
                {
                    if (entry?.Event == null || !events.TryGetValue(entry.Event, out var definition))
                        throw new ArgumentException($"Stage pool names unknown screen event '{entry?.Event}'.");
                    if (pool.Exists(existing => existing.Event == definition))
                        throw new ArgumentException($"Screen event '{definition.Id}' is listed twice in one stage.");
                    pool.Add(new ScreenEventPoolEntry(definition, entry));
                    used.Add(entry.Event);
                }
                if (!pool.Exists(entry => !entry.Event.Rare))
                    throw new ArgumentException("Every stage needs at least one non-rare screen event.");
                var stage = new ScreenEventStage(item.FromSeconds ?? throw new ArgumentException("A stage fromSeconds is required."),
                    item.ValleyIntensity ?? throw new ArgumentException("A stage valleyIntensity is required."),
                    item.PeakIntensity ?? throw new ArgumentException("A stage peakIntensity is required."),
                    item.PauseMinSeconds ?? throw new ArgumentException("A stage pauseMinSeconds is required."),
                    item.PauseMaxSeconds ?? throw new ArgumentException("A stage pauseMaxSeconds is required."), pool);
                if (stages.Count == 0 ? stage.FromSeconds != 0f : stage.FromSeconds <= stages[stages.Count - 1].FromSeconds)
                    throw new ArgumentException("Stages must start at 0 and ascend.");
                stages.Add(stage);
            }
            if (stages.Count == 0) throw new ArgumentException("screenEvents.stages cannot be empty.");
            foreach (var id in events.Keys)
                if (!used.Contains(id)) throw new ArgumentException($"Screen event '{id}' is not used by any stage.");
            Stages = stages;
        }

        /// <summary>
        /// Intensity (0…1) <paramref name="runSeconds"/> into the run: a cosine wave that starts calm, builds to a peak and eases off
        /// every <see cref="WavePeriodSeconds"/>. Its valley and peak are blended linearly between the stages' values, so the waves
        /// grow over the run without jumps.
        /// </summary>
        public float IntensityAt(float runSeconds)
        {
            var index = 0;
            for (var i = 0; i < Stages.Count; i++)
                if (Stages[i].FromSeconds <= runSeconds) index = i;
            var stage = Stages[index];
            var valley = stage.ValleyIntensity;
            var peak = stage.PeakIntensity;
            if (index + 1 < Stages.Count)
            {
                var next = Stages[index + 1];
                var blend = (runSeconds - stage.FromSeconds) / (next.FromSeconds - stage.FromSeconds);
                valley += (next.ValleyIntensity - valley) * blend;
                peak += (next.PeakIntensity - peak) * blend;
            }
            var wave = .5f - .5f * (float)Math.Cos(2.0 * Math.PI * runSeconds / WavePeriodSeconds);
            return valley + (peak - valley) * wave;
        }

        /// <summary>The stage in force <paramref name="runSeconds"/> into the run.</summary>
        public ScreenEventStage StageAt(float runSeconds)
        {
            var current = Stages[0];
            foreach (var stage in Stages)
            {
                if (stage.FromSeconds > runSeconds) break;
                current = stage;
            }
            return current;
        }
    }
}
