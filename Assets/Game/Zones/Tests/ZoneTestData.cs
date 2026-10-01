using System.Linq;
using Game.Zones.Json;

namespace Game.Zones.Tests
{
    /// <summary>Shared builders for zone tests (placeholder numbers; no balance meaning).</summary>
    internal static class ZoneTestData
    {
        public static ZoneEffectData Slow(string id = "T-SLOW", ZoneLifetimeMode mode = ZoneLifetimeMode.Permanent) => Lifetime(new ZoneEffectData
        {
            Id = id, Kind = ZoneEffectKind.Slow, Radius = 6f, Color = "#4aa3ff", PlayerMovementBonus = -0.4f,
            EnemySlowFraction = 0.5f, EnemySlowSeconds = 0.3f
        }, mode);

        public static ZoneEffectData Haste(string id = "T-HASTE", ZoneLifetimeMode mode = ZoneLifetimeMode.Permanent) => Lifetime(new ZoneEffectData
        {
            Id = id, Kind = ZoneEffectKind.Haste, Radius = 5f, Color = "#6df0c2", PlayerMovementBonus = 0.35f
        }, mode);

        public static ZoneEffectData Regeneration(string id = "T-REGEN") => Lifetime(new ZoneEffectData
        {
            Id = id, Kind = ZoneEffectKind.Regeneration, Radius = 5f, Color = "#7bd96a", PlayerRegenerationPerSecond = 4f
        }, ZoneLifetimeMode.Permanent);

        public static ZoneEffectData Arcane(string id = "T-ARCANE") => Lifetime(new ZoneEffectData
        {
            Id = id, Kind = ZoneEffectKind.ArcanePower, Radius = 5f, Color = "#c28cff", PlayerSkillDamageBonus = 0.5f,
            PlayerActionSpeedBonus = 0.25f
        }, ZoneLifetimeMode.Permanent);

        public static ZoneEffectData Rift(string id = "T-RIFT") => Lifetime(new ZoneEffectData
        {
            Id = id, Kind = ZoneEffectKind.Rift, Radius = 5f, Color = "#ff5a5a", PlayerDamagePerSecond = 6f, EnemyDamagePerSecond = 20f
        }, ZoneLifetimeMode.Permanent);

        public static ZoneEffectData Portal(string id = "T-PORTAL") => Lifetime(new ZoneEffectData
        {
            Id = id, Kind = ZoneEffectKind.Portal, Radius = 3f, Color = "#ffd45a", PortalCooldownSeconds = 3f, PortalExitDistance = 2f,
            PortalMinPairDistance = 30f
        }, ZoneLifetimeMode.Permanent);

        /// <summary>Marks an effect permanent, or pulsing with a 30 s period, 20 s shown (3 s fades).</summary>
        public static ZoneEffectData Lifetime(ZoneEffectData data, ZoneLifetimeMode mode)
        {
            data.Lifetime = mode;
            if (mode == ZoneLifetimeMode.Pulsing)
            {
                data.PulsePeriodSeconds = 30f; data.PulseVisibleSeconds = 20f; data.PulseFadeSeconds = 3f;
            }
            return data;
        }

        public static ZoneLayoutData Layout(ZoneEffectData[] effects, params (string effectId, int count)[] zones) => new ZoneLayoutData
        {
            EdgeMargin = 6f, StartClearRadius = 8f, MinGap = 4f, ObstacleClearance = 1.5f, PlacementAttempts = 800, MaxRestarts = 20,
            ReferenceSeed = 6, Effects = effects,
            Zones = zones.Select(z => new ZoneCountData { EffectId = z.effectId, Count = z.count }).ToArray()
        };
    }
}
