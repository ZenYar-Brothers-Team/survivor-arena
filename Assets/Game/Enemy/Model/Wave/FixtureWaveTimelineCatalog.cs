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
            var data = JsonContentFile.Load<WaveTimelineData>(ResourcePath);
            var phases = new WavePhaseDefinition[data.Phases.Length];
            for (var i = 0; i < phases.Length; i++)
                phases[i] = ToPhase(data.Phases[i]);

            var hooks = new WaveHookDefinition[data.Hooks?.Length ?? 0];
            for (var i = 0; i < hooks.Length; i++)
                hooks[i] = new WaveHookDefinition(data.Hooks[i].Kind, data.Hooks[i].TimeSeconds);

            return new WaveTimelineDefinition(data.Id, data.Seed, data.SpawnRadius, phases, hooks);
        }

        private static WavePhaseDefinition ToPhase(WavePhaseData data)
        {
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
                modifiers);
        }
    }
}
