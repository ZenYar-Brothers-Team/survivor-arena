using System.Linq;
using Game.Content;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    /// <summary>F1-04: six production ordinary enemies and their approved baseline v1 behaviour.</summary>
    public sealed class ProductionEnemyCatalogTests
    {
        private static EnemyDefinition Enemy(string id) => ProductionEnemyCatalog.Create().Single(e => e.Id.ToString() == id);

        [Test]
        public void Catalog_ContainsExactlyTheSixStartupEnemies_WithRebalancedStats()
        {
            var enemies = ProductionEnemyCatalog.Create();
            CollectionAssert.AreEqual(new[] { "ENEMY-001", "ENEMY-002", "ENEMY-003", "ENEMY-004", "ENEMY-005", "ENEMY-007" },
                enemies.Select(e => e.Id.ToString()));
            var expected = new (string id, float hp, float speed, float contact, float xp)[]
            {
                ("ENEMY-001", 32, 1.2f, 10, 1), ("ENEMY-002", 24, 2.25f, 8, 1), ("ENEMY-003", 150, 0.8f, 20, 3),
                ("ENEMY-004", 48, 0.95f, 8, 2), ("ENEMY-005", 56, 1.1f, 9, 3), ("ENEMY-007", 64, 1.55f, 14, 4)
            };
            foreach (var row in expected)
            {
                var enemy = Enemy(row.id);
                Assert.AreEqual(row.hp, enemy.MaxHealth, 1e-4f, row.id);
                Assert.AreEqual(row.speed, enemy.MovementSpeed, 1e-4f, row.id);
                Assert.AreEqual(row.contact, enemy.ContactDamage, 1e-4f, row.id);
                Assert.AreEqual(row.xp, enemy.ExperienceReward, 1e-4f, row.id);
                Assert.AreEqual(1f, enemy.ContactDamageInterval, 1e-4f, row.id);
            }
            Assert.AreEqual(0.2f, Enemy("ENEMY-003").KnockbackResistance, 1e-4f);
            Assert.IsNull(Enemy("ENEMY-002").Attack, "Courier is fast melee, never the fixture letter shooter.");
        }

        [Test]
        public void RangedAndDashProfiles_MatchBaseline()
        {
            var sling = Enemy("ENEMY-004");
            Assert.AreEqual(EnemyMovementKind.KeepDistance, sling.Movement.Kind);
            Assert.AreEqual(5f, sling.Movement.PreferredDistance);
            Assert.AreEqual(EnemyAttackCadence.WindupStartToStart, sling.Attack.Cadence);
            Assert.AreEqual(10f, sling.Attack.Damage);
            Assert.AreEqual(2.4f, sling.Attack.CooldownSeconds, 1e-5f);
            Assert.AreEqual(0.55f, sling.Attack.TelegraphSeconds, 1e-5f);
            Assert.AreEqual(0.35f, sling.Attack.Controls.KnockbackDistance, 1e-5f);
            Assert.AreEqual("ENEMY-004-VISUAL-PROJECTILE", sling.Attack.ProjectileVisual.Id.ToString());

            var archer = Enemy("ENEMY-005");
            Assert.AreEqual(EnemyMovementKind.DistanceReposition, archer.Movement.Kind);
            Assert.AreEqual(6f, archer.Movement.PreferredDistance);
            Assert.AreEqual(3f, archer.Movement.CycleSeconds);
            Assert.AreEqual(1f, archer.Movement.RepositionSeconds);
            Assert.AreEqual(0.5f, archer.Movement.LateralStrength);
            Assert.AreEqual(EnemyProjectilePattern.Burst, archer.Attack.Pattern);
            Assert.AreEqual(3, archer.Attack.ProjectileCount);
            Assert.AreEqual(7f, archer.Attack.Damage);
            Assert.AreEqual(0.18f, archer.Attack.BurstIntervalSeconds, 1e-5f);
            Assert.AreEqual(12f, archer.Attack.SpreadDegrees, 1e-5f);
            Assert.AreEqual(6.5f, archer.Attack.ProjectileSpeed);

            var hound = Enemy("ENEMY-007");
            Assert.AreEqual(EnemyMovementKind.TelegraphedDash, hound.Movement.Kind);
            Assert.AreEqual(0.55f, hound.Movement.DashTelegraphSeconds, 1e-5f);
            Assert.AreEqual(0.55f, hound.Movement.DashDurationSeconds, 1e-5f);
            Assert.AreEqual(2.5f, hound.Movement.DashCooldownSeconds, 1e-5f);
            Assert.AreEqual(3.2f, hound.Movement.DashSpeedMultiplier, 1e-5f);
            Assert.AreEqual(0.65f, hound.DashContactControls.KnockbackDistance, 1e-5f);
            Assert.AreEqual(0.4f, hound.ContactControls.KnockbackDistance, 1e-5f);
        }

        [Test]
        public void Visuals_BindProductionBodySprites()
        {
            foreach (var id in new[] { "ENEMY-001", "ENEMY-002", "ENEMY-003", "ENEMY-004", "ENEMY-005", "ENEMY-007" })
                Assert.AreEqual($"{id}-VISUAL-BODY", Enemy(id).Visual.Id.ToString());
        }

        [Test]
        public void WindupCadence_WaitsOneCooldown_SnapshotsAim_AndCountsStartToStart()
        {
            var controller = new EnemyAttackController(Enemy("ENEMY-004").Attack);
            Assert.AreEqual(0, controller.Tick(2.3f, true, Vector2.right).Length);
            Assert.AreEqual(EnemyAttackPhase.Cooldown, controller.Phase);
            Assert.AreEqual(0, controller.Tick(0.11f, true, Vector2.right).Length); // t=2.41: wind-up started, aim right
            Assert.AreEqual(EnemyAttackPhase.Telegraphing, controller.Phase);
            Assert.AreEqual(0, controller.Tick(0.5f, true, Vector2.up).Length);
            var shots = controller.Tick(0.06f, true, Vector2.up); // t=2.96 > 2.95
            Assert.AreEqual(1, shots.Length);
            Assert.AreEqual(1f, shots[0].Direction.x, 1e-4f, "Aim is fixed at wind-up start.");
            Assert.AreEqual(0, controller.Tick(1.8f, true, Vector2.up).Length); // t=4.76
            Assert.AreEqual(0, controller.Tick(0.05f, true, Vector2.up).Length); // t=4.81 >= 2.4+2.4
            Assert.AreEqual(EnemyAttackPhase.Telegraphing, controller.Phase, "Next wind-up starts 2.4 s after the previous start.");
            Assert.AreEqual(0, controller.Tick(10f, false, Vector2.right).Length, "Pause freezes the wind-up.");
            Assert.AreEqual(EnemyAttackPhase.Telegraphing, controller.Phase);
        }

        [Test]
        public void ArcherVolley_FiresThreeSequentialArrows_EachWithinTheSpreadWindow()
        {
            var attack = Enemy("ENEMY-005").Attack;
            var controller = new EnemyAttackController(attack, random: new System.Random(5));
            var shots = new System.Collections.Generic.List<EnemyShotCommand>();
            var times = new System.Collections.Generic.List<float>();
            var time = 0f;
            for (var i = 0; i < 400 && shots.Count < 3; i++)
            {
                time += 0.01f;
                var fired = controller.Tick(0.01f, true, Vector2.right);
                foreach (var shot in fired) { shots.Add(shot); times.Add(time); }
            }
            Assert.AreEqual(3, shots.Count);
            Assert.AreEqual(times[0] + 0.18f, times[1], 0.011f, "Arrows follow one after another.");
            Assert.AreEqual(times[1] + 0.18f, times[2], 0.011f);
            var angles = new System.Collections.Generic.HashSet<float>();
            foreach (var shot in shots)
            {
                var angle = Mathf.Atan2(shot.Direction.y, shot.Direction.x) * Mathf.Rad2Deg;
                Assert.LessOrEqual(Mathf.Abs(angle), 6f + 1e-3f, "Deviation stays within ±spread/2.");
                angles.Add(Mathf.Round(angle * 1000f));
            }
            Assert.Greater(angles.Count, 1, "Arrows deviate randomly rather than repeat one angle.");
        }

        [Test]
        public void BurstSpread_RequiresARandomSource()
        {
            Assert.Throws<System.ArgumentNullException>(() => new EnemyAttackController(Enemy("ENEMY-005").Attack));
        }

        [Test]
        public void ArcherReposition_HoldsThenSidestepsAtSpeed_AlternatingSides()
        {
            var archer = Enemy("ENEMY-005");
            var controller = new EnemyMovementController(archer.Movement);
            var self = Vector2.zero;
            var target = new Vector2(6f, 0f); // exactly at preferred distance
            var hold = controller.Tick(self, target, archer.MovementSpeed, 1f, true);
            Assert.AreEqual(Vector2.zero, hold.Velocity);
            Assert.AreEqual(EnemyMovementPhase.HoldingDistance, hold.Phase);
            var first = controller.Tick(self, target, archer.MovementSpeed, 1.5f, true); // t=2.5 in reposition window
            Assert.AreEqual(EnemyMovementPhase.Repositioning, first.Phase);
            Assert.AreEqual(archer.MovementSpeed, first.Velocity.magnitude, 1e-4f);
            Assert.AreEqual(0f, first.Velocity.x, 1e-4f, "Within tolerance only the tangent moves.");
            controller.Tick(self, target, archer.MovementSpeed, 2.0f, true); // t=4.5 hold of next cycle
            var second = controller.Tick(self, target, archer.MovementSpeed, 1.0f, true); // t=5.5
            Assert.AreEqual(EnemyMovementPhase.Repositioning, second.Phase);
            Assert.AreEqual(-Mathf.Sign(first.Velocity.y), Mathf.Sign(second.Velocity.y), "Tangent side alternates each cycle.");
            var far = controller.Tick(self, new Vector2(20f, 0f), archer.MovementSpeed, 0.1f, true);
            Assert.LessOrEqual(far.Velocity.magnitude, archer.MovementSpeed + 1e-4f, "Axes never add up above speed.");
        }
    }
}
