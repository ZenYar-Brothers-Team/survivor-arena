namespace Game.Enemy.Json
{
    // No tuning defaults live here: every field the chosen movement kind reads must be
    // written in the config file (FixtureEnemyCatalog rejects a missing one by name).
    // Fields the kind never reads may be omitted.
    public sealed class EnemyMovementProfileData
    {
        public string Kind { get; set; }
        public float? PreferredDistance { get; set; }
        public float? DistanceTolerance { get; set; }
        public float? LateralStrength { get; set; }
        public float? CycleSeconds { get; set; }
        public float? DashTelegraphSeconds { get; set; }
        public float? DashDurationSeconds { get; set; }
        public float? DashCooldownSeconds { get; set; }
        public float? DashSpeedMultiplier { get; set; }
    }
}
