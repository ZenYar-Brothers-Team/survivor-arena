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
        public void OffsetPursuit_AlternatesPersonalTargetAndDirectAttackWithoutAdvancingOnPause()
        {
            var profile = new EnemyMovementProfile(EnemyMovementKind.OffsetPursuit,
                preferredDistance: 1.4f, distanceTolerance: 0.25f,
                cycleSeconds: 3f, directPursuitSeconds: 1.5f);
            var first = new EnemyMovementController(profile, new System.Random(17));
            var sameSeed = new EnemyMovementController(profile, new System.Random(17));
            var otherSeed = new EnemyMovementController(profile, new System.Random(18));
            var sawDifferentOffset = false;
            var sawOffset = false;
            var sawAttack = false;
            for (var i = 0; i < 90; i++)
            {
                var position = Vector2.right * 2f;
                var frame = first.Tick(position, Vector2.zero, 4f, 0.1f, true);
                var repeated = sameSeed.Tick(position, Vector2.zero, 4f, 0.1f, true);
                var different = otherSeed.Tick(position, Vector2.zero, 4f, 0.1f, true);
                Assert.AreEqual(frame.Phase, repeated.Phase);
                Assert.AreEqual(frame.Velocity, repeated.Velocity);
                if (frame.Phase == EnemyMovementPhase.OffsetPursuit)
                {
                    sawOffset = true;
                    sawDifferentOffset |= frame.Velocity != different.Velocity;
                }
                else if (frame.Phase == EnemyMovementPhase.Seeking)
                {
                    sawAttack = true;
                    Assert.AreEqual(Vector2.left * 4f, frame.Velocity,
                        "Direct pursuit must keep threatening the player after reaching an offset point.");
                }
                var paused = first.Tick(position, Vector2.zero, 4f, 10f, false);
                Assert.AreEqual(frame.Phase, paused.Phase);
                Assert.AreEqual(Vector2.zero, paused.Velocity);
            }
            Assert.IsTrue(sawOffset);
            Assert.IsTrue(sawAttack);
            Assert.IsTrue(sawDifferentOffset);
        }

        [Test]
        public void CommittedPursuit_HoldsCourseThenRetargets()
        {
            var controller = new EnemyMovementController(
                new EnemyMovementProfile(EnemyMovementKind.CommittedPursuit, cycleSeconds: 1f),
                new System.Random(9));

            var first = controller.Tick(Vector2.zero, Vector2.right, 3f, 0.1f, true);
            var held = controller.Tick(Vector2.zero, Vector2.up, 3f, 10f, true);
            var retargeted = controller.Tick(Vector2.zero, Vector2.up, 3f, 0.1f, true);

            Assert.AreEqual(Vector2.right * 3f, first.Velocity);
            Assert.AreEqual(Vector2.right * 3f, held.Velocity, "The old course can carry the enemy past the player.");
            Assert.AreEqual(Vector2.up * 3f, retargeted.Velocity);
        }

        [Test]
        public void BlockedSidestep_WhenForwardProgressStops_TemporarilyMovesSidewaysAndPausesSafely()
        {
            var profile = new EnemyMovementProfile(EnemyMovementKind.BlockedSidestep,
                preferredDistance: 1.1f, lateralStrength: 2f,
                blockedTriggerSeconds: .4f, blockedProgressFraction: .35f,
                sidestepSeconds: .9f, sidestepCooldownSeconds: 1.2f,
                sidestepNearDistance: 2.2f, sidestepNearSeconds: 1.2f);
            var controller = new EnemyMovementController(profile, new System.Random(7));
            var position = Vector2.right * 3f;

            Assert.AreEqual(EnemyMovementPhase.Seeking,
                controller.Tick(position, Vector2.zero, 1f, .2f, true).Phase);
            Assert.AreEqual(EnemyMovementPhase.Seeking,
                controller.Tick(position, Vector2.zero, 1f, .2f, true).Phase);
            var sidestep = controller.Tick(position, Vector2.zero, 1f, .2f, true);
            Assert.AreEqual(EnemyMovementPhase.Sidestepping, sidestep.Phase);
            Assert.Greater(Mathf.Abs(sidestep.Velocity.y), .5f);
            Assert.Less(sidestep.Velocity.x, 0f, "The detour must still advance toward the player.");
            Assert.AreEqual(EnemyMovementPhase.Sidestepping,
                controller.Tick(position, Vector2.zero, 1f, 10f, false).Phase);
            Assert.AreEqual(EnemyMovementPhase.Sidestepping,
                controller.Tick(position, Vector2.zero, 1f, .2f, true).Phase);
            for (var i = 0; i < 5; i++) controller.Tick(position, Vector2.zero, 1f, .2f, true);
            Assert.AreEqual(EnemyMovementPhase.Seeking, controller.Phase);
        }

        [Test]
        public void BlockedSidestep_WhenProgressOnlySlightlySlows_StillTakesWideDetour()
        {
            var profile = new EnemyMovementProfile(EnemyMovementKind.BlockedSidestep,
                preferredDistance: .5f, lateralStrength: 3f,
                blockedTriggerSeconds: .25f, blockedProgressFraction: .95f,
                sidestepSeconds: 1.8f, sidestepCooldownSeconds: 1.2f,
                sidestepNearDistance: 2.2f, sidestepNearSeconds: 1.2f);
            var controller = new EnemyMovementController(profile, new System.Random(5));
            var position = new Vector2(2f, 0f);
            controller.Tick(position, Vector2.zero, 1f, .1f, true);
            for (var i = 0; i < 3; i++)
            {
                position.x -= .09f; // 90% of the commanded 0.1 wu: a small collision slowdown.
                var frame = controller.Tick(position, Vector2.zero, 1f, .1f, true);
                if (i < 2) Assert.AreEqual(EnemyMovementPhase.Seeking, frame.Phase);
                else
                {
                    Assert.AreEqual(EnemyMovementPhase.Sidestepping, frame.Phase);
                    Assert.Greater(Mathf.Abs(frame.Velocity.y), .9f);
                }
            }
        }

        [Test]
        public void BlockedSidestep_NearPlayerEventuallySweepsEvenAtFullSpeed()
        {
            var profile = new EnemyMovementProfile(EnemyMovementKind.BlockedSidestep,
                preferredDistance: .5f, lateralStrength: 3f,
                blockedTriggerSeconds: .25f, blockedProgressFraction: .95f,
                sidestepSeconds: 1.8f, sidestepCooldownSeconds: 1.2f,
                sidestepNearDistance: 2.2f, sidestepNearSeconds: 1.2f);
            var controller = new EnemyMovementController(profile, new System.Random(6));
            var swept = false;
            for (var i = 0; i < 14; i++)
            {
                var position = new Vector2(2f - i * .1f, 0f);
                var frame = controller.Tick(position, Vector2.zero, 1f, .1f, true);
                swept |= frame.Phase == EnemyMovementPhase.Sidestepping;
            }
            Assert.IsTrue(swept, "A moving blob must not bypass the slowdown trigger indefinitely.");
        }

        [Test]
        public void ArcPassPursuit_NearTargetAlternatesArcAndDirectApproach()
        {
            var profile = new EnemyMovementProfile(EnemyMovementKind.ArcPassPursuit,
                preferredDistance: 3f, lateralStrength: 1.2f,
                cycleSeconds: 2.2f, directPursuitSeconds: .8f);
            var controller = new EnemyMovementController(profile, new System.Random(8));
            Assert.AreEqual(EnemyMovementPhase.Seeking,
                controller.Tick(Vector2.right * 4f, Vector2.zero, 1f, .1f, true).Phase);
            var sawArc = false;
            var sawDirect = false;
            for (var i = 0; i < 45; i++)
            {
                var frame = controller.Tick(Vector2.right * 2f, Vector2.zero, 1f, .1f, true);
                sawArc |= frame.Phase == EnemyMovementPhase.ArcPassing && Mathf.Abs(frame.Velocity.y) > .1f;
                sawDirect |= frame.Phase == EnemyMovementPhase.Seeking && frame.Velocity == Vector2.left;
            }
            Assert.IsTrue(sawArc);
            Assert.IsTrue(sawDirect);
        }

        [Test]
        public void InertialPursuit_WhenTargetTurns_ChangesDirectionGradually()
        {
            var controller = new EnemyMovementController(new EnemyMovementProfile(
                EnemyMovementKind.InertialPursuit, turnResponseSeconds: 1.2f));
            Assert.AreEqual(Vector2.right,
                controller.Tick(Vector2.zero, Vector2.right, 1f, .1f, true).Velocity);
            var turning = controller.Tick(Vector2.zero, Vector2.up, 1f, .1f, true);
            Assert.AreEqual(EnemyMovementPhase.InertialPursuit, turning.Phase);
            Assert.Greater(turning.Velocity.x, 0f);
            Assert.Greater(turning.Velocity.y, 0f);
            Assert.AreEqual(EnemyMovementPhase.InertialPursuit,
                controller.Tick(Vector2.zero, Vector2.up, 1f, 10f, false).Phase);

            var reverse = new EnemyMovementController(new EnemyMovementProfile(
                EnemyMovementKind.InertialPursuit, turnResponseSeconds: 1.2f), new System.Random(9));
            reverse.Tick(Vector2.zero, Vector2.right, 1f, .02f, true);
            EnemyMovementFrame frame = default;
            for (var i = 0; i < 210; i++)
                frame = reverse.Tick(Vector2.zero, Vector2.left, 1f, .02f, true);
            Assert.Less(frame.Velocity.x, -.9f, "A target exactly behind the enemy must still be reachable.");
        }

        [Test]
        public void MovementVariants_SelectByChanceAndLeaveRemainderForPrimaryMovement()
        {
            var offset = new EnemyMovementProfile(EnemyMovementKind.OffsetPursuit, 1f, 0.5f,
                cycleSeconds: 2f, directPursuitSeconds: 1f);
            var committed = new EnemyMovementProfile(EnemyMovementKind.CommittedPursuit, cycleSeconds: 1f);
            var definition = new EnemyDefinition("FIXTURE-VARIANTS", 1f, 1f, 1f, 1f, 1f,
                movementVariants: new[]
                {
                    new EnemyMovementVariant(0.25f, offset),
                    new EnemyMovementVariant(0.15f, committed)
                });

            Assert.AreSame(offset, definition.SelectMovement(0.1f));
            Assert.AreSame(committed, definition.SelectMovement(0.3f));
            Assert.AreSame(definition.Movement, definition.SelectMovement(0.8f));
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
