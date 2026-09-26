using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    /// <summary>IP-20 remainder: ENEMY-006 and ENEMY-008…020 from the approved enemies-v1 data (DECISION-0062).</summary>
    public sealed class ProductionLateEnemyCatalogTests
    {
        private static EnemyDefinition Enemy(string id) => ProductionEnemyCatalog.Create().Single(e => e.Id.ToString() == id);

        [Test]
        public void CardNumbers_AreKept_AndOnlySpeedFollowsTheFieldOneRebalance()
        {
            // (id, HP, approved speed, contact, XP, resistance): cards unchanged except speed = card ×1.3 rounded to 0.05 (enemies-v1).
            var rows = new (string id, float hp, float speed, float contact, float xp, float resistance)[]
            {
                ("ENEMY-006", 85, 0.85f, 12, 4, 0.1f), ("ENEMY-008", 90, 1.3f, 12, 4, 0.2f), ("ENEMY-009", 220, 0.7f, 22, 6, 0.55f),
                ("ENEMY-010", 100, 0.9f, 12, 5, 0.1f), ("ENEMY-011", 120, 0.85f, 14, 6, 0.15f), ("ENEMY-012", 115, 1f, 15, 6, 0.15f),
                ("ENEMY-013", 110, 1.55f, 16, 6, 0.1f), ("ENEMY-014", 180, 0.65f, 20, 8, 0.25f), ("ENEMY-015", 145, 0.9f, 16, 7, 0.2f),
                ("ENEMY-016", 320, 0.8f, 28, 10, 0.65f), ("ENEMY-017", 170, 1.75f, 20, 8, 0.2f), ("ENEMY-018", 190, 0.85f, 18, 9, 0.25f),
                ("ENEMY-019", 300, 0.9f, 26, 12, 0.6f), ("ENEMY-020", 420, 1.1f, 36, 15, 0.7f),
            };
            foreach (var row in rows)
            {
                var enemy = Enemy(row.id);
                Assert.AreEqual(row.hp, enemy.MaxHealth, 1e-4f, row.id);
                Assert.AreEqual(row.contact, enemy.ContactDamage, 1e-4f, row.id);
                Assert.AreEqual(row.xp, enemy.ExperienceReward, 1e-4f, row.id);
                Assert.AreEqual(row.resistance, enemy.KnockbackResistance, 1e-4f, row.id);
                Assert.AreEqual(1f, enemy.ContactDamageInterval, 1e-4f, row.id);
                Assert.AreEqual(row.speed, enemy.MovementSpeed, 1e-4f, row.id);
                Assert.Less(enemy.MovementSpeed, 3f, $"{row.id} stays slower than the player.");
            }
        }

        [Test]
        public void MovementFamilies_FollowTheCards()
        {
            Assert.AreEqual(EnemyMovementKind.Orbit, Enemy("ENEMY-008").Movement.Kind);
            Assert.AreEqual(1.2f, Enemy("ENEMY-008").Movement.PreferredDistance, 1e-5f, "Arcs in close enough to touch.");
            Assert.AreEqual(EnemyMovementKind.ApproachRetreat, Enemy("ENEMY-013").Movement.Kind);
            Assert.AreEqual(2.6f, Enemy("ENEMY-013").Movement.CycleSeconds, 1e-5f);
            Assert.AreEqual(EnemyMovementKind.Zigzag, Enemy("ENEMY-017").Movement.Kind);
            Assert.AreEqual(0.9f, Enemy("ENEMY-017").Movement.CycleSeconds, 1e-5f);
            var knight = Enemy("ENEMY-016");
            Assert.AreEqual(EnemyMovementKind.TelegraphedDash, knight.Movement.Kind);
            Assert.AreEqual(0.8f, knight.Movement.DashTelegraphSeconds, 1e-5f);
            Assert.AreEqual(3.5f, knight.Movement.DashSpeedMultiplier, 1e-5f);
            Assert.AreEqual(1.1f, knight.DashContactControls.KnockbackDistance, 1e-5f);
            Assert.IsFalse(knight.Movement.ShowDashTelegraphLine, "Same as the hound after DECISION-0057.");
            foreach (var id in new[] { "ENEMY-009", "ENEMY-019", "ENEMY-020" })
                Assert.AreEqual(EnemyMovementKind.Seek, Enemy(id).Movement.Kind, id);
        }

        [Test]
        public void RangedAttacks_KeepCardNumbers_AndReachFromTheirHeldDistance()
        {
            var rows = new (string id, EnemyProjectilePattern pattern, float damage, float cooldown, int count)[]
            {
                ("ENEMY-006", EnemyProjectilePattern.Fan, 10, 3f, 3), ("ENEMY-010", EnemyProjectilePattern.Burst, 9, 3.2f, 3),
                ("ENEMY-011", EnemyProjectilePattern.Ring, 9, 3.8f, 8), ("ENEMY-012", EnemyProjectilePattern.Single, 18, 3f, 1),
                ("ENEMY-014", EnemyProjectilePattern.Explosive, 24, 4f, 1), ("ENEMY-015", EnemyProjectilePattern.Cross, 13, 3.3f, 4),
                ("ENEMY-018", EnemyProjectilePattern.Spiral, 12, 3.4f, 10), ("ENEMY-019", EnemyProjectilePattern.Fan, 15, 3.5f, 5),
            };
            foreach (var row in rows)
            {
                var enemy = Enemy(row.id);
                Assert.AreEqual(row.pattern, enemy.Attack.Pattern, row.id);
                Assert.AreEqual(row.damage, enemy.Attack.Damage, 1e-4f, row.id);
                Assert.AreEqual(row.cooldown, enemy.Attack.CooldownSeconds, 1e-4f, row.id);
                Assert.AreEqual(row.count, enemy.Attack.ProjectileCount, row.id);
                Assert.AreEqual(EnemyAttackCadence.WindupStartToStart, enemy.Attack.Cadence, row.id);
                Assert.GreaterOrEqual(enemy.Attack.TelegraphSeconds, 0.5f, $"{row.id}: readable wind-up");
                if (enemy.Movement.Kind == EnemyMovementKind.KeepDistance)
                    Assert.GreaterOrEqual(enemy.Attack.ProjectileSpeed * enemy.Attack.ProjectileLifetimeSeconds,
                        enemy.Movement.PreferredDistance + enemy.Movement.DistanceTolerance + 1f, $"{row.id}: shots reach the player");
            }
            Assert.AreEqual(35f, Enemy("ENEMY-006").Attack.SpreadDegrees, 1e-4f);
            Assert.AreEqual(0.15f, Enemy("ENEMY-010").Attack.BurstIntervalSeconds, 1e-5f);
            Assert.AreEqual(1f, Enemy("ENEMY-014").Attack.ExplosionRadius, 1e-5f);
            Assert.AreEqual(18f, Enemy("ENEMY-018").Attack.RotationStepDegrees, 1e-5f);
            foreach (var id in new[] { "ENEMY-008", "ENEMY-009", "ENEMY-013", "ENEMY-016", "ENEMY-017", "ENEMY-020" })
                Assert.IsNull(Enemy(id).Attack, $"{id} is melee on its card.");
        }

        [Test]
        public void CrossAimsOneArmAtThePlayer_AndSpiralTurnsHalfAStepPerVolley()
        {
            var cross = NextVolley(new EnemyAttackController(Enemy("ENEMY-015").Attack), Vector2.up);
            Assert.AreEqual(4, cross.Length);
            Assert.IsTrue(cross.Any(s => Vector2.Angle(s.Direction, Vector2.up) < 1e-2f), "One cross arm points at the target.");

            var spiral = new EnemyAttackController(Enemy("ENEMY-018").Attack);
            var first = NextVolley(spiral, Vector2.right);
            var second = NextVolley(spiral, Vector2.right);
            Assert.AreEqual(10, first.Length);
            Assert.AreEqual(10, second.Length);
            Assert.AreEqual(18f, Mathf.Abs(Vector2.SignedAngle(first[0].Direction, second[0].Direction)), 1e-2f,
                "Next volley is rotated by half of the 36° step.");
        }

        [Test]
        public void EveryProductionEnemy_MovesWithinItsSpeed_AndRangedOnesFireWithinTenSeconds()
        {
            foreach (var enemy in ProductionEnemyCatalog.Create())
            {
                var movement = new EnemyMovementController(enemy.Movement);
                var attack = enemy.Attack != null ? new EnemyAttackController(enemy.Attack, random: new System.Random(7)) : null;
                var self = Vector2.zero;
                var target = new Vector2(8f, 0f);
                var limit = enemy.MovementSpeed * (enemy.Movement.Kind == EnemyMovementKind.TelegraphedDash
                    ? enemy.Movement.DashSpeedMultiplier : 1f) + 1e-3f;
                var shots = 0;
                for (var step = 0; step < 500; step++)
                {
                    var result = movement.Tick(self, target, enemy.MovementSpeed, 0.02f, true);
                    Assert.LessOrEqual(result.Velocity.magnitude, limit, $"{enemy.Id} at step {step}");
                    self += result.Velocity * 0.02f;
                    if (attack != null) shots += attack.Tick(0.02f, true, (target - self).normalized).Length;
                }
                if (attack != null) Assert.Greater(shots, 0, $"{enemy.Id} never fired in 10 s.");
            }
        }

        private static EnemyShotCommand[] NextVolley(EnemyAttackController controller, Vector2 aim)
        {
            for (var step = 0; step < 2000; step++)
            {
                var shots = controller.Tick(0.01f, true, aim);
                if (shots.Length > 0) return shots;
            }
            Assert.Fail("No volley within 20 s.");
            return null;
        }

        [Test]
        public void LateEnemies_UseApprovedBodies_AndUnapprovedProjectilesRemainUnbound()
        {
            // Approved body art is bound; separate projectile art still has a per-ID gate (IP-20).
            foreach (var enemy in ProductionEnemyCatalog.Create().Where(e => int.Parse(e.Id.ToString().Substring(6)) >= 10))
            {
                Assert.AreEqual(enemy.Id.ToString() + "-VISUAL-BODY", enemy.Visual.Id.ToString());
                Assert.IsTrue(enemy.MotionProfile.Id.IsValid, enemy.Id.ToString());
                if (enemy.Attack != null) Assert.IsFalse(enemy.Attack.ProjectileVisual.Id.IsValid, enemy.Id.ToString());
            }
            var crossbow = ProductionEnemyCatalog.Create().Single(e => e.Id.ToString() == "ENEMY-006");
            Assert.AreEqual("ENEMY-005-VISUAL-PROJECTILE", crossbow.Attack.ProjectileVisual.Id.ToString());
            foreach (var id in new[] { "ENEMY-006", "ENEMY-008", "ENEMY-009" })
            {
                var enemy = ProductionEnemyCatalog.Create().Single(e => e.Id.ToString() == id);
                Assert.AreEqual(id + "-VISUAL-BODY", enemy.Visual.Id.ToString());
                Assert.IsTrue(enemy.MotionProfile.Id.IsValid, id);
            }
        }
    }
}
