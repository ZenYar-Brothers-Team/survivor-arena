using System;
using System.Linq;
using System.Reflection;
using Game.Character;
using Game.Combat;
using Game.Content;
using Game.Enemy.Json;
using Game.Pooling;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Game.Enemy.Tests
{
    public sealed class EnemyPatternIntegrationTests
    {
        private GameObject _root;
        private RunController _run;
        private PlayerCharacterRuntime _player;
        private GameObjectPool<EnemyProjectileRuntime> _pool;
        private int _registryBaseline;
        [SetUp]
        public void SetUp()
        {
            _registryBaseline = EnemyRegistry.Count;
            _root = new GameObject("pattern integration");
            _run = _root.AddComponent<RunController>();
            _run.Initialize();
            _run.Model.Start();
            var target = new GameObject("target");
            target.transform.SetParent(_root.transform);
            _player = target.AddComponent<PlayerCharacterRuntime>();
            _player.Initialize(new CharacterBaseStats(100f, 3f), _run);
            target.AddComponent<CircleCollider2D>();
            _pool = new GameObjectPool<EnemyProjectileRuntime>(() =>
            {
                var projectile = EnemyProjectileFactory.CreateInstance();
                projectile.gameObject.AddComponent<TrailRenderer>();
                return projectile;
            }, _root.transform);
        }
        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Assert.AreEqual(_registryBaseline, EnemyRegistry.Count);
        }
        [TestCase(EnemyProjectilePattern.Single, 1)]
        [TestCase(EnemyProjectilePattern.Fan, 3)]
        [TestCase(EnemyProjectilePattern.Burst, 1)]
        [TestCase(EnemyProjectilePattern.Ring, 8)]
        [TestCase(EnemyProjectilePattern.Cross, 4)]
        [TestCase(EnemyProjectilePattern.Spiral, 10)]
        [TestCase(EnemyProjectilePattern.Explosive, 1)]
        public void FixturePattern_TelegraphsThenHitsWithConfiguredSourceAndLifetime(EnemyProjectilePattern pattern, int count)
        {
            var definition = FixtureEnemyCatalog.Create().Single(d => d.Attack?.Pattern == pattern);
            var profile = definition.Attack;
            var controller = new EnemyAttackController(profile);
            Assert.IsEmpty(controller.Tick(0, true, Vector2.right));
            Assert.AreEqual(EnemyAttackPhase.Telegraphing, controller.Phase);
            Assert.IsEmpty(controller.Tick(10, false, Vector2.up));
            Assert.AreEqual(Vector2.right, controller.AimDirection);
            var shots = controller.Tick(profile.TelegraphSeconds, true, Vector2.right);
            Assert.AreEqual(count, shots.Length);
            foreach (var shot in shots) Assert.AreEqual(1f, shot.Direction.magnitude, .0001f);
            var source = new CombatSource(new CombatIdentity(Guid.NewGuid(), _run.Model.RunId, definition.Id, CombatEntityCategory.Boss), definition.Id, CombatSourceOrigin.EnemyProjectile);
            var projectile = Spawn(profile, source);
            Assert.AreEqual(profile.ProjectileLifetimeSeconds, projectile.RemainingSeconds);
            CombatResult hit = default;
            _player.CombatResolved += result => hit = result;
            Invoke(projectile, "OnTriggerEnter2D", _player.GetComponent<CircleCollider2D>());
            Assert.AreEqual(profile.Damage, hit.Health.Actual, .0001f);
            Assert.AreEqual(source.Owner.LifeId, hit.Source.Owner.LifeId);
            Assert.AreEqual(CombatEntityCategory.Boss, hit.Source.Owner.Category);
            Assert.IsFalse(projectile.IsActive);
            Assert.AreEqual(Guid.Empty, projectile.Source.Owner.LifeId);
        }
        [Test]
        public void Projectile_PauseFreezesAndTerminalReturnsImmediatelyWithoutPhysicsTick()
        {
            var profile = Profile();
            var projectile = Spawn(profile);
            projectile.Tick(.25f);
            Assert.Greater(projectile.GetComponent<Rigidbody2D>().linearVelocity.x, 0);
            _run.TogglePause();
            Assert.AreEqual(Vector2.zero, projectile.GetComponent<Rigidbody2D>().linearVelocity);
            projectile.Tick(100);
            Assert.AreEqual(profile.ProjectileLifetimeSeconds - .25f, projectile.RemainingSeconds, .0001f);
            Assert.IsTrue(projectile.IsActive);
            _run.Model.Stop();
            Assert.IsFalse(projectile.IsActive);
            Assert.AreEqual(1, _pool.InactiveCount);
            Assert.IsNull(projectile.Profile);
        }
        [Test]
        public void PoolReuse_ResetsSourceLifetimeVelocityRendererTrailAndOldRunSubscription()
        {
            var source = new CombatSource(new CombatIdentity(Guid.NewGuid(), _run.Model.RunId, "FIXTURE-OWNER", CombatEntityCategory.Traveler), "FIXTURE-OWNER", CombatSourceOrigin.EnemyProjectile);
            var projectile = Spawn(Profile(), source);
            projectile.Tick(.5f);
            projectile.GetComponent<TrailRenderer>().AddPosition(Vector3.one);
            projectile.GetComponent<SpriteRenderer>().flipX = true;
            projectile.transform.localRotation = Quaternion.Euler(0, 0, 90);
            var previousRun = _run.Model;
            projectile.Shutdown();
            Assert.AreEqual(0, projectile.GetComponent<TrailRenderer>().positionCount);
            Assert.IsFalse(projectile.GetComponent<TrailRenderer>().emitting);
            Assert.AreEqual(Guid.Empty, projectile.Source.Owner.LifeId);
            for (var i = 0; i < 3; i++)
            {
                var reused = Spawn(Profile());
                Assert.AreSame(projectile, reused);
                Assert.AreEqual(2f, reused.RemainingSeconds);
                Assert.AreEqual(Vector2.zero, reused.GetComponent<Rigidbody2D>().linearVelocity);
                Assert.IsFalse(reused.GetComponent<SpriteRenderer>().flipX);
                Assert.AreEqual(Quaternion.identity, reused.transform.localRotation);
                Assert.AreEqual(0, reused.GetComponent<TrailRenderer>().positionCount);
                reused.Shutdown();
            }
            _run.Shutdown();
            _run.Initialize();
            _run.Model.Start();
            var final = Spawn(Profile());
            previousRun.Start();
            Assert.IsTrue(final.IsActive);
        }
        [Test]
        public void OldRunTerminalCallback_DoesNotDespawnReinitializedProjectile()
        {
            var next = new GameObject("next run");
            next.transform.SetParent(_root.transform);
            var nextRun = next.AddComponent<RunController>();
            nextRun.Initialize();
            nextRun.Model.Start();
            EnemyProjectileRuntime projectile = null;
            _run.Model.StateChanged += state =>
            {
                if (state != RunState.Stopped) return;
                projectile.Initialize(Profile(), Vector2.right, _player, nextRun, _pool);
            };
            projectile = Spawn(Profile());
            _run.Model.Stop();
            Assert.IsTrue(projectile.IsActive);
            Assert.AreEqual(0, _pool.InactiveCount);
            nextRun.Model.Stop();
            Assert.IsFalse(projectile.IsActive);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Expiry_ExplodesOnlyInsideRadius_OrdinaryMissDoesNotHit(bool explosive)
        {
            var profile = new EnemyAttackProfile(explosive ? EnemyProjectilePattern.Explosive : EnemyProjectilePattern.Single,
                4, 1, 1, 1, explosionRadius: explosive ? 1 : 0);
            _player.transform.position = Vector3.right * 2;
            var projectile = Spawn(profile);
            projectile.Tick(1);
            Assert.AreEqual(100, _player.Health.CurrentHealth);
            Assert.IsFalse(projectile.IsActive);
            _player.transform.position = Vector3.right * .5f;
            projectile = Spawn(profile);
            projectile.Tick(1);
            Assert.AreEqual(explosive ? 96 : 100, _player.Health.CurrentHealth);
        }
        [Test]
        public void ImpactCallback_CanRentSameProjectileWithoutOldHitDespawningNewLife()
        {
            var projectile = Spawn(Profile());
            EnemyProjectileRuntime reused = null;
            _player.CombatResolved += _ => reused = Spawn(Profile());
            Invoke(projectile, "OnTriggerEnter2D", _player.GetComponent<CircleCollider2D>());
            Assert.AreSame(projectile, reused);
            Assert.IsTrue(reused.IsActive);
            Assert.AreEqual(2f, reused.RemainingSeconds);
        }
        [Test]
        public void ScaledDash_SlowAndKnockbackComposeWithoutChangingLockedDirectionOrTiming()
        {
            var original = FixtureEnemyCatalog.Create().Single(d => d.Movement.Kind == EnemyMovementKind.TelegraphedDash);
            var definition = WaveEnemyScaler.Apply(original, new WaveEnemyModifiers(speedMultiplier: 2, attackDamageMultiplier: 2));
            Assert.AreSame(original.DashContactControls, definition.DashContactControls);
            Assert.AreEqual(original.Attack.TelegraphSeconds, definition.Attack.TelegraphSeconds);
            var movement = new EnemyMovementController(definition.Movement);
            movement.Tick(Vector2.zero, Vector2.right, definition.MovementSpeed, definition.Movement.DashCooldownSeconds, true);
            var controls = new CombatControlState();
            controls.Apply(new CombatDamageRequest(default, 0, new CombatControlProfile(2, 2, .5f, 2), 0, 1), .25f, true);
            var control = controls.Tick(.1f, true);
            var frame = movement.Tick(Vector2.zero, Vector2.up, definition.MovementSpeed * control.MovementMultiplier,
                definition.Movement.DashTelegraphSeconds, true);
            Assert.AreEqual(EnemyMovementPhase.Dashing, frame.Phase);
            Assert.AreEqual(definition.MovementSpeed * .5f * definition.Movement.DashSpeedMultiplier, frame.Velocity.x, .0001f);
            Assert.AreEqual(0, frame.Velocity.y);
            Assert.AreEqual(.75f, control.KnockbackY, .0001f);
            var paused = movement.Tick(Vector2.zero, Vector2.up, definition.MovementSpeed, 10, false);
            Assert.AreEqual(frame.Phase, paused.Phase);
            Assert.AreEqual(Vector2.zero, paused.Velocity);
        }
        [TestCase(EnemyMovementKind.Orbit)]
        [TestCase(EnemyMovementKind.Zigzag)]
        [TestCase(EnemyMovementKind.ApproachRetreat)]
        public void Pause_PreservesObservableMovementPhase(EnemyMovementKind kind)
        {
            var movement = new EnemyMovementController(new EnemyMovementProfile(kind));
            var before = movement.Tick(Vector2.zero, Vector2.right, 1, .1f, true);
            var paused = movement.Tick(Vector2.zero, Vector2.up, 1, 10, false);
            Assert.AreEqual(before.Phase, paused.Phase);
            Assert.AreEqual(Vector2.zero, paused.Velocity);
        }
        private EnemyProjectileRuntime Spawn(EnemyAttackProfile profile, CombatSource source = default) =>
            EnemyProjectileFactory.Spawn(profile, Vector2.zero, Vector2.right, _player, _run, _root.transform, _pool, source);
        private static EnemyAttackProfile Profile() => new EnemyAttackProfile(EnemyProjectilePattern.Single, 2, 1, 2, 2);
        private static void Invoke(object target, string name, params object[] args) =>
            target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
    }
}
