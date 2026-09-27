using System;
using Game.Enemy.Json;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    /// <summary>DECISION-0066 E4: several dash-end actions after the last dash of a series, orientation, delay, replacement.</summary>
    public sealed class EnemyDashEndAttacksTests
    {
        private static EnemyAttackProfile Fan(int count) => new EnemyAttackProfile(EnemyProjectilePattern.Fan, 21f, 1f, 4.5f, 3f, count, 60f);
        private static EnemyAttackProfile Cross() => new EnemyAttackProfile(EnemyProjectilePattern.Cross, 28f, 1f, 5f, 3f, 4);

        [Test]
        public void Series_FiresOnlyAfterTheLastDash_NotBetweenDashes()
        {
            var controller = new EnemyDashVolleyController(new EnemyDashVolleyProfile(new[]
            {
                new EnemyDashVolleyEntry(0f, EnemyDashVolleyOrientation.TowardPlayer, Fan(6))
            }));
            Assert.AreEqual(0, controller.Tick(EnemyMovementPhase.Dashing, 1f, .1f, true, Vector2.right, Vector2.right).Length);
            Assert.AreEqual(0, controller.Tick(EnemyMovementPhase.TelegraphingDash, 1f, .1f, true, Vector2.right).Length,
                "The follow-up telegraph of a series is not its end.");
            Assert.AreEqual(0, controller.Tick(EnemyMovementPhase.Dashing, 1f, .1f, true, Vector2.right, Vector2.up).Length);
            var shots = controller.Tick(EnemyMovementPhase.Seeking, 1f, .1f, true, Vector2.right);
            Assert.AreEqual(6, shots.Length);
            Assert.AreEqual(1, controller.DashCount, "One series.");
            Assert.IsTrue(Array.TrueForAll(shots, s => s.Attack != null && s.Attack.ProjectileCount == 6), "Shots carry their own profile.");
        }

        [Test]
        public void AwayFromDash_AimsBackAlongTheLastDash_AndSelfZoneIsReported()
        {
            var zone = BossHazardTestData.Zone(BossZonePlacement.AroundSelf, radius: 2f, fill: .5f);
            var controller = new EnemyDashVolleyController(new EnemyDashVolleyProfile(new[]
            {
                new EnemyDashVolleyEntry(0f, EnemyDashVolleyOrientation.AwayFromDash, Fan(5)),
                new EnemyDashVolleyEntry(0f, EnemyDashVolleyOrientation.Self, zone: zone)
            }));
            controller.Tick(EnemyMovementPhase.Dashing, 1f, .1f, true, Vector2.left, Vector2.up);
            var shots = controller.Tick(EnemyMovementPhase.Seeking, 1f, .1f, true, Vector2.left);
            Assert.AreEqual(5, shots.Length);
            Assert.AreEqual(0f, Vector2.Angle(shots[2].Direction, Vector2.down), 1e-3f, "Middle of the rear fan points back along the dash.");
            Assert.AreEqual(1, controller.TriggeredZones.Count);
            Assert.AreSame(zone, controller.TriggeredZones[0]);
            controller.Tick(EnemyMovementPhase.Seeking, 1f, .1f, true, Vector2.left);
            Assert.AreEqual(0, controller.TriggeredZones.Count, "Zones are reported for one tick only.");
        }

        [Test]
        public void DelayedEntry_WaitsRunningTimeOnly_AndReplacementAppliesStrictlyBelowThreshold()
        {
            var controller = new EnemyDashVolleyController(new EnemyDashVolleyProfile(
                new[]
                {
                    new EnemyDashVolleyEntry(0f, EnemyDashVolleyOrientation.TowardPlayer, Fan(3)),
                    new EnemyDashVolleyEntry(0.4f, EnemyDashVolleyOrientation.TowardPlayer, Cross())
                }, 0.45f, new[] { new EnemyDashVolleyEntry(0f, EnemyDashVolleyOrientation.TowardPlayer, Fan(7)) }));
            EnemyShotCommand[] EndDash(float health)
            {
                controller.Tick(EnemyMovementPhase.Dashing, health, .1f, true, Vector2.up, Vector2.up);
                return controller.Tick(EnemyMovementPhase.Seeking, health, .1f, true, Vector2.up);
            }

            Assert.AreEqual(3, EndDash(1f).Length);
            Assert.IsTrue(controller.RepeatPending);
            Assert.AreEqual(0, controller.Tick(EnemyMovementPhase.Seeking, 1f, 5f, false, Vector2.up).Length, "Pause freezes the delay.");
            Assert.AreEqual(0, controller.Tick(EnemyMovementPhase.Seeking, 1f, .3f, true, Vector2.up).Length);
            var cross = controller.Tick(EnemyMovementPhase.Seeking, 1f, .11f, true, Vector2.left);
            Assert.AreEqual(4, cross.Length);
            Assert.AreEqual(0f, Vector2.Angle(cross[0].Direction, Vector2.up), 1e-3f, "Aimed when the series ended.");
            Assert.AreEqual(3, EndDash(0.45f).Length, "At the threshold the normal entries still fire.");
            controller.Tick(EnemyMovementPhase.Seeking, 1f, 1f, true, Vector2.up);
            Assert.AreEqual(7, EndDash(0.44f).Length, "Strictly below: the replacement fires instead.");
            Assert.IsFalse(controller.RepeatPending, "The replacement has no delayed cross.");
        }

        [Test]
        public void Entry_RejectsZoneNotAroundSelf_VolleyWithoutDirection_AndBothOrNeither()
        {
            Assert.Throws<ArgumentException>(() => new EnemyDashVolleyEntry(0f, EnemyDashVolleyOrientation.Self,
                zone: BossHazardTestData.Zone(BossZonePlacement.AtPlayer)));
            Assert.Throws<ArgumentException>(() => new EnemyDashVolleyEntry(0f, EnemyDashVolleyOrientation.Self, Fan(3)));
            Assert.Throws<ArgumentException>(() => new EnemyDashVolleyEntry(0f, EnemyDashVolleyOrientation.TowardPlayer));
            Assert.Throws<ArgumentException>(() => new EnemyDashVolleyProfile(new[]
                { new EnemyDashVolleyEntry(0f, EnemyDashVolleyOrientation.TowardPlayer, Fan(3)) }, 0.5f));
        }

        [Test]
        public void Catalog_ReadsDashEndAttacksAndReplacement_AndRejectsMixingWithTheSingleRing()
        {
            const string body = "{\"id\":\"FIXTURE-DASHER\",\"maxHealth\":100,\"collisionSize\":1,\"movementSpeed\":1,\"contactDamage\":1," +
                "\"contactDamageInterval\":1,\"knockbackResistance\":0,\"contactControls\":{\"knockbackDistance\":0}," +
                "\"dashContactControls\":{\"knockbackDistance\":1,\"knockbackSeconds\":0.12}," +
                "\"movement\":{\"kind\":\"TelegraphedDash\",\"dashTelegraphSeconds\":0.8,\"dashDurationSeconds\":0.8," +
                "\"dashCooldownSeconds\":5,\"dashSpeedMultiplier\":4.5}";
            const string fan = "{\"pattern\":\"Fan\",\"damage\":21,\"cooldownSeconds\":1,\"projectileSpeed\":4.5,\"projectileLifetimeSeconds\":3," +
                "\"projectileCount\":5,\"spreadDegrees\":60,\"projectileRadius\":0.14,\"telegraphSeconds\":0," +
                "\"controls\":{\"knockbackDistance\":0.35,\"knockbackSeconds\":0.12}}";
            const string zone = "{\"placement\":\"AroundSelf\",\"count\":1,\"radius\":2,\"fillSeconds\":0.5,\"damage\":30," +
                "\"controls\":{\"knockbackDistance\":0.3,\"knockbackSeconds\":0.12},\"lingerSeconds\":3,\"lingerDamagePerSecond\":11," +
                "\"lingerTickSeconds\":0.5,\"impactEffectSeconds\":0.45,\"telegraphColor\":[1,0.3,0.1,0.8],\"impactColor\":[1,0.5,0.1,0.9]}";
            var attacks = ",\"dashEndAttacks\":[{\"delaySeconds\":0,\"orientation\":\"AwayFromDash\",\"attack\":" + fan + "}," +
                          "{\"delaySeconds\":0,\"orientation\":\"Self\",\"zone\":" + zone + "}]";
            var replacement = ",\"dashEndReplacement\":{\"belowHealthFraction\":0.45,\"attacks\":[{\"delaySeconds\":0,\"orientation\":\"TowardPlayer\",\"attack\":" + fan + "}]}";

            var definition = FixtureEnemyCatalog.ToDefinition(JsonConvert.DeserializeObject<EnemyDefinitionData>(body + attacks + replacement + "}"));
            Assert.AreEqual(2, definition.DashVolley.Entries.Count);
            Assert.AreEqual(EnemyDashVolleyOrientation.AwayFromDash, definition.DashVolley.Entries[0].Orientation);
            Assert.AreEqual(11f, definition.DashVolley.Entries[1].Zone.LingerDamagePerSecond, 1e-5f);
            Assert.AreEqual(0.45f, definition.DashVolley.ReplacementBelowHealthFraction, 1e-5f);
            const string ring = ",\"dashEndAttack\":{\"pattern\":\"Ring\",\"damage\":16,\"cooldownSeconds\":1,\"projectileSpeed\":4," +
                "\"projectileLifetimeSeconds\":3,\"projectileCount\":8,\"projectileRadius\":0.15,\"telegraphSeconds\":0," +
                "\"controls\":{\"knockbackDistance\":0.25,\"knockbackSeconds\":0.12}}";
            Assert.Throws<InvalidOperationException>(() =>
                FixtureEnemyCatalog.ToDefinition(JsonConvert.DeserializeObject<EnemyDefinitionData>(body + attacks + ring + "}")));
            Assert.Throws<InvalidOperationException>(() =>
                FixtureEnemyCatalog.ToDefinition(JsonConvert.DeserializeObject<EnemyDefinitionData>(body + replacement + "}")));
            var scaled = WaveEnemyScaler.Apply(definition, new WaveEnemyModifiers(1f, 1f, 1f, 2f));
            Assert.AreEqual(42f, scaled.DashVolley.Entries[0].Attack.Damage, 1e-4f, "Field damage modifiers reach every volley.");
            Assert.AreSame(definition.DashVolley.Entries[1].Zone, scaled.DashVolley.Entries[1].Zone);
        }
    }
}
