using System.Linq;
using System.Threading.Tasks;
using Game.Content;
using Game.Meta;
using Game.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Game.Bootstrap.Tests
{
    /// <summary>
    /// FIELD-DEV-BLOBS (field-dev-blobs-v1): a development-only test field for the blob geometry study. Spawn settings are
    /// the FIELD-001 ones; the arena is smaller and its obstacles are generated blobs.
    /// </summary>
    public sealed class ProductionFieldDevBlobsContentTests
    {
        private static RuntimeContentCatalog Catalog => RuntimeContentCatalog.CreateProduction();

        [Test]
        public void DevField_SharesFirstFieldSpawnSettings()
        {
            var catalog = Catalog;
            var first = catalog.Fields.Roster.AllFields.Single(f => f.Id.ToString() == "FIELD-001").Resolve(catalog.Registry);
            var dev = catalog.Fields.Roster.AllFields.Single(f => f.Id.ToString() == "FIELD-DEV-BLOBS").Resolve(catalog.Registry);
            Assert.AreEqual(first.Timeline.Id, dev.Timeline.Id, "Spawn settings are FIELD-001's timeline.");
            CollectionAssert.AreEqual(first.Enemies.Select(e => e.Id), dev.Enemies.Select(e => e.Id));
            CollectionAssert.AreEqual(first.Bosses.Select(b => b.Id), dev.Bosses.Select(b => b.Id));
            Assert.AreEqual("FIELD-DEV-BLOBS-ENVIRONMENT", dev.Environment.Id.ToString());
        }

        [Test]
        public void Presentation_GeneratesTwentyBlobsEveryRun_InAMoreCompactArena()
        {
            var presentation = FixtureFieldEnvironmentPresentationCatalog.Load(RuntimeContentCatalog.ProductionFieldPresentationPath)
                .Values.Single(p => p.Id.ToString() == "FIELD-DEV-BLOBS-PRESENTATION");
            Assert.AreEqual(120f, presentation.ArenaSideLength);
            Assert.IsNull(presentation.ObstacleLayout);
            Assert.AreEqual(20, presentation.BlobLayout.TotalCount);
            var layouts = new System.Collections.Generic.HashSet<string>();
            for (var seed = 0; seed < 100; seed++)
            {
                var shapes = FieldBlobLayoutGenerator.Generate(presentation.BlobLayout, presentation.ArenaSideLength.Value, Vector2.zero,
                    seed, "FIELD-DEV-BLOBS-ENVIRONMENT");
                Assert.AreEqual(20, shapes.Count, $"seed {seed}");
                layouts.Add(string.Join("|", shapes.Select(s => s.Center.ToString("F2"))));
            }
            Assert.Greater(layouts.Count, 95, "A fresh seed gives a fresh arrangement.");
        }

        [Test]
        public void TestFields_AreListedAfterEveryDesignedField_AndNamedAfterTheFieldTheyTest()
        {
            var catalog = Catalog;
            var rules = MetaCatalog.Load().Unlocks;
            var ids = catalog.Fields.Roster.AllFields.Select(f => f.Id.ToString()).ToList();
            var firstTest = ids.FindIndex(id => rules[id].Condition == "dev");
            Assert.Greater(firstTest, 0, "At least one test field exists and a designed field comes first.");
            Assert.IsTrue(ids.Skip(firstTest).All(id => rules[id].Condition == "dev"), "Test fields sit below all designed fields.");
            Assert.AreEqual("Тест 02", catalog.Fields.Roster.AllFields.Single(f => f.Id.ToString() == "FIELD-DEV-BLOBS").DisplayName);
        }

        [Test]
        public async Task DevField_IsAlwaysAvailable_OnAFreshProfile_WhileLaterFieldsStayLocked()
        {
            var profile = new ProfileService(MetaCatalog.Load(), new MemoryProfileStore());
            await profile.LoadAsync();
            var access = new ProfileAccessProvider(profile);
            Assert.AreEqual("dev", profile.Catalog.Unlocks["FIELD-DEV-BLOBS"].Condition);
            Assert.IsNull(access.GetLockReason(new ContentId("FIELD-DEV-BLOBS")));
            Assert.IsNull(access.GetLockReason(new ContentId("FIELD-001")));
            Assert.IsNotNull(access.GetLockReason(new ContentId("FIELD-002")));
        }
    }
}
