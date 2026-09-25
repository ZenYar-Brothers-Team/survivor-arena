using System;
using Game.Content.Json;
using Game.Enemy.Json;

namespace Game.Enemy
{
    // Non-production wave schedule, config-driven: see
    // Resources/Content/Waves/FixtureWaveTimeline.json and AGENTS.md's content-config rule.
    // Canonical 15-minute schedules stay CG-02 gated (IP-24).
    public static class FixtureWaveTimelineCatalog
    {
        private const string ResourcePath = "Content/Waves/FixtureWaveTimeline";

        public static WaveTimelineDefinition Create()
        {
            return FromJson(JsonContentFile.ReadText(ResourcePath));
        }

        public static WaveTimelineDefinition FromJson(string json)
        {
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<WaveTimelineData>(json, JsonContentFile.Settings)
                ?? throw new InvalidOperationException("Wave timeline is required.");
            if (data.Phases == null) throw new InvalidOperationException("Wave timeline requires phases.");
            var phases = new WavePhaseDefinition[data.Phases.Length];
            for (var i = 0; i < phases.Length; i++)
                phases[i] = ToPhase(data.Phases[i]);

            var hooks = new WaveHookDefinition[data.Hooks?.Length ?? 0];
            for (var i = 0; i < hooks.Length; i++)
                hooks[i] = new WaveHookDefinition(data.Hooks[i].Kind, data.Hooks[i].TimeSeconds);

            var opening = data.OpeningSpawn == null ? null : new WaveOpeningSpawnDefinition(
                data.OpeningSpawn.DurationSeconds ?? throw new InvalidOperationException("Opening spawn requires durationSeconds."),
                data.OpeningSpawn.ScreenMargin ?? throw new InvalidOperationException("Opening spawn requires screenMargin."));

            return new WaveTimelineDefinition(data.Id,
                data.Seed ?? throw new InvalidOperationException("Wave timeline requires seed."), data.SpawnRadius, phases, hooks,
                opening);
        }

        private static WavePhaseDefinition ToPhase(WavePhaseData data)
        {
            var mode = data.SpawnMode ?? throw new InvalidOperationException("Wave phase requires spawnMode.");
            var burst = data.Burst == null ? null : new WaveBurstDefinition(
                data.Burst.Count ?? throw new InvalidOperationException("Burst requires count."),
                data.Burst.OffsetSeconds ?? throw new InvalidOperationException("Burst requires offsetSeconds."),
                data.Burst.WindowSeconds ?? throw new InvalidOperationException("Burst requires windowSeconds."));
            var composition = new WaveCompositionEntry[data.Composition.Length];
            for (var i = 0; i < composition.Length; i++)
                composition[i] = new WaveCompositionEntry(data.Composition[i].EnemyId, data.Composition[i].Weight);

            var neutral = WaveEnemyModifiers.Identity;
            var modifiers = data.Modifiers == null
                ? neutral
                : new WaveEnemyModifiers(
                    data.Modifiers.HealthMultiplier ?? neutral.HealthMultiplier,
                    data.Modifiers.SpeedMultiplier ?? neutral.SpeedMultiplier,
                    data.Modifiers.ContactDamageMultiplier ?? neutral.ContactDamageMultiplier,
                    data.Modifiers.AttackDamageMultiplier ?? neutral.AttackDamageMultiplier);

            return new WavePhaseDefinition(
                data.Id,
                data.DisplayName,
                data.Tag,
                data.DurationSeconds,
                data.SpawnIntervalSeconds,
                data.MaxAliveEnemies,
                composition,
                modifiers, mode, burst);
        }
    }
}
