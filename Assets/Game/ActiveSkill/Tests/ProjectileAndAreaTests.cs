using System.Reflection;
using Game.Enemy;
using Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    public class ProjectileAndAreaTests
    {
        private GameObject _runObject;
        private GameObject _targetObject;
        private RunController _runController;
        private EnemyRuntime _firstEnemy;
        private EnemyRuntime _secondEnemy;
        private FixtureProjectileRuntime _projectile;

        [SetUp]
        public void SetUp()
        {
            _runObject = new GameObject("RunController");
            _runController = _runObject.AddComponent<RunController>();
            typeof(RunController)
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_runController, null);
            _targetObject = new GameObject("Target");
        }

        [TearDown]
        public void TearDown()
        {
            if (_projectile != null)
                _projectile.Despawn();
            if (_firstEnemy != null)
                _firstEnemy.Despawn();
            if (_secondEnemy != null)
                _secondEnemy.Despawn();
            if (_targetObject != null)
                Object.DestroyImmediate(_targetObject);
            if (_runObject != null)
                Object.DestroyImmediate(_runObject);
        }

        [Test]
        public void Projectile_MovesOnlyWhileRunningAndDespawnsWhenRunEnds()
        {
            _runController.Model.Start();
            _projectile = FixtureProjectileFactory.Spawn(CreateProjectile(lifetime: 2f), _runController);

            _projectile.Simulate(0.5f);
            Assert.AreEqual(5f, _projectile.Position.x, 0.0001f);

            _runController.Model.Pause();
            _projectile.Simulate(10f);
            Assert.AreEqual(5f, _projectile.Position.x, 0.0001f);

            _runController.Model.Resume();
            _runController.Model.Kill();
            _projectile.Simulate(0f);
            Assert.IsTrue(_projectile == null);
        }

        [Test]
        public void DamageArea_DamagesEachEnemyOnceIncludingDirectTarget()
        {
            _firstEnemy = SpawnEnemy(Vector2.zero);
            _secondEnemy = SpawnEnemy(new Vector2(0.4f, 0f));
            Physics2D.SyncTransforms();
            var damage = new EnemyDamageRequest("FIXTURE-SKILL-BOLT", 2f);

            var count = EnemyDamageArea.Apply(Vector2.zero, 1f, damage, _firstEnemy);

            Assert.AreEqual(2, count);
            Assert.AreEqual(3f, _firstEnemy.Health.CurrentHealth);
            Assert.AreEqual(3f, _secondEnemy.Health.CurrentHealth);
        }

        [Test]
        public void Projectile_ImpactKillsThroughDamageContractAndDespawns()
        {
            _runController.Model.Start();
            _firstEnemy = SpawnEnemy(Vector2.zero, maxHealth: 1f);
            _projectile = FixtureProjectileFactory.Spawn(CreateProjectile(lifetime: 2f), _runController);

            var hit = _projectile.TryImpact(_firstEnemy, Vector2.zero);

            Assert.IsTrue(hit);
            Assert.IsTrue(_firstEnemy == null);
            Assert.IsTrue(_projectile == null);
        }

        private EnemyRuntime SpawnEnemy(Vector2 position, float maxHealth = 5f)
        {
            var definition = new EnemyDefinition("FIXTURE-ENEMY", maxHealth, 1f, 0f, 0f, 1f);
            return EnemyFactory.Spawn(definition, position, _targetObject.transform, _runController);
        }

        private static ActiveSkillProjectile CreateProjectile(float lifetime)
        {
            return new ActiveSkillProjectile(
                Vector2.zero,
                Vector2.right,
                10f,
                lifetime,
                0.15f,
                0.5f,
                new EnemyDamageRequest("FIXTURE-SKILL-BOLT", 1f));
        }
    }
}
