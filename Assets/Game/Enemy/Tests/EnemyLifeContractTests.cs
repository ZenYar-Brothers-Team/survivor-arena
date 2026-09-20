using System;
using System.Linq;
using Game.Content;
using Game.Pooling;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Enemy.Tests
{
    public sealed class EnemyLifeContractTests
    {
        private GameObject _root;
        private GameObject _target;
        private RunController _run;
        private GameObjectPool<EnemyRuntime> _pool;
        private FakeEnemyLifecycleSink _sink;
        private int _baseline;

        [SetUp]
        public void SetUp()
        {
            _baseline = EnemyRegistry.Count;
            _root = new GameObject("enemy-life-test");
            _target = new GameObject("target");
            _target.transform.SetParent(_root.transform);
            _run = _root.AddComponent<RunController>();
            if (!_run.IsInitialized) _run.Initialize();
            _run.Model.Start();
            _pool = new GameObjectPool<EnemyRuntime>(EnemyFactory.CreateInstance, _root.transform);
            _sink = new FakeEnemyLifecycleSink();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Assert.AreEqual(_baseline, EnemyRegistry.Count);
        }

        [Test]
        public void DeathSnapshot_RetainsSourcePositionAndIdentityAfterPoolReuse()
        {
            var enemy = Spawn();
            var id = enemy.LifeId;
            var location = enemy.Position;
            enemy.ApplyDamage(new EnemyDamageRequest("FIXTURE-SKILL-TEST", 10f));
            var death = _sink.Events.Single(e => e.Kind == EnemyLifeEventKind.Died);
            Assert.AreEqual(0f, enemy.TakeDamage(10f));
            Assert.AreEqual(_baseline, EnemyRegistry.Count);
            Assert.AreEqual(1, _pool.InactiveCount);
            var reused = Spawn();
            reused.transform.position = Vector3.one * 20f;
            Assert.AreSame(enemy, reused);
            Assert.AreNotEqual(id, reused.LifeId);
            Assert.AreEqual(id, death.LifeId);
            Assert.AreEqual(_run.Model.RunId, death.RunId);
            Assert.AreEqual(location, death.Position);
            Assert.AreEqual(new ContentId("FIXTURE-SKILL-TEST"), death.DamageSourceId);
            Assert.AreEqual(EnemyCategory.Ordinary, death.Category);
            Assert.AreEqual(EnemyLifeReason.PoolReuse, reused.LastLifeEvent.Reason);
            Assert.AreEqual(10f, reused.Health.CurrentHealth);
            Assert.IsTrue(reused.GetComponent<CircleCollider2D>().enabled);
            Assert.AreEqual(Vector2.zero, reused.GetComponent<Rigidbody2D>().linearVelocity);
        }

        [TestCase(EnemyLifeReason.Cleanup)]
        [TestCase(EnemyLifeReason.Escaped)]
        public void NonDeathDespawn_DoesNotPublishKill(EnemyLifeReason reason)
        {
            var enemy = Spawn();
            enemy.Despawn(reason);
            enemy.Despawn(reason);
            Assert.AreEqual(2, _sink.Events.Count);
            Assert.AreEqual(EnemyLifeEventKind.Despawned, _sink.Events[1].Kind);
            Assert.AreEqual(reason, _sink.Events[1].Reason);
            Assert.IsNull(_sink.Events[1].DamageSourceId);
        }

        [Test]
        public void SynchronousLethalCallbacks_CannotDoubleKillHealOrReuseMidDispatch()
        {
            var enemy = Spawn();
            var definition = enemy.Definition;
            var deaths = 0;
            enemy.Health.Damaged += _ =>
            {
                Assert.IsTrue(enemy.Health.IsDead);
                Assert.AreEqual(0f, enemy.Health.Heal(10f));
                Assert.AreEqual(0f, enemy.TakeDamage(10f));
            };
            enemy.Died += value =>
            {
                deaths++;
                value.Despawn();
                Assert.AreEqual(0, _pool.InactiveCount);
                Assert.Throws<InvalidOperationException>(() => value.Initialize(definition, _target.transform, _run));
            };
            enemy.TakeDamage(10f);
            Assert.AreEqual(1, deaths);
            Assert.AreEqual(1, _pool.InactiveCount);
            Assert.AreEqual(1, _sink.Events.Count(e => e.Kind == EnemyLifeEventKind.Died));
            Assert.AreEqual(EnemyLifeReason.Killed, _sink.Events.Last().Reason);
            Spawn().TakeDamage(10f);
            Assert.AreEqual(1, deaths, "Per-life subscribers must not leak into another rent.");
        }

        [Test]
        public void LiveReinitialize_EndsOldLifeWithoutKillAndRestoresRegistry()
        {
            var enemy = Spawn();
            var firstId = enemy.LifeId;
            enemy.Initialize(enemy.Definition, _target.transform, _run, _sink, pool: _pool,
                category: EnemyCategory.Traveler);
            Assert.AreNotEqual(firstId, enemy.LifeId);
            Assert.AreEqual(EnemyLifeReason.Reinitialized, _sink.Events[1].Reason);
            Assert.AreEqual(EnemyCategory.Traveler, enemy.Category);
            Assert.AreEqual(_baseline + 1, EnemyRegistry.Count);
            Assert.IsTrue(enemy.gameObject.activeSelf);
            Assert.IsFalse(_sink.Events.Any(e => e.Kind == EnemyLifeEventKind.Died));
        }

        private EnemyRuntime Spawn()
        {
            return EnemyFactory.Spawn(new EnemyDefinition("FIXTURE-ENEMY-LIFE", 10f, 1f, 1f, 1f, 0.5f, 3f),
                new Vector2(2f, 4f), _target.transform, _run, _root.transform,
                pool: _pool, lifecycleSink: _sink);
        }
    }
}
