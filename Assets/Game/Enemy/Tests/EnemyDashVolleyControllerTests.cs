using System;
using System.Linq;
using Game.Enemy.Json;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    /// <summary>F2-01 (DECISION-0063): ring on dash end, optional repeat every N-th dash below a health fraction.</summary>
    public sealed class EnemyDashVolleyControllerTests
    {
        private static EnemyAttackProfile Ring(int count) =>
            new EnemyAttackProfile(EnemyProjectilePattern.Ring, 16f, 1f, 4f, 3f, count, 360f);

        [Test]
        public void DashEnd_FiresOneEvenRing_StartingAtTheAim()
        {
            var controller = new EnemyDashVolleyController(new EnemyDashVolleyProfile(Ring(8)));
            Assert.AreEqual(0, controller.Tick(EnemyMovementPhase.TelegraphingDash, 1f, 0.1f, true, Vector2.up).Length);
            Assert.AreEqual(0, controller.Tick(EnemyMovementPhase.Dashing, 1f, 0.1f, true, Vector2.up).Length, "Nothing during the dash.");
            var ring = controller.Tick(EnemyMovementPhase.Seeking, 1f, 0.1f, true, Vector2.up);
            Assert.AreEqual(8, ring.Length);
            Assert.AreEqual(0f, Vector2.Angle(ring[0].Direction, Vector2.up), 1e-3f);
            Assert.AreEqual(45f, Vector2.Angle(ring[0].Direction, ring[1].Direction), 1e-3f);
            Assert.AreEqual(0, controller.Tick(EnemyMovementPhase.Seeking, 1f, 0.1f, true, Vector2.up).Length, "Once per dash.");
            Assert.AreEqual(1, controller.DashCount);
        }

        [Test]
        public void Repeat_EverySecondDashBelowHalfHealth_AfterTheDelay_RotatedAndFrozenByPause()
        {
            var controller = new EnemyDashVolleyController(new EnemyDashVolleyProfile(Ring(8), 2, 0.5f, 0.35f, 22.5f));
            EnemyShotCommand[] Dash(float health)
            {
                controller.Tick(EnemyMovementPhase.Dashing, health, 0.1f, true, Vector2.right);
                return controller.Tick(EnemyMovementPhase.Seeking, health, 0.1f, true, Vector2.right);
            }

            Dash(1f);
            Dash(1f);
            Assert.IsFalse(controller.RepeatPending, "Above the threshold the second dash does not repeat.");
            Dash(0.5f);
            Dash(0.5f);
            Assert.IsFalse(controller.RepeatPending, "Strictly below the threshold only.");
            Dash(0.49f); // fifth dash, odd
            Assert.IsFalse(controller.RepeatPending);
            var first = Dash(0.49f); // sixth dash, even
            Assert.AreEqual(8, first.Length);
            Assert.IsTrue(controller.RepeatPending);
            Assert.AreEqual(0, controller.Tick(EnemyMovementPhase.Seeking, 0.49f, 10f, false, Vector2.right).Length, "Pause freezes.");
            Assert.AreEqual(0, controller.Tick(EnemyMovementPhase.Seeking, 0.49f, 0.3f, true, Vector2.right).Length);
            var repeat = controller.Tick(EnemyMovementPhase.Seeking, 0.49f, 0.06f, true, Vector2.left);
            Assert.AreEqual(8, repeat.Length);
            Assert.AreEqual(22.5f, Vector2.SignedAngle(first[0].Direction, repeat[0].Direction), 1e-3f,
                "Rotated from the dash-end aim, not from the current aim.");
            Assert.IsFalse(controller.RepeatPending);
        }

        [Test]
        public void Profile_RejectsNonRingsAndRepeatsWithoutThreshold_AndNeedsDashMovement()
        {
            Assert.Throws<ArgumentException>(() => new EnemyDashVolleyProfile(
                new EnemyAttackProfile(EnemyProjectilePattern.Fan, 1f, 1f, 1f, 1f, 3, 30f)));
            Assert.Throws<ArgumentException>(() => new EnemyDashVolleyProfile(Ring(6), 2));
            Assert.Throws<ArgumentException>(() => new EnemyDefinition("FIXTURE-VOLLEY", 10f, 1f, 1f, 1f, 1f,
                movement: EnemyMovementProfile.Seek, dashVolley: new EnemyDashVolleyProfile(Ring(6))));
        }

        [Test]
        public void Catalog_ReadsDashEndAttackAndRepeat_AndRejectsARepeatWithoutRing()
        {
            const string body = "{\"id\":\"FIXTURE-VOLLEY\",\"maxHealth\":100,\"collisionSize\":1,\"movementSpeed\":1,\"contactDamage\":1," +
                "\"contactDamageInterval\":1,\"knockbackResistance\":0,\"contactControls\":{\"knockbackDistance\":0}," +
                "\"dashContactControls\":{\"knockbackDistance\":1,\"knockbackSeconds\":0.12}," +
                "\"movement\":{\"kind\":\"TelegraphedDash\",\"dashTelegraphSeconds\":0.8,\"dashDurationSeconds\":0.8," +
                "\"dashCooldownSeconds\":5,\"dashSpeedMultiplier\":4.5}";
            const string ring = ",\"dashEndAttack\":{\"pattern\":\"Ring\",\"damage\":16,\"cooldownSeconds\":1,\"projectileSpeed\":4," +
                "\"projectileLifetimeSeconds\":3,\"projectileCount\":8,\"projectileRadius\":0.15,\"telegraphSeconds\":0," +
                "\"controls\":{\"knockbackDistance\":0.25,\"knockbackSeconds\":0.12}}";
            const string repeat = ",\"dashEndRepeat\":{\"everyNthDash\":2,\"belowHealthFraction\":0.5,\"delaySeconds\":0.35,\"rotationDegrees\":22.5}";

            var definition = FixtureEnemyCatalog.ToDefinition(JsonConvert.DeserializeObject<EnemyDefinitionData>(body + ring + repeat + "}"));
            Assert.AreEqual(8, definition.DashVolley.Attack.ProjectileCount);
            Assert.AreEqual(2, definition.DashVolley.RepeatEveryNthDash);
            Assert.AreEqual(22.5f, definition.DashVolley.RepeatRotationDegrees, 1e-5f);
            Assert.IsNull(FixtureEnemyCatalog.ToDefinition(JsonConvert.DeserializeObject<EnemyDefinitionData>(body + "}")).DashVolley);
            Assert.Throws<InvalidOperationException>(() =>
                FixtureEnemyCatalog.ToDefinition(JsonConvert.DeserializeObject<EnemyDefinitionData>(body + repeat + "}")));
            var scaled = WaveEnemyScaler.Apply(definition, new WaveEnemyModifiers(1.12f, 1f, 1.08f, 1.08f));
            Assert.AreEqual(16f * 1.08f, scaled.DashVolley.Attack.Damage, 1e-4f, "Field damage modifiers reach the ring.");
        }
    }
}
