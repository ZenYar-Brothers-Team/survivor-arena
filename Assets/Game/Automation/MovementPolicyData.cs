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
    }
}
