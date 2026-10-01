namespace Game.Zones.Json
{
    /// <summary>Authoring values of one zone effect; each kind requires its own subset (see ZoneEffectDefinition).</summary>
    public sealed class ZoneEffectData
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public ZoneEffectKind? Kind { get; set; }
        /// <summary>Zone radius in world units.</summary>
        public float? Radius { get; set; }
        /// <summary>HTML color of the placeholder disc.</summary>
        public string Color { get; set; }
        /// <summary>Permanent zones always exist; pulsing zones fade in and out on a period.</summary>
        public ZoneLifetimeMode? Lifetime { get; set; }
        // Pulsing: one cycle is period seconds; the zone is shown for visible seconds (including both fades) and hidden otherwise.
        public float? PulsePeriodSeconds { get; set; }
        public float? PulseVisibleSeconds { get; set; }
        public float? PulseFadeSeconds { get; set; }
        // Burst: the cycle length is pulsePeriodSeconds; the disc swells for telegraphSeconds, goes off, then flashes for flashSeconds.
        public float? TelegraphSeconds { get; set; }
        public float? FlashSeconds { get; set; }
        // Slow / Haste
        public float? PlayerMovementBonus { get; set; }
        public float? EnemySlowFraction { get; set; }
        public float? EnemySlowSeconds { get; set; }
        // Regeneration
        public float? PlayerRegenerationPerSecond { get; set; }
        // ArcanePower
        public float? PlayerSkillDamageBonus { get; set; }
        public float? PlayerActionSpeedBonus { get; set; }
        // Rift
        public float? PlayerDamagePerSecond { get; set; }
        public float? EnemyDamagePerSecond { get; set; }
        // Protection
        public float? PlayerIncomingDamageReduction { get; set; }
        // SpeedBurst: uses playerMovementBonus plus how long the buff lasts.
        public float? PlayerBuffSeconds { get; set; }
        // Charge: bonuses at full charge, how long standing inside takes to fill, how long leaving takes to drain.
        public float? ChargeSecondsToMax { get; set; }
        public float? ChargeDecaySeconds { get; set; }
        public float? ChargeSkillDamageBonus { get; set; }
        public float? ChargeActionSpeedBonus { get; set; }
        // Altars: whose side the zone is on (required for cycling zones, charge, strike and shrine kinds).
        public ZoneAltarPolarity? Polarity { get; set; }
        // Strike: every strikePeriodSeconds, strikeCount circles of strikeRadius warn for strikeTelegraphSeconds at random points
        // inside the zone, hit once, then flash for strikeFlashSeconds.
        public float? StrikePeriodSeconds { get; set; }
        public float? StrikeTelegraphSeconds { get; set; }
        public float? StrikeFlashSeconds { get; set; }
        public int? StrikeCount { get; set; }
        public float? StrikeRadius { get; set; }
        public float? StrikePlayerDamage { get; set; }
        public float? StrikeEnemyDamage { get; set; }
        // Shrine: seconds inside to fire, seconds outside for the progress to drain, cooldown after firing.
        public float? ShrineChargeSeconds { get; set; }
        public float? ShrineDecaySeconds { get; set; }
        public float? ShrineCooldownSeconds { get; set; }
        // Shrine rewards (at least one): timed buff, healing, blast around the shrine, timed shield.
        public float? RewardBuffSeconds { get; set; }
        public float? RewardMovementBonus { get; set; }
        public float? RewardSkillDamageBonus { get; set; }
        public float? RewardActionSpeedBonus { get; set; }
        public float? RewardHealFraction { get; set; }
        public float? RewardBlastDamage { get; set; }
        public float? RewardBlastRadius { get; set; }
        public float? RewardShieldSeconds { get; set; }
        public float? RewardIncomingDamageReduction { get; set; }
        // Portal
        public float? PortalCooldownSeconds { get; set; }
        public float? PortalExitDistance { get; set; }
        public float? PortalMinPairDistance { get; set; }
    }
}
