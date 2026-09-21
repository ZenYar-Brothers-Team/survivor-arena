using System.Linq;
using System.Reflection;
using Game.Combat;
using Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    public class EnemyPatternTests
    {
        [Test]
        public void MovementFamilies_ProduceDistinctConfiguredMotion()
        {
            var keepDistance = new EnemyMovementController(new EnemyMovementProfile(
                EnemyMovementKind.KeepDistance,
                preferredDistance: 5f,
                distanceTolerance: 0.5f));
            Assert.AreEqual(EnemyMovementPhase.Retreating,
                keepDistance.Tick(Vector2.zero, Vector2.right * 2f, 2f, 0.1f, true).Phase);
            Assert.AreEqual(EnemyMovementPhase.HoldingDistance,
                keepDistance.Tick(Vector2.zero, Vector2.right * 5f, 2f, 0.1f, true).Phase);
            Assert.AreEqual(EnemyMovementPhase.Approaching,
                keepDistance.Tick(Vector2.zero, Vector2.right * 8f, 2f, 0.1f, true).Phase);

            var orbit = new EnemyMovementController(new EnemyMovementProfile(
                EnemyMovementKind.Orbit,
                preferredDistance: 5f,
                distanceTolerance: 0.5f));
            var orbitFrame = orbit.Tick(Vector2.zero, Vector2.right * 5f, 2f, 0.1f, true);
            Assert.AreEqual(EnemyMovementPhase.Orbiting, orbitFrame.Phase);
            Assert.AreEqual(0f, orbitFrame.Velocity.x, 0.0001f);
            Assert.Greater(orbitFrame.Velocity.y, 0f);

            var zigzag = new EnemyMovementController(new EnemyMovementProfile(
                EnemyMovementKind.Zigzag,
                lateralStrength: 1f,
                cycleSeconds: 1f));
            var zigzagFrame = zigzag.Tick(Vector2.zero, Vector2.right * 5f, 2f, 0.125f, true);
            Assert.AreEqual(EnemyMovementPhase.Zigzagging, zigzagFrame.Phase);
            Assert.Greater(zigzagFrame.Velocity.y, 0f);

            var approachRetreat = new EnemyMovementController(new EnemyMovementProfile(
                EnemyMovementKind.ApproachRetreat,
                cycleSeconds: 2f));
            Assert.AreEqual(EnemyMovementPhase.Approaching,
                approachRetreat.Tick(Vector2.zero, Vector2.right, 1f, 0.25f, true).Phase);
            Assert.AreEqual(EnemyMovementPhase.Retreating,
                approachRetreat.Tick(Vector2.zero, Vector2.right, 1f, 1f, true).Phase);
        }

        [Test]
        public void Dash_TelegraphsLocksDirectionAndPauseDoesNotAdvance()
        {
            var controller = new EnemyMovementController(new EnemyMovementProfile(
                EnemyMovementKind.TelegraphedDash,
                dashTelegraphSeconds: 0.5f,
                dashDurationSeconds: 0.4f,
                dashCooldownSeconds: 1f,
                dashSpeedMultiplier: 3f));

            var telegraph = controller.Tick(Vector2.zero, Vector2.right, 2f, 1f, true);
            Assert.IsTrue(telegraph.IsTelegraphing);
            Assert.AreEqual(Vector2.right, telegraph.TelegraphDirection);

            var paused = controller.Tick(Vector2.zero, Vector2.up, 2f, 10f, false);
            Assert.IsTrue(paused.IsTelegraphing);
            Assert.AreEqual(Vector2.zero, paused.Velocity);

            var dash = controller.Tick(Vector2.zero, Vector2.up, 2f, 0.5f, true);
            Assert.AreEqual(EnemyMovementPhase.Dashing, dash.Phase);
            Assert.AreEqual(Vector2.right, dash.Velocity.normalized);
            Assert.AreEqual(6f, dash.Velocity.magnitude, 0.0001f);
        }

        [Test]
        public void ProjectilePatterns_CreateExpectedGeometry()
        {
            var fan = Profile(EnemyProjectilePattern.Fan, count: 3, spread: 40f);
            var fanShots = EnemyProjectilePatternGenerator.Create(fan, Vector2.right);
            Assert.AreEqual(3, fanShots.Length);
            Assert.Less(fanShots[0].Direction.y, 0f);
            Assert.AreEqual(0f, fanShots[1].Direction.y, 0.0001f);
            Assert.Greater(fanShots[2].Direction.y, 0f);

            var ringShots = EnemyProjectilePatternGenerator.Create(Profile(EnemyProjectilePattern.Ring, 8), Vector2.right);
            Assert.AreEqual(8, ringShots.Length);
            Assert.AreEqual(0f, Vector2.Dot(ringShots[0].Direction, ringShots[2].Direction), 0.0001f);

            var crossShots = EnemyProjectilePatternGenerator.Create(Profile(EnemyProjectilePattern.Cross, 4), Vector2.up);
            Assert.AreEqual(4, crossShots.Length);
            Assert.Less(Vector2.Distance(Vector2.up, crossShots[0].Direction), 0.0001f);
            Assert.Less(Vector2.Distance(Vector2.down, crossShots[2].Direction), 0.0001f);
        }

        [Test]
        public void BurstAndSpiral_ArePauseSafeAndStateful()
        {
            var burst = new EnemyAttackController(Profile(EnemyProjectilePattern.Burst, count: 3));
            Assert.AreEqual(1, burst.Tick(0f, true, Vector2.right).Length);
            Assert.AreEqual(0, burst.Tick(5f, false, Vector2.right).Length);
            Assert.AreEqual(1, burst.Tick(0.15f, true, Vector2.right).Length);
            Assert.AreEqual(1, burst.Tick(0.15f, true, Vector2.right).Length);

            var spiralProfile = new EnemyAttackProfile(
                EnemyProjectilePattern.Spiral, 1f, 1f, 1f, 1f, 4, rotationStepDegrees: 45f);
            var spiral = new EnemyAttackController(spiralProfile);
            var first = spiral.Tick(0f, true, Vector2.right);
            var second = spiral.Tick(1f, true, Vector2.right);
            Assert.AreEqual(4, first.Length);
            Assert.AreEqual(4, second.Length);
            Assert.AreNotEqual(first[0].Direction, second[0].Direction);
        }

        [Test]
        public void ProjectileDamage_CoversHitMissExplosionAndRunState()
        {
            using (var health = new Health(new FixedHealthProfile(20f)))
            {
                var single = Profile(EnemyProjectilePattern.Single);
                Assert.AreEqual(1f, EnemyProjectileDamage.Apply(
                    single, Vector2.zero, Vector2.zero, health, RunState.Running));
                Assert.AreEqual(0f, EnemyProjectileDamage.Apply(
                    single, Vector2.zero, Vector2.zero, health, RunState.Paused));

                var explosive = new EnemyAttackProfile(
                    EnemyProjectilePattern.Explosive, 4f, 1f, 1f, 1f, explosionRadius: 1f);
                Assert.AreEqual(0f, EnemyProjectileDamage.Apply(
                    explosive, Vector2.zero, Vector2.right * 2f, health, RunState.Running));
                Assert.AreEqual(4f, EnemyProjectileDamage.Apply(
                    explosive, Vector2.zero, Vector2.right * 0.5f, health, RunState.Running));
                Assert.AreEqual(15f, health.CurrentHealth);
            }
        }

        [Test]
        public void ProjectileLifetime_PausesAndExpiresOnRunningTimeOnly()
        {
            var lifetime = new EnemyProjectileLifetime(1f);
            Assert.IsFalse(lifetime.Tick(0.75f, true));
            Assert.IsFalse(lifetime.Tick(10f, false));
            Assert.IsTrue(lifetime.Tick(0.25f, true));
        }

        [Test]
        public void ProjectileRuntime_DespawnsWhenRunHasEnded()
        {
            var runObject = new GameObject("Run Controller");
            var runController = runObject.AddComponent<RunController>();
            EnemyProjectileRuntime projectile = null;
            try
            {
                typeof(RunController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(runController, null);
                runController.Model.Start();
                runController.Model.Tick(runController.Model.Duration);
                projectile = EnemyProjectileFactory.Spawn(
                    Profile(EnemyProjectilePattern.Single),
                    Vector2.zero,
                    Vector2.right,
                    null,
                    runController);

                Assert.IsTrue(projectile == null);
            }
            finally
            {
                if (projectile != null)
                    Object.DestroyImmediate(projectile.gameObject);
                Object.DestroyImmediate(runObject);
            }
        }

        [Test]
        public void FixtureCatalog_ContainsMeleeDashAndAllRequiredProjectilePatterns()
        {
            var definitions = FixtureEnemyCatalog.Create();
            var patterns = definitions.Where(definition => definition.Attack != null)
                .Select(definition => definition.Attack.Pattern)
                .ToArray();

            Assert.IsTrue(definitions.Any(definition => definition.Attack == null));
            Assert.IsTrue(definitions.Any(definition => definition.Movement.Kind == EnemyMovementKind.TelegraphedDash));
            CollectionAssert.IsSubsetOf(new[]
            {
                EnemyProjectilePattern.Fan,
                EnemyProjectilePattern.Burst,
                EnemyProjectilePattern.Ring,
                EnemyProjectilePattern.Cross,
                EnemyProjectilePattern.Spiral,
                EnemyProjectilePattern.Explosive
            }, patterns);
        }

        [Test]
        public void FixtureCatalog_ReadsFormerCodeDefaultsFromConfig()
        {
            var definitions = FixtureEnemyCatalog.Create().ToDictionary(definition => definition.Id.ToString());

            var fan = definitions["FIXTURE-ENEMY-FAN"];
            Assert.AreEqual(0.12f, fan.Attack.ProjectileRadius);
            Assert.AreEqual(4.5f, fan.Movement.PreferredDistance);
            var burst = definitions["FIXTURE-ENEMY-BURST-ORBIT"].Attack;
            Assert.AreEqual(0.15f, burst.BurstIntervalSeconds);
            Assert.AreEqual(0.12f, burst.ProjectileRadius);
            var dash = definitions["FIXTURE-ENEMY-DASH-EXPLOSIVE"].Movement;
            Assert.AreEqual(0.7f, dash.DashTelegraphSeconds);
            Assert.AreEqual(4f, dash.DashSpeedMultiplier);
            Assert.AreEqual(0.22f, definitions["FIXTURE-ENEMY-DASH-EXPLOSIVE"].Attack.ProjectileRadius);
        }

        private static EnemyAttackProfile Profile(
            EnemyProjectilePattern pattern,
            int count = 1,
            float spread = 0f) =>
            new EnemyAttackProfile(pattern, 1f, 1f, 2f, 2f, count, spread);
    }
}
