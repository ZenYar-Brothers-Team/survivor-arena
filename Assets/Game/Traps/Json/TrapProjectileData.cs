namespace Game.Traps.Json
{
    /// <summary>Authoring values of one trap projectile kind; sizes and speeds are world units and units per second.</summary>
    public sealed class TrapProjectileData
    {
        public string Id { get; set; }
        /// <summary>Art key (spear, bolt, blade, spikeball); several projectile kinds may share one sprite.</summary>
        public string Visual { get; set; }
        public float? Radius { get; set; }
        public float? Speed { get; set; }
        public float? Damage { get; set; }
        public float? SpinDegreesPerSecond { get; set; }
        /// <summary>Optional boomerang: seconds after which the projectile flies back to its trap; absent = never returns.</summary>
        public float? ReturnAfterSeconds { get; set; }
        /// <summary>Optional range in world units after which the projectile fades; absent = limited only by the scene cutoff.</summary>
        public float? MaxDistance { get; set; }
    }
}
