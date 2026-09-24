namespace Game.Enemy.Json
{
    /// <summary>Boss-owned attack payload; the carrier reuses the boss body values and never spawns on its own.</summary>
    public sealed class BossAttackData
    {
        public string Id { get; set; }
        public EnemyAttackProfileData Attack { get; set; }
    }
}
