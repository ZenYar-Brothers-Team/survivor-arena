namespace Game.Traps.Json
{
    public sealed class TrapBarrelData
    {
        public string Id { get; set; }
        public int? Count { get; set; }
        /// <summary>Fraction (0..1) of the barrels that explode; the rest look identical and do nothing.</summary>
        public float? ExplosiveShare { get; set; }
        public float? BodyRadius { get; set; }
        public float? TriggerRadius { get; set; }
        public float? FuseSeconds { get; set; }
        public float? BlastRadius { get; set; }
        public float? Damage { get; set; }
    }
}
