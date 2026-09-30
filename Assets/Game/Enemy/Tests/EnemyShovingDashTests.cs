using System;
using System.Reflection;
using Game.Character;
using Game.Combat;
using Game.Enemy.Json;
using Game.Pooling;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Enemy.Tests
{
    /// <summary>DECISION-0119: fixed-length dash, wide telegraph, sideways shove and pass-through.</summary>
    public sealed class EnemyShovingDashTests
    {
        private GameObject _root;
        private PlayerCharacterRuntime _player;
        private RunController _run;
        private GameObjectPool<EnemyRuntime> _pool;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("shoving-dash-test");
            _run = _root.AddComponent<RunController>();
            if (!_run.IsInitialized) _run.Initialize();
            _run.Model.Start();
            var player = new GameObject("player");
            player.transform.SetParent(_root.transform);
            player.transform.position = Vector3.right * 10f;
            _player = player.AddComponent<PlayerCharacterRuntime>();
            _player.Initialize(new CharacterBaseStats(100f, 3f), _run, "FIXTURE-CHARACTER");
            _pool = new GameObjectPool<EnemyRuntime>(EnemyFactory.CreateInstance, _root.transform);
        }

        [TearDown] public void TearDown() => Object.DestroyImmediate(_root);

        private static EnemyMovementProfile ShovingDash() => new EnemyMovementProfile(EnemyMovementKind.TelegraphedDash,
            dashTelegraphSeconds: 0f, dashDurationSeconds: 1f, dashCooldownSeconds: 0.01f, dashDistance: 4.5f,
            dashTelegraphWidth: 1.6f, dashShoveRadius: 0.8f, dashShoveDistance: 1.5f, dashShoveSeconds: 0.25f);

        [Test]
        public void DashSpeed_FixedDistanceIgnoresBaseSpeed_ClassicDashKeepsMultiplier()
        {
            Assert.AreEqual(4.5f / 0.6f, new EnemyMovementProfile(EnemyMovementKind.TelegraphedDash,
                dashDurationSeconds: 0.6f, dashDistance: 4.5f).DashSpeed(0.65f), 1e-4f);
            Assert.AreEqual(0.65f * 4f, new EnemyMovementProfile(EnemyMovementKind.TelegraphedDash,
                dashSpeedMultiplier: 4f).DashSpeed(0.65f), 1e-4f);
        }

        [Test]
        public void Profile_ShoveNeedsKnockbackAndRejectsNegatives()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyMovementProfile(EnemyMovementKind.TelegraphedDash,
                dashShoveRadius: 0.8f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyMovementProfile(EnemyMovementKind.TelegraphedDash,
                dashDistance: -1f));
            Assert.IsFalse(new EnemyMovementProfile(EnemyMovementKind.TelegraphedDash).DashShoves);
        }

        [Test]
        public void Controller_FixedDistanceDashCoversExactlyTheConfiguredLength()
        {
            // A cooldown longer than one tick, so the first dash ends before the next one starts.
            var controller = new EnemyMovementController(new EnemyMovementProfile(EnemyMovementKind.TelegraphedDash,
                dashTelegraphSeconds: 0f, dashDurationSeconds: 1f, dashCooldownSeconds: 0.05f, dashDistance: 4.5f));
            var travelled = 0f;
            var started = false;
            for (var i = 0; i < 100; i++)
            {
                var frame = controller.Tick(Vector2.zero, Vector2.right * 10f, 0.65f, 0.02f, true);
                if (frame.Phase == EnemyMovementPhase.Dashing) { started = true; travelled += frame.Velocity.magnitude * 0.02f; }
                else if (started) break;
            }
            Assert.AreEqual(4.5f, travelled, 0.1f);
        }

        [Test]
        public void Catalog_FixedDistanceDashOmitsMultiplier_ButShoveRequiresItsKnockback()
        {
            var data = new EnemyMovementProfileData
            {
                Kind = "TelegraphedDash", DashTelegraphSeconds = .7f, DashDurationSeconds = .6f, DashCooldownSeconds = 4,
                DashDistance = 4.5f, DashTelegraphWidth = 1.6f, DashShoveRadius = .8f
            };
            Assert.Throws<InvalidOperationException>(() => FixtureEnemyCatalog.ToMovementProfile("FIXTURE", data));
            data.DashShoveDistance = 1.5f; data.DashShoveSeconds = .25f;
            var profile = FixtureEnemyCatalog.ToMovementProfile("FIXTURE", data);
            Assert.AreEqual(4.5f, profile.DashDistance);
            Assert.AreEqual(1.6f, profile.DashTelegraphWidth);
            Assert.IsTrue(profile.DashShoves);
            data.DashDistance = null;
            Assert.Throws<InvalidOperationException>(() => FixtureEnemyCatalog.ToMovementProfile("FIXTURE", data));
        }

        [Test]
        public void Dash_ShovesOrdinaryEnemiesSidewaysOncePerDash_AndBothSidesSplit()
        {
            var dasher = SpawnDasher();
            var above = SpawnOrdinary(new Vector2(0.4f, 0.3f));
            var below = SpawnOrdinary(new Vector2(0.4f, -0.3f));
            Physics2D.SyncTransforms();
            Invoke(dasher, "FixedUpdate");
            Assert.AreEqual(EnemyMovementPhase.Dashing, dasher.MovementPhase);
            Assert.Greater(above.Controls.KnockbackRemaining, 0f);
            Assert.Greater(below.Controls.KnockbackRemaining, 0f);
            Invoke(above, "FixedUpdate"); Invoke(below, "FixedUpdate");
            Assert.Greater(above.GetComponent<Rigidbody2D>().linearVelocity.y, 0f);
            Assert.Less(below.GetComponent<Rigidbody2D>().linearVelocity.y, 0f);
            var remaining = above.Controls.KnockbackRemaining;
            for (var i = 0; i < 3; i++) Invoke(dasher, "FixedUpdate");
            Assert.LessOrEqual(above.Controls.KnockbackRemaining, remaining, "A second query must not re-shove the same enemy.");
        }

        [Test]
        public void Dash_LeavesBossesTravelersAndFarEnemiesAlone_AndRestoresCollisionAfterwards()
        {
            var dasher = SpawnDasher();
            var traveler = EnemyFactory.Spawn(Ordinary(), new Vector2(0.4f, 0.3f), _player.transform, _run, _root.transform,
                pool: _pool, category: EnemyCategory.Traveler);
            var boss = EnemyFactory.Spawn(Ordinary(), new Vector2(0.4f, -0.3f), _player.transform, _run, _root.transform,
                pool: _pool, category: EnemyCategory.Boss);
            var far = SpawnOrdinary(new Vector2(0.4f, 3f));
            var collider = dasher.GetComponent<CircleCollider2D>();
            var before = collider.excludeLayers.value;
            Physics2D.SyncTransforms();
            Invoke(dasher, "FixedUpdate");
            Assert.AreNotEqual(before, collider.excludeLayers.value, "Dashing body passes through enemies and the player.");
            Assert.AreEqual(0f, traveler.Controls.KnockbackRemaining);
            Assert.AreEqual(0f, boss.Controls.KnockbackRemaining);
            Assert.AreEqual(0f, far.Controls.KnockbackRemaining);
            _run.Model.Stop();
            Invoke(dasher, "FixedUpdate");
            Assert.AreEqual(before, collider.excludeLayers.value);
        }

        [Test]
        public void Dash_HitsThePlayerOnceAndKeepsGoing()
        {
            var playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer < 0) Assert.Ignore("Physics layer 'Player' is not configured in this project.");
            _player.gameObject.layer = playerLayer;
            if (_player.GetComponent<Collider2D>() == null) _player.gameObject.AddComponent<CircleCollider2D>().radius = 0.4f;
            _player.transform.position = new Vector3(0.5f, 0f, 0f);
            var dasher = SpawnDasher(contactDamage: 10f);
            Physics2D.SyncTransforms();
            var health = _player.Health.CurrentHealth;
            Invoke(dasher, "FixedUpdate");
            Assert.AreEqual(health - 10f, _player.Health.CurrentHealth, 1e-3f);
            for (var i = 0; i < 3; i++) Invoke(dasher, "FixedUpdate");
            Assert.AreEqual(health - 10f, _player.Health.CurrentHealth, 1e-3f, "One contact hit per dash.");
            Assert.AreEqual(EnemyMovementPhase.Dashing, dasher.MovementPhase);
        }

        private EnemyRuntime SpawnDasher(float contactDamage = 0f) => EnemyFactory.Spawn(
            new EnemyDefinition("FIXTURE-KNIGHT", 100f, 1.45f, 0.65f, contactDamage, 1f, movement: ShovingDash(),
                dashContactControls: new CombatControlProfile(0.5f, 0.1f)),
            Vector2.zero, _player.transform, _run, _root.transform, pool: _pool);

        private static EnemyDefinition Ordinary() => new EnemyDefinition("FIXTURE-CROWD", 100f, 0.8f, 0f, 0f, 1f);

        private EnemyRuntime SpawnOrdinary(Vector2 position) => EnemyFactory.Spawn(Ordinary(), position,
            _player.transform, _run, _root.transform, pool: _pool);

        private static void Invoke(object target, string method, params object[] args) =>
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
    }
}
