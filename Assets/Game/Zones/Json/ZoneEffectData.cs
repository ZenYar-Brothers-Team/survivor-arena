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
        // Portal
        public float? PortalCooldownSeconds { get; set; }
        public float? PortalExitDistance { get; set; }
        public float? PortalMinPairDistance { get; set; }
    }
}
