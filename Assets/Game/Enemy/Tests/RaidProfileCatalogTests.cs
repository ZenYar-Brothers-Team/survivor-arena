using System;
using Game.Content.Json;
using NUnit.Framework;

namespace Game.Enemy.Tests
{
    public sealed class RaidProfileCatalogTests
    {
        [Test]
        public void Load_ProductionProfile_MatchesTheAgreedRoundupRules()
        {
            var profile = RaidProfileCatalog.Load();

            Assert.AreEqual("RAID-001", profile.Id);
            Assert.AreEqual(1f, profile.TriggerRadiusScreenWidths);
            Assert.AreEqual(40, profile.TriggerEnemyCount);
            Assert.AreEqual(60f, profile.MinIntervalSeconds);
            Assert.AreEqual(120f, profile.MaxIntervalSeconds);
            Assert.AreEqual(2f, profile.InnerExemptRadius);
            Assert.AreEqual(15f, profile.Ring.DurationSeconds);
            Assert.AreEqual(20f, profile.Wall.DurationSeconds);
            Assert.AreEqual(4f, profile.Wall.SampleSeconds);
            Assert.AreEqual(.25f, profile.Wall.DistanceScreenWidths);
            Assert.AreEqual(.18f, profile.Contraction.EndRadiusScreenWidths);
            Assert.AreEqual(1f, profile.RetargetSeconds);
            Assert.AreEqual(.6f, profile.FormationSpeedBonus);
            Assert.AreEqual(.15f, profile.ArrivalSlowDistance);
            CollectionAssert.AreEquivalent(
                new[] { RaidTemplateKind.Ring, RaidTemplateKind.Wall, RaidTemplateKind.Contraction, RaidTemplateKind.Pincer },
                System.Enum.GetValues(typeof(RaidTemplateKind)));
        }

        [Test]
        public void FromJson_MissingRingField_FailsByName()
        {
            var json = JsonContentFile.ReadText(RaidProfileCatalog.ResourcePath).Replace("\"slotSpacing\": 0.9,", "");

            var error = Assert.Throws<InvalidOperationException>(() => RaidProfileCatalog.FromJson(json));

            StringAssert.Contains("slotSpacing", error.Message);
        }

        [Test]
        public void Constructor_MaxIntervalShorterThanMinimum_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestProfile.Create(minInterval: 90f, maxInterval: 60f));
        }
    }
}
