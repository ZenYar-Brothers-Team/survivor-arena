using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using NUnit.Framework;

namespace Game.Traveler.Tests
{
    /// <summary>
    /// F1-07: TRAVELER-001/002/005 and the FIELD-001 Traveler schedule (baseline v1); TRAVELER-003/004/006…010 from
    /// travelers-v1 (DECISION-0088) on the same progression tier.
    /// </summary>
    public sealed class ProductionTravelerCatalogTests
    {
        private static TravelerDefinition Traveler(string id) =>
            FixtureTravelerCatalog.CreateProduction().Definitions[new ContentId(id)];

        [Test]
        public void ThreeRoles_UseApprovedProfilesPresenceAndXp()
        {
            var catalog = FixtureTravelerCatalog.CreateProduction();
            CollectionAssert.AreEquivalent(Enumerable.Range(1, 10).Select(i => $"TRAVELER-{i:000}"),
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
            Assert.AreEqual(14.2f, wanderer.AvoidRadius, 1e-4f, "DECISION-0121: 0.8 of the screen width.");
            Assert.AreEqual(1.5f, wanderer.AvoidSeconds);
            var guard = Traveler("TRAVELER-005");
            Assert.AreEqual(TravelerRole.Protector, guard.Role);
            Assert.AreEqual(TravelerSupportKind.Aura, guard.Support);
            Assert.AreEqual(3f, guard.SupportRadius);
            Assert.AreEqual(0.4f, guard.Reduction, 1e-5f, "DECISION-0120: aura reduction 20% -> 40%.");
            Assert.AreEqual(0.8f, guard.SupportVerticalScale, 1e-5f);
            Assert.AreEqual(0.65f, guard.Body.KnockbackResistance, 1e-5f);
            Assert.AreEqual(18f, guard.Body.ExperienceReward);
        }

        [TestCase("TRAVELER-003", TravelerRole.Wanderer, 620f, 0.9f, 0f, 10f, 75f)]
        [TestCase("TRAVELER-004", TravelerRole.Offensive, 500f, 1.15f, 16f, 12f, 90f)]
        [TestCase("TRAVELER-006", TravelerRole.Wanderer, 520f, 0.85f, 0f, 10f, 75f)]
        [TestCase("TRAVELER-007", TravelerRole.Protector, 950f, 0.8f, 0f, 18f, 90f)]
        [TestCase("TRAVELER-008", TravelerRole.Offensive, 800f, 0.65f, 24f, 12f, 90f)]
        [TestCase("TRAVELER-009", TravelerRole.Protector, 1050f, 0.75f, 0f, 18f, 90f)]
        [TestCase("TRAVELER-010", TravelerRole.Offensive, 600f, 0.85f, 18f, 12f, 90f)]
        public void LateTravelers_ShareTheFieldOneTierOfTheirRole(
            string id, TravelerRole role, float health, float speed, float contact, float xp, float presence)
        {
            var traveler = Traveler(id);
            Assert.AreEqual(role, traveler.Role);
            Assert.AreEqual(health, traveler.Body.MaxHealth, 1e-3f);
            Assert.AreEqual(speed, traveler.Body.MovementSpeed, 1e-5f);
            Assert.AreEqual(contact, traveler.Body.ContactDamage, 1e-5f);
            Assert.AreEqual(xp, traveler.Body.ExperienceReward);
            Assert.AreEqual(presence, traveler.PresenceSeconds);
            Assert.AreEqual($"{id}-VISUAL-BODY", traveler.Body.Visual.Id.ToString());
        }

        [Test]
        public void LateTravelers_UseCardBehaviourFamilies()
        {
            Assert.AreEqual(Game.Enemy.EnemyMovementKind.Zigzag, Traveler("TRAVELER-004").Body.Movement.Kind);
            var knight = Traveler("TRAVELER-008").Body;
            Assert.AreEqual(Game.Enemy.EnemyMovementKind.TelegraphedDash, knight.Movement.Kind);
            Assert.AreEqual(4f, knight.Movement.DashCooldownSeconds, 1e-5f, "Card: one long dash about every 4 s.");
            Assert.IsNotNull(knight.DashContactControls);
            // DECISION-0119: about a quarter of the screen width, shoving, wide telegraph.
            Assert.AreEqual(9f, knight.Movement.DashDistance, 1e-5f, "DECISION-0121: dash range doubled.");
            Assert.AreEqual(0.53f, knight.Movement.DashTelegraphWidth, 1e-5f, "DECISION-0121: band three times thinner.");
            Assert.AreEqual(4f, knight.Movement.DashTelegraphLength, 1e-5f, "DECISION-0121: drawn line shorter than the dash.");
            Assert.IsTrue(knight.Movement.DashShoves);
            Assert.AreEqual(1.5f, knight.Movement.DashShoveDistance, 1e-5f);
            var angel = Traveler("TRAVELER-010").Body.Attack;
            Assert.AreEqual(Game.Enemy.EnemyProjectilePattern.Cross, angel.Pattern);
            Assert.AreEqual(4, angel.ProjectileCount);
            Assert.AreEqual(10f, angel.Damage, 1e-5f);
            Assert.AreEqual(2.6f, angel.CooldownSeconds, 1e-5f);
            Assert.AreEqual("BOSS-010-VISUAL-PROJECTILE", angel.ProjectileVisual.Id.ToString());
            // DECISION-0120: Inquisitor throws a small haste splash, Heavenly Pilgrim heals in waves.
            var inquisitor = Traveler("TRAVELER-007");
            Assert.AreEqual(TravelerSupportKind.SpeedBurst, inquisitor.Support);
            Assert.AreEqual(4f, inquisitor.SupportRadius, 1e-5f);
            Assert.AreEqual(1.5f, inquisitor.EffectRadius, 1e-5f);
            Assert.AreEqual(0.5f, inquisitor.SpeedBonus, 1e-5f);
            Assert.AreEqual(5f, inquisitor.EffectSeconds, 1e-5f);
            Assert.AreEqual(5f, inquisitor.SupportCooldown, 1e-5f);
            Assert.AreEqual(0.8f, inquisitor.SupportVerticalScale, 1e-5f);
            var pilgrim = Traveler("TRAVELER-009");
            Assert.AreEqual(TravelerSupportKind.Heal, pilgrim.Support);
            Assert.AreEqual(3.5f, pilgrim.SupportRadius, 1e-5f);
            Assert.AreEqual(3f, pilgrim.SupportCooldown, 1e-5f);
            Assert.AreEqual(20f, pilgrim.HealAmount, 1e-5f);
            Assert.AreEqual(TravelerMovementStyle.ZigzagEscape, Traveler("TRAVELER-002").MovementStyle);
            Assert.AreEqual(TravelerMovementStyle.DashEscape, Traveler("TRAVELER-003").MovementStyle);
            var mage = Traveler("TRAVELER-006");
            Assert.AreEqual(TravelerMovementStyle.Orbit, mage.MovementStyle);
            Assert.Greater(mage.OrbitRadiusX, mage.OrbitRadiusY, "The oval follows the wide screen.");
            Assert.AreEqual(TravelerMovementStyle.Wander, Traveler("TRAVELER-001").MovementStyle);
            Assert.AreEqual(5f, Traveler("TRAVELER-003").WanderSeconds, "Long straight segments.");
            Assert.AreEqual(0f, Traveler("TRAVELER-003").RestSeconds);
            Assert.AreEqual(1f, Traveler("TRAVELER-006").RestSeconds, "Stops for a while between walks.");
        }

        [Test]
        public void Scale_KeepsTheWholeAttackProfile()
        {
            var angel = Traveler("TRAVELER-010");
            var scaled = angel.Scale(1.5f).Attack;
            Assert.AreEqual(15f, scaled.Damage, 1e-4f, "Projectile damage scales like contact damage.");
            Assert.AreEqual(angel.Body.Attack.Cadence, scaled.Cadence);
            Assert.AreEqual(angel.Body.Attack.TelegraphSeconds, scaled.TelegraphSeconds, 1e-5f);
            Assert.AreEqual(angel.Body.Attack.ProjectileVisual.Id, scaled.ProjectileVisual.Id);
            Assert.AreEqual(angel.Body.Attack.WindupMovementMultiplier, scaled.WindupMovementMultiplier, 1e-5f);
        }

        [Test]
        public void EveryFieldDrawsFromAllTenTravelers_WithDistinctRolesPerGroupOfThree()
        {
            var catalog = FixtureTravelerCatalog.CreateProduction();
            foreach (var schedule in catalog.Schedules.Cast<TravelerScheduleDefinition>())
            {
                Assert.AreEqual(10, schedule.TravelerIds.Count, schedule.Id.ToString());
                var seen = new HashSet<ContentId>();
                for (var seed = 0; seed < 300; seed++)
                {
                    var entries = schedule.Draw(900f, new Random(seed), id => catalog.Definitions[id].Role);
                    var roles = entries.OrderBy(e => e.Sequence).Select(e => catalog.Definitions[e.Id].Role).ToList();
                    for (var start = 0; start < roles.Count; start += 3)
                        Assert.AreEqual(roles.Skip(start).Take(3).Count(), roles.Skip(start).Take(3).Distinct().Count(), "DECISION-0122: every group of three has three different roles.");
                    foreach (var entry in entries) seen.Add(entry.Id);
                }
                Assert.AreEqual(10, seen.Count, $"{schedule.Id}: every Traveler can appear");
            }
        }

        [Test]
        public void Schedule_ScalesOnlyHpAndDamage_ByFieldRankAndTime()
        {
            var schedule = FixtureTravelerCatalog.CreateProduction().Schedules.Single(s => s.Id.ToString() == "FIELD-001-TRAVELERS");
            Assert.AreEqual("FIELD-001-TRAVELERS", schedule.Id.ToString());
            Assert.AreEqual(1, schedule.MinCount);
            CollectionAssert.AreEqual(new[] { 0.1f, 0.25f, 0.3f, 0.25f, 0.1f }, schedule.CountProbabilities);
            Assert.AreEqual(1f, schedule.Scale(0f, 900f), 1e-5f);
            Assert.AreEqual(1.25f, schedule.Scale(390f, 900f), 1e-5f);
            Assert.AreEqual(1.5f, schedule.Scale(900f, 900f), 1e-5f);
            Assert.AreEqual(1f / 3f, schedule.HealthScale(0f, 900f), 1e-5f);
            Assert.AreEqual(1.5f, schedule.HealthScale(780f, 900f), 1e-5f);
            var scaled = Traveler("TRAVELER-001").Scale(1.25f);
            Assert.AreEqual(812.5f, scaled.MaxHealth, 1e-3f);
            Assert.AreEqual(25f, scaled.ContactDamage, 1e-3f);
            Assert.AreEqual(0.75f, scaled.MovementSpeed, 1e-5f, "Speed is never scaled.");
            var body = Traveler("TRAVELER-001").Body;
            Assert.IsTrue(body.Visual.Id.IsValid && body.MotionProfile.Id.IsValid, "Production Traveler has approved body art.");
            Assert.AreEqual(body.Visual.Id, scaled.Visual.Id, "Scaling keeps the body art the runtime animates (DECISION-0059).");
            Assert.AreEqual(body.MotionProfile.Id, scaled.MotionProfile.Id);
        }

        [Test]
        public void Draws_ProduceOneToFiveDistinctTravelers_WithinTheSpawnWindow()
        {
            var schedule = FixtureTravelerCatalog.CreateProduction().Schedules.Single(s => s.Id.ToString() == "FIELD-001-TRAVELERS");
            var counts = new int[6];
            for (var seed = 0; seed < 400; seed++)
            {
                var entries = schedule.Draw(900f, new Random(seed));
                Assert.That(entries.Count, Is.InRange(1, 5), "DECISION-0122: 1 to 5 Travelers per map.");
                counts[entries.Count]++;
                Assert.AreEqual(entries.Count, entries.Select(e => e.Id).Distinct().Count(), "No repeated Traveler type.");
                foreach (var entry in entries) Assert.That(entry.Time, Is.InRange(0f, 780f));
            }
            for (var n = 1; n <= 5; n++) Assert.Greater(counts[n], 0, $"count {n} occurs");
        }
    }
}
