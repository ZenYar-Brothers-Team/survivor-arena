namespace Game.Enemy.Json
{
    public sealed class BlobBreakupData
    {
        public string Id { get; set; }
        public float? CheckIntervalSeconds { get; set; }
        public int? MinimumClusterCount { get; set; }
        public float? SelectionFraction { get; set; }
        public int? MaxSelected { get; set; }
        public float? ConeHalfAngleDegrees { get; set; }
        public float? OvershootDistance { get; set; }
        public float? StaggerSeconds { get; set; }
        public float? ManeuverSeconds { get; set; }
    }

    public sealed class BlobBreakupPhaseData
    {
        public string ProfileId { get; set; }
        public string[] EnemyIds { get; set; }
    }
}
