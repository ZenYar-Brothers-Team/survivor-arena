namespace Game.Enemy.Json
{
    /// <summary>Dash-end entries used instead while health is strictly below the fraction (DECISION-0066, E4).</summary>
    public sealed class EnemyDashVolleyReplacementData
    {
        public float? BelowHealthFraction { get; set; }
        public EnemyDashVolleyEntryData[] Attacks { get; set; }
    }
}
