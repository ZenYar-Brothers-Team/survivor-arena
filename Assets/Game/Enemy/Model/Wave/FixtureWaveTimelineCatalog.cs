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
            BlobBreakupData profile = null;
            for (var i = 0; i < data.Phases.Length; i++)
                if (data.Phases[i].BlobBreakup != null)
                {
                    profile = JsonContentFile.Load<BlobBreakupData>("Content/Waves/ProductionBlobBreakupProfile")
                        ?? throw new InvalidOperationException("Blob breakup profile is required.");
                    break;
                }
            var phases = new WavePhaseDefinition[data.Phases.Length];
            for (var i = 0; i < phases.Length; i++)
                phases[i] = ToPhase(data.Phases[i], profile);

            var hooks = new WaveHookDefinition[data.Hooks?.Length ?? 0];
            for (var i = 0; i < hooks.Length; i++)
                hooks[i] = new WaveHookDefinition(data.Hooks[i].Kind, data.Hooks[i].TimeSeconds);

            var opening = data.OpeningSpawn == null ? null : new WaveOpeningSpawnDefinition(
                data.OpeningSpawn.DurationSeconds ?? throw new InvalidOperationException("Opening spawn requires durationSeconds."),
                data.OpeningSpawn.ScreenMargin ?? throw new InvalidOperationException("Opening spawn requires screenMargin."));
            var openingIntensity = data.OpeningIntensity == null ? null : new WaveOpeningIntensityDefinition(
                data.OpeningIntensity.DurationSeconds ?? throw new InvalidOperationException("Opening intensity requires durationSeconds."),
                data.OpeningIntensity.RateMultiplier ?? throw new InvalidOperationException("Opening intensity requires rateMultiplier."));

            return new WaveTimelineDefinition(data.Id,
                data.Seed ?? throw new InvalidOperationException("Wave timeline requires seed."), data.SpawnRadius,
                data.MaxAliveEnemies ?? throw new InvalidOperationException("Wave timeline requires maxAliveEnemies."), phases, hooks,
                opening,
                data.SpawnOppositeBias ?? throw new InvalidOperationException("Wave timeline requires spawnOppositeBias."),
                openingIntensity);
        }

        private static WavePhaseDefinition ToPhase(WavePhaseData data, BlobBreakupData profile)
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

            var breakup = data.BlobBreakup;
            if (breakup != null && (string.IsNullOrWhiteSpace(breakup.ProfileId) ||
                !string.Equals(breakup.ProfileId, profile?.Id, StringComparison.Ordinal)))
                throw new InvalidOperationException($"Unknown blob breakup profile '{breakup.ProfileId}'.");
            var enemyIds = breakup?.EnemyIds;
            var filter = enemyIds == null ? null : Array.ConvertAll(enemyIds, id => new Game.Content.ContentId(id));
            var breakupDefinition = breakup == null ? null : new BlobBreakupDefinition(
                profile.CheckIntervalSeconds ?? throw new InvalidOperationException("Blob breakup profile requires checkIntervalSeconds."),
                profile.MinimumClusterCount ?? throw new InvalidOperationException("Blob breakup profile requires minimumClusterCount."),
                profile.SelectionFraction ?? throw new InvalidOperationException("Blob breakup profile requires selectionFraction."),
                profile.MaxSelected ?? throw new InvalidOperationException("Blob breakup profile requires maxSelected."),
                profile.ConeHalfAngleDegrees ?? throw new InvalidOperationException("Blob breakup profile requires coneHalfAngleDegrees."),
                profile.OvershootDistance ?? throw new InvalidOperationException("Blob breakup profile requires overshootDistance."),
                profile.StaggerSeconds ?? throw new InvalidOperationException("Blob breakup profile requires staggerSeconds."),
                profile.ManeuverSeconds ?? throw new InvalidOperationException("Blob breakup profile requires maneuverSeconds."), filter);

            return new WavePhaseDefinition(
                data.Id,
                data.DisplayName,
                data.Tag,
                data.DurationSeconds,
                data.SpawnIntervalSeconds,
                composition,
                modifiers, mode, burst, breakupDefinition);
        }
    }
}
