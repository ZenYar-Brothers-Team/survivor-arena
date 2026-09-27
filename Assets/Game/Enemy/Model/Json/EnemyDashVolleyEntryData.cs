namespace Game.Enemy.Json
{
    /// <summary>One dash-end action (DECISION-0066, E4): an attack or a zone around the boss.</summary>
    public sealed class EnemyDashVolleyEntryData
    {
        public float? DelaySeconds { get; set; }
        public string Orientation { get; set; }
        public EnemyAttackProfileData Attack { get; set; }
        public BossZoneData Zone { get; set; }
    }
}
