namespace Game.Traps.Json
{
    public sealed class TrapLayoutData
    {
        public int? ReferenceSeed { get; set; }
        /// <summary>Activation, rage and projectile cutoff radius around the player, in screen widths.</summary>
        public float? RadiusScreenWidths { get; set; }
        public float? PlayerHitRadius { get; set; }
        public int? MaxActiveProjectiles { get; set; }
        public float? EdgeMargin { get; set; }
        public float? StartClearRadius { get; set; }
        public float? MinGap { get; set; }
        public float? ObstacleClearance { get; set; }
        /// <summary>Free distance a fixed-heading trap needs in front of its first shot.</summary>
        public float? ForwardClearance { get; set; }
        public int? PlacementAttempts { get; set; }
        public TrapRageData Rage { get; set; }
        /// <summary>Optional screen-based scatter: when present the random traps are placed per screen cell (types act as weights).</summary>
        public TrapDensityData Density { get; set; }
        public TrapModelData[] Models { get; set; }
        /// <summary>Optional sprite ids by key: each projectile visual key (spear, bolt, ...) and "barrel"; absent = placeholder shapes.</summary>
        public System.Collections.Generic.Dictionary<string, string> Sprites { get; set; }
        public TrapStartBarrelData[] StartBarrels { get; set; }
        public TrapStartData[] StartTraps { get; set; }
        public TrapProjectileData[] Projectiles { get; set; }
        public TrapTypeData[] Types { get; set; }
        public TrapBarrelData Barrels { get; set; }
    }
}
