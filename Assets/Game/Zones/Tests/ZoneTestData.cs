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

        /// <summary>Speed burst: swells for 4 s of a 40 s cycle, goes off, flashes 0.5 s; +60% speed for 8 s.</summary>
        public static ZoneEffectData SpeedBurst(string id = "T-BURST") => new ZoneEffectData
        {
            Id = id, Kind = ZoneEffectKind.SpeedBurst, Radius = 8f, Color = "#ffe14a", Lifetime = ZoneLifetimeMode.Burst,
            PulsePeriodSeconds = 40f, TelegraphSeconds = 4f, FlashSeconds = 0.5f, PlayerMovementBonus = 0.6f, PlayerBuffSeconds = 8f
        };

        /// <summary>Ward: 80% less incoming damage while inside.</summary>
        public static ZoneEffectData Protection(string id = "T-WARD", ZoneLifetimeMode mode = ZoneLifetimeMode.Permanent) => Lifetime(new ZoneEffectData
        {
            Id = id, Kind = ZoneEffectKind.Protection, Radius = 6f, Color = "#9fd3ff", PlayerIncomingDamageReduction = 0.8f
        }, mode);

        /// <summary>Charging altar: 20 s inside to fill, 8 s outside to drain; +100% skill damage and +30% action speed at full charge.</summary>
        public static ZoneEffectData Charge(string id = "T-CHARGE", ZoneLifetimeMode mode = ZoneLifetimeMode.Permanent) => Lifetime(new ZoneEffectData
        {
            Id = id, Kind = ZoneEffectKind.Charge, Radius = 5f, Color = "#ffb347", ChargeSecondsToMax = 20f, ChargeDecaySeconds = 8f,
            ChargeSkillDamageBonus = 1f, ChargeActionSpeedBonus = 0.3f,
            Polarity = ZoneAltarPolarity.Positive
        }, mode);

        /// <summary>A cycling altar: 90 s cycle, 24 s on (3 s fades), never moves.</summary>
        public static ZoneEffectData Altar(string id = "T-ALTAR") => new ZoneEffectData
        {
            Id = id, Kind = ZoneEffectKind.Haste, Radius = 4f, Color = "#6df0c2", Lifetime = ZoneLifetimeMode.Cycling,
            PulsePeriodSeconds = 90f, PulseVisibleSeconds = 24f, PulseFadeSeconds = 3f, PlayerMovementBonus = 0.5f,
            Polarity = ZoneAltarPolarity.Positive
        };

        /// <summary>Strike altar: 20 s volleys of 2 circles (radius 2) inside radius 8; 3 s warning, 0.5 s flash; 10 to the player, 50 to enemies.</summary>
        public static ZoneEffectData Strike(string id = "T-STRIKE", ZoneAltarPolarity polarity = ZoneAltarPolarity.Neutral) => new ZoneEffectData
        {
            Id = id, Kind = ZoneEffectKind.Strike, Radius = 8f, Color = "#ff5a5a", Lifetime = ZoneLifetimeMode.Permanent,
            StrikePeriodSeconds = 20f, StrikeTelegraphSeconds = 3f, StrikeFlashSeconds = 0.5f, StrikeCount = 2, StrikeRadius = 2f,
            StrikePlayerDamage = 10f, StrikeEnemyDamage = 50f, Polarity = polarity
        };

        /// <summary>
        /// Shrine: 10 s inside to fire, 5 s outside to drain, 60 s cooldown; rewards +50% speed for 8 s, 25% healing, a 100 blast
        /// within 6 and 50% damage reduction for 5 s.
        /// </summary>
        public static ZoneEffectData Shrine(string id = "T-SHRINE") => new ZoneEffectData
        {
            Id = id, Kind = ZoneEffectKind.Shrine, Radius = 5f, Color = "#ffe08a", Lifetime = ZoneLifetimeMode.Permanent,
            ShrineChargeSeconds = 10f, ShrineDecaySeconds = 5f, ShrineCooldownSeconds = 60f,
            RewardBuffSeconds = 8f, RewardMovementBonus = 0.5f, RewardHealFraction = 0.25f,
            RewardBlastDamage = 100f, RewardBlastRadius = 6f, RewardShieldSeconds = 5f, RewardIncomingDamageReduction = 0.5f,
            Polarity = ZoneAltarPolarity.Positive
        };

        /// <summary>Cycling negative altar that strengthens enemies inside (mirror of a player-side altar).</summary>
        public static ZoneEffectData EnemyAltar(ZoneEffectKind kind, string id) => new ZoneEffectData
        {
            Id = id, Kind = kind, Radius = 4f, Color = "#ed756b", Lifetime = ZoneLifetimeMode.Cycling,
            PulsePeriodSeconds = 90f, PulseVisibleSeconds = 24f, PulseFadeSeconds = 3f, Polarity = ZoneAltarPolarity.Negative,
            EnemyMovementBonus = kind == ZoneEffectKind.EnemyHaste ? 0.5f : (float?)null,
            EnemyRegenerationPerSecond = kind == ZoneEffectKind.EnemyRegeneration ? 5f : (float?)null,
            EnemyIncomingDamageReduction = kind == ZoneEffectKind.EnemyProtection ? 0.8f : (float?)null,
            EnemyDamageBonus = kind == ZoneEffectKind.EnemyPower ? 0.5f : (float?)null
        };

        /// <summary>Shrine whose only reward is picked-up experience x5 for 30 s; fills in 4 s.</summary>
        public static ZoneEffectData ExperienceShrine(string id = "T-XP-SHRINE") => new ZoneEffectData
        {
            Id = id, Kind = ZoneEffectKind.Shrine, Radius = 5f, Color = "#ffe08a", Lifetime = ZoneLifetimeMode.Permanent,
            ShrineChargeSeconds = 4f, ShrineDecaySeconds = 4f, ShrineCooldownSeconds = 60f,
            RewardExperienceSeconds = 30f, RewardExperienceMultiplier = 5f, Polarity = ZoneAltarPolarity.Positive
        };

        /// <summary>Experience field on the random schedule: x5 picked-up experience while inside; Academy-style preparation timing.</summary>
        public static ZoneEffectData ExperienceField(string id = "T-XP-FIELD") => new ZoneEffectData
        {
            Id = id, Kind = ZoneEffectKind.Experience, Radius = 6f, MinRadius = 2f, Color = "#ff9f43", Lifetime = ZoneLifetimeMode.Pulsing,
            PulsePeriodSeconds = 30f, PulseVisibleSeconds = 22f, PulseFadeSeconds = 3f, PulsePrepareSeconds = 5f, PulseIdleVisibility = .14f,
            RelocatesBetweenCycles = true, PlayerExperienceMultiplier = 5f
        };

        /// <summary>Portal pair on the random schedule: its ends lie exactly one screen height apart.</summary>
        public static ZoneEffectData ScheduledPortal(string id = "T-SCHED-PORTAL") => new ZoneEffectData
        {
            Id = id, Kind = ZoneEffectKind.Portal, Radius = 3.5f, MinRadius = 1.5f, Color = "#ffd45a", Lifetime = ZoneLifetimeMode.Pulsing,
            PulsePeriodSeconds = 40f, PulseVisibleSeconds = 25f, PulseFadeSeconds = 3f, PulsePrepareSeconds = 5f, PulseIdleVisibility = .14f,
            RelocatesBetweenCycles = true, PortalCooldownSeconds = 3f, PortalExitDistance = 2f, PortalPairScreenHeights = 1f
        };

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
            EdgeMargin = 6f, StartClearRadius = 8f, MinGap = 4f, ObstacleClearance = 1.5f, ActiveScreenMargin = 0.5f,
            PlacementAttempts = 800, MaxRestarts = 20,
            ReferenceSeed = 6, Effects = effects,
            Zones = zones.Select(z => new ZoneCountData { EffectId = z.effectId, Count = z.count }).ToArray()
        };
    }
}
