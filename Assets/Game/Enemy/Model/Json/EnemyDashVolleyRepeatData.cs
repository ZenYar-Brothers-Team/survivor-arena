namespace Game.Enemy.Json
{
    /// <summary>Optional repeat of the dash-end ring; every field is required when the object is present.</summary>
    public sealed class EnemyDashVolleyRepeatData
    {
        public int? EveryNthDash { get; set; }
        public float? BelowHealthFraction { get; set; }
        public float? DelaySeconds { get; set; }
        public float? RotationDegrees { get; set; }
    }
}
