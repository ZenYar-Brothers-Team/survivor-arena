using Game.Content;
using NUnit.Framework;

namespace Game.Pickup.Tests
{
    /// <summary>F1-04: PICKUP-001 potion and PICKUP-002 Book production data (baseline v1).</summary>
    public sealed class ProductionPickupCatalogTests
    {
        [Test]
        public void Potion_And_Book_UseApprovedValues()
        {
            var catalog = ProductionPickupCatalog.Create();
            Assert.AreEqual("PICKUP-001", catalog.Potion.Id.ToString());
            Assert.AreEqual(PickupRewardKind.Potion, catalog.Potion.Kind);
            Assert.AreEqual(18f, catalog.Potion.Healing);
            Assert.AreEqual(0.4f, catalog.Potion.ContactRadius, 1e-5f);
            Assert.IsNull(catalog.Potion.LifetimeSeconds, "Potion stays until picked up or run end.");
            Assert.AreEqual("PICKUP-001-VISUAL", catalog.Potion.Visual.Id.ToString());
            Assert.AreEqual("PICKUP-002", catalog.Book.Id.ToString());
            Assert.AreEqual(0f, catalog.Book.Healing);
            Assert.AreEqual(0.015f, catalog.BaseChance, 1e-6f);
        }

        [Test]
        public void PotionChance_AppliesRelativeMultiplierOnce()
        {
            var catalog = ProductionPickupCatalog.Create();
            Assert.AreEqual(0.024f, catalog.Chance(new ContentId("ENEMY-001"), new ContentId("FIELD-001"), 1.6f), 1e-6f);
            Assert.AreEqual(0.02775f, catalog.Chance(new ContentId("ENEMY-003"), new ContentId("FIELD-001"), 1.85f), 1e-6f);
        }
    }
}
