namespace Game.Pickup.Json
{
    public sealed class PickupData
    {
        public string Id { get; set; }
        public PickupRewardKind? Kind { get; set; }
        public float? Healing { get; set; }
        public float? ContactRadius { get; set; }
        public float? LifetimeSeconds { get; set; }
        public string Marker { get; set; }
        public float[] Color { get; set; }
        public float? MarkerSize { get; set; }
        public string VisualId { get; set; }
        public float? VisualScale { get; set; }
    }
}
