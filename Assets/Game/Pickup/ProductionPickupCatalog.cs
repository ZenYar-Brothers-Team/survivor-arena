using Game.Content.Json;

namespace Game.Pickup
{
    /// <summary>
    /// Production world pickups for FIELD-001: PICKUP-001 potion (F1-04) and PICKUP-002 Book (F1-07 binding),
    /// generated from the approved baseline by scripts/generate_field001_content.py.
    /// </summary>
    public static class ProductionPickupCatalog
    {
        public const string ResourcePath = "Content/Pickups/ProductionPickups";

        public static FixturePickupCatalog Create() => FixturePickupCatalog.FromJson(JsonContentFile.ReadText(ResourcePath));
    }
}
