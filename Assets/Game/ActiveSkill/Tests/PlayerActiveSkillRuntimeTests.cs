using System.Reflection;
using Game.Character;
using Game.Enemy;
using Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    public class PlayerActiveSkillRuntimeTests
    {
        private GameObject _playerObject;
        private GameObject _runObject;
        private GameObject _enemyTargetObject;
        private PlayerCharacterRuntime _player;
        private RunController _runController;
        private EnemyRuntime _spawnedEnemy;

        [SetUp]
        public void SetUp()
        {
            _playerObject = new GameObject("Player");
            _player = _playerObject.AddComponent<PlayerCharacterRuntime>();
            InvokeAwake(_player);
            _runObject = new GameObject("RunController");
            _runController = _runObject.AddComponent<RunController>();
            InvokeAwake(_runController);
        }

        [TearDown]
        public void TearDown()
        {
            if (_spawnedEnemy != null)
                _spawnedEnemy.Despawn();
            if (_enemyTargetObject != null)
                Object.DestroyImmediate(_enemyTargetObject);
            if (_playerObject != null)
                Object.DestroyImmediate(_playerObject);
            if (_runObject != null)
                Object.DestroyImmediate(_runObject);
        }

        [Test]
        public void Tick_AutoTargetsAndKillsThroughEnemyDamageContract()
        {
            var enemy = CreateEnemy(maxHealth: 5f);
            var launcher = new ApplyingLauncher(enemy);
            var runtime = CreateRuntime(enemy, launcher, baseDamage: 5f);
            _runController.Model.Start();

            var triggered = runtime.Tick(0f);

            Assert.IsTrue(triggered);
            Assert.AreEqual(1, runtime.TriggerCount);
            Assert.AreEqual("FIXTURE-SKILL-BOLT", launcher.LastProjectile.Damage.SourceId.ToString());
            Assert.AreEqual(5f, launcher.LastProjectile.Damage.Amount);
            Assert.IsTrue(enemy == null);
        }

        [Test]
        public void Tick_UsesCharacterDamageAndCooldownMultipliers()
        {
            var target = new FakeReceiver(new Vector2(2f, 0f));
            var launcher = new RecordingLauncher();
            _player.SetModifier("fixture", new CharacterStatModifier(
                activeSkillDamageMultiplierBonus: 1f,
                activeSkillCooldownMultiplierBonus: -0.5f));
            var runtime = CreateRuntime(target, launcher, baseDamage: 5f, cooldown: 2f);
            _runController.Model.Start();

            Assert.IsTrue(runtime.Tick(0f));
            Assert.AreEqual(10f, launcher.LastProjectile.Damage.Amount);
            Assert.IsFalse(runtime.Tick(0.99f));
            Assert.IsTrue(runtime.Tick(0.01f));
        }

        [Test]
        public void Tick_PauseAndEndStopAutomaticTriggers()
        {
            var target = new FakeReceiver(Vector2.right);
            var launcher = new RecordingLauncher();
            var runtime = CreateRuntime(target, launcher, cooldown: 1f);
            _runController.Model.Start();
            Assert.IsTrue(runtime.Tick(0f));

            _runController.Model.Pause();
            Assert.IsFalse(runtime.Tick(10f));
            Assert.AreEqual(1, launcher.Count);

            _runController.Model.Resume();
            Assert.IsFalse(runtime.Tick(0.99f));
            Assert.IsTrue(runtime.Tick(0.01f));
            Assert.AreEqual(2, launcher.Count);

            _runController.Model.Kill();
            Assert.IsFalse(runtime.Tick(10f));
            Assert.AreEqual(2, launcher.Count);
        }

        [Test]
        public void Tick_DoesNotConsumeCooldownWhenNoTargetExists()
        {
            var launcher = new RecordingLauncher();
            var targetProvider = new FixedTargetProvider(null);
            var runtime = _playerObject.AddComponent<PlayerActiveSkillRuntime>();
            runtime.Initialize(
                CreateDefinition(1f, 1f),
                _player,
                _runController,
                targetProvider,
                launcher);
            _runController.Model.Start();

            Assert.IsFalse(runtime.Tick(10f));
            Assert.AreEqual(0, launcher.Count);

            targetProvider.Target = new FakeReceiver(Vector2.right);
            Assert.IsTrue(runtime.Tick(0f));
        }

        private PlayerActiveSkillRuntime CreateRuntime(
            IEnemyDamageReceiver target,
            IActiveSkillProjectileLauncher launcher,
            float baseDamage = 1f,
            float cooldown = 1f)
        {
            var runtime = _playerObject.AddComponent<PlayerActiveSkillRuntime>();
            runtime.Initialize(
                CreateDefinition(baseDamage, cooldown),
                _player,
                _runController,
                new FixedTargetProvider(target),
                launcher);
            return runtime;
        }

        private EnemyRuntime CreateEnemy(float maxHealth)
        {
            _enemyTargetObject = new GameObject("Enemy Target Transform");
            var definition = new EnemyDefinition("FIXTURE-ENEMY-TARGET", maxHealth, 1f, 0f, 0f, 1f);
            _spawnedEnemy = EnemyFactory.Spawn(
                definition,
                Vector2.right,
                _enemyTargetObject.transform,
                _runController);
            return _spawnedEnemy;
        }

        private static ActiveSkillDefinition CreateDefinition(float damage, float cooldown)
        {
            return new ActiveSkillDefinition(
                "FIXTURE-SKILL-BOLT",
                damage,
                cooldown,
                10f,
                2f,
                0.15f,
                0.5f);
        }

        private static void InvokeAwake(object behaviour)
        {
            behaviour.GetType()
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(behaviour, null);
        }

        private sealed class FixedTargetProvider : IActiveSkillTargetProvider
        {
            public IEnemyDamageReceiver Target { get; set; }

            public FixedTargetProvider(IEnemyDamageReceiver target) => Target = target;

            public bool TryGetTarget(Vector2 origin, out IEnemyDamageReceiver target)
            {
                target = Target;
                return target != null;
            }
        }

        private class RecordingLauncher : IActiveSkillProjectileLauncher
        {
            public int Count { get; private set; }
            public ActiveSkillProjectile LastProjectile { get; private set; }

            public virtual void Launch(ActiveSkillProjectile projectile)
            {
                Count++;
                LastProjectile = projectile;
            }
        }

        private sealed class ApplyingLauncher : RecordingLauncher
        {
            private readonly IEnemyDamageReceiver _target;

            public ApplyingLauncher(IEnemyDamageReceiver target) => _target = target;

            public override void Launch(ActiveSkillProjectile projectile)
            {
                base.Launch(projectile);
                _target.ApplyDamage(projectile.Damage);
            }
        }

        private sealed class FakeReceiver : IEnemyDamageReceiver
        {
            public bool IsAlive { get; private set; } = true;
            public Vector2 Position { get; }

            public FakeReceiver(Vector2 position) => Position = position;

            public float ApplyDamage(EnemyDamageRequest request) => request.Amount;
        }
    }
}
