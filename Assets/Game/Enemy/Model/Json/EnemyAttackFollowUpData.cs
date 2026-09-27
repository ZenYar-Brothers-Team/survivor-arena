namespace Game.Enemy.Json
{
    /// <summary>One extra volley after the main shot (DECISION-0066, E1); both fields are required.</summary>
    public sealed class EnemyAttackFollowUpData
    {
        public float? DelaySeconds { get; set; }
        public float? RotationDegrees { get; set; }
    }
}
