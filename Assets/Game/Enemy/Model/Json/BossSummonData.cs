namespace Game.Enemy.Json
{
    /// <summary>Boss summon of an ordinary enemy by id (DECISION-0066); every field is required.</summary>
    public sealed class BossSummonData
    {
        public string EnemyId { get; set; }
        public int? Count { get; set; }
        public float? SpawnDistance { get; set; }
        public int? MaxAlive { get; set; }
        public float? TelegraphSeconds { get; set; }
        public float[] MarkerColor { get; set; }
    }
}
