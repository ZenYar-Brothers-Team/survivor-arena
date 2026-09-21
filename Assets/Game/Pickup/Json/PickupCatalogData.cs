using System.Collections.Generic;
namespace Game.Pickup.Json
{
    public sealed class PickupCatalogData
    {
        public PickupData[] Pickups { get; set; }
        public string PotionId { get; set; }
        public string BookId { get; set; }
        public float? BaseChance { get; set; }
        public int? Seed { get; set; }
        public float? PlacementSkin { get; set; }
        public float? FeedbackSeconds { get; set; }
        public Dictionary<string, float> EnemyChances { get; set; }
        public Dictionary<string, float> FieldChances { get; set; }
    }
}
