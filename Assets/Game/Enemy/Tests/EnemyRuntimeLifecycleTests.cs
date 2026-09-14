using Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    public class EnemyRuntimeLifecycleTests
    {
        private GameObject _target;
        private GameObject _runObject;
        private EnemyRuntime _enemy;

        [SetUp]
        public void SetUp()
        {
            _target = new GameObject("Target");
            _runObject = new GameObject("RunController");
        }

        [TearDown]
        public void TearDown()
        {
            if (_enemy != null)
                _enemy.Despawn();
            if (_target != null)
                Object.DestroyImmediate(_target);
            if (_runObject != null)
                Object.DestroyImmediate(_runObject);
        }

        [Test]
        public void Spawn_AppliesDefinitionToRuntimeAndPhysics()
        {
            var registryCount = EnemyRegistry.Count;
            var definition = new EnemyDefinition("FIXTURE-ENEMY", 12f, 1.5f, 2f, 3f, 0.5f);
            var runController = _runObject.AddComponent<RunController>();

            _enemy = EnemyFactory.Spawn(definition, new Vector2(2f, 3f), _target.transform, runController);

            Assert.AreSame(definition, _enemy.Definition);
            Assert.AreEqual(12f, _enemy.Health.MaxHealth);
            Assert.AreEqual(new Vector3(1.5f, 1.5f, 1.5f), _enemy.transform.localScale);
            Assert.AreEqual(new Vector3(2f, 3f, 0f), _enemy.transform.position);
            Assert.AreEqual(0.5f, _enemy.GetComponent<CircleCollider2D>().radius);
            Assert.AreEqual(0f, _enemy.GetComponent<Rigidbody2D>().gravityScale);
            Assert.AreEqual(registryCount + 1, EnemyRegistry.Count);
            Assert.IsTrue(EnemyRegistry.TryFindNearest(Vector2.zero, out var nearest));
            Assert.AreSame(_enemy, nearest);
        }

        [Test]
        public void LethalDamage_EmitsDeathAndDespawnOnce()
        {
            var registryCount = EnemyRegistry.Count;
            var definition = new EnemyDefinition("FIXTURE-ENEMY", 10f, 1f, 1f, 1f, 0.5f);
            var runController = _runObject.AddComponent<RunController>();
            _enemy = EnemyFactory.Spawn(definition, Vector2.zero, _target.transform, runController);
            var deaths = 0;
            var despawns = 0;
            _enemy.Died += _ => deaths++;
            _enemy.Despawned += _ => despawns++;

            _enemy.TakeDamage(10f);

            Assert.AreEqual(1, deaths);
            Assert.AreEqual(1, despawns);
            Assert.IsTrue(_enemy == null);
            Assert.AreEqual(registryCount, EnemyRegistry.Count);
        }
    }
}
