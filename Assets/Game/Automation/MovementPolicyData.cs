namespace Game.Automation
{
    /// <summary>Technical bot parameters in simulation seconds and world units, not gameplay balance.</summary>
    public sealed class MovementPolicyData
    {
        public string Id { get; set; }
        public int? Version { get; set; }
        public float? DecisionIntervalSeconds { get; set; }
        public float? ObservationRadius { get; set; }
        public float? PredictionSeconds { get; set; }
        public float? ObstaclePadding { get; set; }
        public float? StuckSeconds { get; set; }
        public float? ArcOffsetWorldUnits { get; set; }
        public int? CrowdMinEnemies { get; set; }
        public float? CrowdRadius { get; set; }
        public float? LureSeconds { get; set; }
        public float? SweepSeconds { get; set; }
        public float? CollectSeconds { get; set; }
    }
}
