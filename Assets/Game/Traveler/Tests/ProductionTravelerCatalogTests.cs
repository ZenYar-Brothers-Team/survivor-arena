using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using NUnit.Framework;

namespace Game.Traveler.Tests
{
    /// <summary>F1-07: TRAVELER-001/002/005 and the FIELD-001 Traveler schedule (baseline v1).</summary>
    public sealed class ProductionTravelerCatalogTests
    {
        private static TravelerDefinition Traveler(string id) =>
            FixtureTravelerCatalog.CreateProduction().Definitions[new ContentId(id)];

        [Test]
        public void ThreeRoles_UseApprovedProfilesPresenceAndXp()
        {
            var catalog = FixtureTravelerCatalog.CreateProduction();
            CollectionAssert.AreEquivalent(new[] { "TRAVELER-001", "TRAVELER-002", "TRAVELER-005" },
                catalog.Definitions.Keys.Select(k => k.ToString()));
            var bruiser = Traveler("TRAVELER-001");
            Assert.AreEqual(TravelerRole.Offensive, bruiser.Role);
            Assert.AreEqual(650f, bruiser.Body.MaxHealth);
            Assert.AreEqual(20f, bruiser.Body.ContactDamage);
            Assert.AreEqual(12f, bruiser.Body.ExperienceReward);
            Assert.AreEqual(90f, bruiser.PresenceSeconds);
            var wanderer = Traveler("TRAVELER-002");
            Assert.AreEqual(TravelerRole.Wanderer, wanderer.Role);
            Assert.AreEqual(0f, wanderer.Body.ContactDamage);
            Assert.IsNull(wanderer.Body.Attack);
            Assert.AreEqual(75f, wanderer.PresenceSeconds);
            Assert.AreEqual(3f, wanderer.WanderSeconds);
            Assert.AreEqual(0.5f, wanderer.RestSeconds);
            Assert.AreEqual(3f, wanderer.AvoidRadius);
            Assert.AreEqual(1.5f, wanderer.AvoidSeconds);
            var guard = Traveler("TRAVELER-005");
            Assert.AreEqual(TravelerRole.Protector, guard.Role);
            Assert.AreEqual(TravelerSupportKind.Aura, guard.Support);
            Assert.AreEqual(3f, guard.SupportRadius);
            Assert.AreEqual(0.2f, guard.Reduction, 1e-5f);
            Assert.AreEqual(0.65f, guard.Body.KnockbackResistance, 1e-5f);
            Assert.AreEqual(18f, guard.Body.ExperienceReward);
        }

        [Test]
        public void Schedule_ScalesOnlyHpAndDamage_ByFieldRankAndTime()
        {
            var schedule = FixtureTravelerCatalog.CreateProduction().Schedules.Single();
            Assert.AreEqual("FIELD-001-TRAVELERS", schedule.Id.ToString());
            CollectionAssert.AreEqual(new[] { 0.15f, 0.4f, 0.35f, 0.1f }, schedule.CountProbabilities);
            Assert.AreEqual(1f, schedule.Scale(0f, 900f), 1e-5f);
            Assert.AreEqual(1.25f, schedule.Scale(390f, 900f), 1e-5f);
            Assert.AreEqual(1.5f, schedule.Scale(900f, 900f), 1e-5f);
            var scaled = Traveler("TRAVELER-001").Scale(1.25f);
            Assert.AreEqual(812.5f, scaled.MaxHealth, 1e-3f);
            Assert.AreEqual(25f, scaled.ContactDamage, 1e-3f);
            Assert.AreEqual(0.75f, scaled.MovementSpeed, 1e-5f, "Speed is never scaled.");
        }

        [Test]
        public void Draws_ProduceZeroToThreeDistinctTypes_WithinTheSpawnWindow()
        {
            var schedule = FixtureTravelerCatalog.CreateProduction().Schedules.Single();
            var counts = new int[4];
            for (var seed = 0; seed < 400; seed++)
            {
                var entries = schedule.Draw(900f, new Random(seed));
                Assert.LessOrEqual(entries.Count, 3);
                counts[entries.Count]++;
                Assert.AreEqual(entries.Count, entries.Select(e => e.Id).Distinct().Count(), "No repeated Traveler type.");
                foreach (var entry in entries) Assert.That(entry.Time, Is.InRange(0f, 780f));
            }
            for (var n = 0; n < 4; n++) Assert.Greater(counts[n], 0, $"count {n} occurs");
        }
    }
}
