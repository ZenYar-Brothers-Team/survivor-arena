namespace Game.Enemy.Json
{
    public sealed class EnemyMovementProfileData
    {
        public string Kind { get; set; }
        public float PreferredDistance { get; set; }
        public float DistanceTolerance { get; set; }
        public float LateralStrength { get; set; } = 1f;
        public float CycleSeconds { get; set; } = 1f;
        public float DashTelegraphSeconds { get; set; } = 0.5f;
        public float DashDurationSeconds { get; set; } = 0.4f;
        public float DashCooldownSeconds { get; set; } = 3f;
        public float DashSpeedMultiplier { get; set; } = 3f;
    }
}
