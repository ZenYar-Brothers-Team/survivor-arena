using System.Linq;
using System.Threading.Tasks;
using Game.Content;
using Game.Meta;
using NUnit.Framework;

namespace Game.Bootstrap.Tests
{
    /// <summary>Test (dev) fields: always available, flagged for the field screen and listed after the designed fields.</summary>
    public sealed class ProductionFieldDevBlobsContentTests
    {
        private static RuntimeContentCatalog Catalog => RuntimeContentCatalog.CreateProduction();

        [Test]
        public void TestFields_AreListedAfterEveryDesignedField_AndNamedAfterTheFieldTheyTest()
        {
            var catalog = Catalog;
            var rules = MetaCatalog.Load().Unlocks;
            var ids = catalog.Fields.Roster.AllFields.Select(f => f.Id.ToString()).ToList();
            var firstTest = ids.FindIndex(id => rules[id].Condition == "dev");
            Assert.Greater(firstTest, 0, "At least one test field exists and a designed field comes first.");
            Assert.IsTrue(ids.Skip(firstTest).All(id => rules[id].Condition == "dev"), "Test fields sit below all designed fields.");
            Assert.IsTrue(catalog.Fields.Roster.AllFields.Where(f => rules[f.Id.ToString()].Condition == "dev").All(f => f.IsTest),
                "Every dev-condition field is flagged as a test field for the field screen.");
            Assert.IsFalse(catalog.Fields.Roster.AllFields.Any(f => f.IsTest && rules[f.Id.ToString()].Condition != "dev"));
            Assert.AreEqual("Тест 06", catalog.Fields.Roster.AllFields.Single(f => f.Id.ToString() == "FIELD-DEV-ZONES").DisplayName);
        }

        [Test]
        public async Task DevFields_AreAlwaysAvailable_OnAFreshProfile_WhileLaterFieldsStayLocked()
        {
            var profile = new ProfileService(MetaCatalog.Load(), new MemoryProfileStore());
            await profile.LoadAsync();
            var access = new ProfileAccessProvider(profile);
            Assert.AreEqual("dev", profile.Catalog.Unlocks["FIELD-DEV-ZONES"].Condition);
            Assert.IsNull(access.GetLockReason(new ContentId("FIELD-DEV-ZONES")));
            Assert.IsNull(access.GetLockReason(new ContentId("FIELD-001")));
            Assert.IsNotNull(access.GetLockReason(new ContentId("FIELD-002")));
        }
    }
}
