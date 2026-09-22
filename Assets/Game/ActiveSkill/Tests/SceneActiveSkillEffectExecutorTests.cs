using System.Collections.Generic;
using Game.Content;
using Game.Enemy;
using Game.Presentation;
using Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    public class SceneActiveSkillEffectExecutorTests
    {
        private GameObject _runObject;
        private GameObject _owner;
        private RunController _runController;
        private SceneActiveSkillEffectExecutor _executor;
        private readonly List<EnemyRuntime> _enemies = new List<EnemyRuntime>();

        [SetUp]
        public void SetUp()
        {
            _runObject = new GameObject("RunController");
            _runController = _runObject.AddComponent<RunController>();
            TestLifecycle.InvokeAwake(_runController);
            _runController.Model.Start();
            _owner = new GameObject("Owner");
        }

        [TearDown]
        public void TearDown()
        {
            _executor?.Dispose();
            for (var i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i] != null)
                    _enemies[i].Despawn();
            }
            if (_owner != null)
                Object.DestroyImmediate(_owner);
            if (_runObject != null)
                Object.DestroyImmediate(_runObject);
        }

        [Test]
        public void DelayedArea_CountdownFreezesWhilePaused_ThenDamagesAtFixedPoint()
        {
            var enemy = SpawnEnemy(new Vector2(2f, 0f));
            _executor = new SceneActiveSkillEffectExecutor(_runController, new RecordingLauncher());
            var level = Level(
                ActiveSkillTargetingMode.NearestEnemy,
                new ActiveSkillActivationWave(0.5f, 0f, 1f, new AreaEffect(1f)));
            _executor.Schedule(Activation(level, enemy, 4f));

            _executor.Tick(0.25f, true);
            _executor.Tick(10f, false);
            Assert.AreEqual(10f, enemy.Health.CurrentHealth);
            _executor.Tick(0.25f, true);

            Assert.AreEqual(6f, enemy.Health.CurrentHealth);
        }

        [Test]
        public void Chain_RetargetsNearestUnhitEnemiesWithDamageFalloff()
        {
            var first = SpawnEnemy(new Vector2(1f, 0f));
            var second = SpawnEnemy(new Vector2(2f, 0f));
            var third = SpawnEnemy(new Vector2(3f, 0f));
            _executor = new SceneActiveSkillEffectExecutor(_runController, new RecordingLauncher());
            var level = Level(
                ActiveSkillTargetingMode.NearestEnemy,
                new ActiveSkillActivationWave(0f, 0f, 1f, new ChainEffect(3, 2f, 0.5f)));
            _executor.Schedule(Activation(level, first, 4f));

            _executor.Tick(0f, true);

            Assert.AreEqual(6f, first.Health.CurrentHealth);
            Assert.AreEqual(8f, second.Health.CurrentHealth);
            Assert.AreEqual(9f, third.Health.CurrentHealth);
        }

        [Test]
        public void Mines_EnforceConcurrentLimitAndLifetimeFreezesWhilePaused()
        {
            _executor = new SceneActiveSkillEffectExecutor(_runController, new RecordingLauncher());
            var level = Level(
                ActiveSkillTargetingMode.Self,
                new ActiveSkillActivationWave(0f, 0f, 1f, new MineEffect(0.5f, 1f, 1f, 2)));

            _executor.Schedule(Activation(level, null, 2f, new Vector2(0f, 0f)));
            _executor.Schedule(Activation(level, null, 2f, new Vector2(2f, 0f)));
            _executor.Schedule(Activation(level, null, 2f, new Vector2(4f, 0f)));
            _executor.Tick(0f, true);
            Assert.AreEqual(2, _executor.ActiveMineCount);

            _executor.Tick(10f, false);
            Assert.AreEqual(2, _executor.ActiveMineCount);
            _executor.Tick(1f, true);
            Assert.AreEqual(0, _executor.ActiveMineCount);
        }

        [Test]
        public void OrbitVisual_FollowsAuthoredBladeCountFreezesOnPauseAndReturnsToPool()
        {
            var sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f),
                new Vector2(.5f, .5f), 1f);
            try
            {
                var profile = new ProjectilePresentationProfile(1f, 0f, .1f, .1f,
                    Color.white, 3, .05f, .4f, Color.gray);
                var visual = new SpriteDefinition("FIXTURE-ORBIT-VISUAL", sprite,
                    SpriteRole.Projectile, projectilePresentation: profile);
                var registry = ContentRegistry.BuildFrom(new IContentDefinition[] { visual });
                _executor = new SceneActiveSkillEffectExecutor(
                    _runController, new RecordingLauncher(), contentRegistry: registry);
                var orbit = new OrbitEffect(2, 1f, 90f, .5f, .25f, bladeHitboxRadius: .2f);
                var level = new ActiveSkillLevelDefinition(1f, 1f, ActiveSkillTargetingMode.Self,
                    new ContentRef<SpriteDefinition>(visual.Id),
                    new ActiveSkillActivationWave(0f, 0f, 1f, orbit));
                _executor.Schedule(Activation(level, null, 1f));

                _executor.Tick(0f, true);
                Assert.AreEqual(2, _executor.ActiveOrbitBladeCount);
                var blade = _owner.GetComponentsInChildren<SpriteRenderer>()[0];
                var initialPosition = blade.transform.localPosition;
                _executor.Tick(10f, false);
                Assert.AreEqual(initialPosition, blade.transform.localPosition);
                _executor.Tick(.25f, true);
                Assert.AreNotEqual(initialPosition, blade.transform.localPosition);
                _executor.Tick(.25f, true);
                Assert.AreEqual(0, _executor.ActiveOrbitBladeCount);
            }
            finally
            {
                Object.DestroyImmediate(sprite);
            }
        }

        [Test]
        public void MultiWaveRing_SchedulesAndExecutesRotatedSecondActivation()
        {
            var launcher = new RecordingLauncher();
            _executor = new SceneActiveSkillEffectExecutor(_runController, launcher);
            var ring = new ProjectileBurstEffect(4, ProjectileLayout.Ring, 0f, 0, 5f, 1f, 0.1f);
            var level = Level(
                ActiveSkillTargetingMode.Self,
                new ActiveSkillActivationWave(0f, 0f, 1f, ring),
                new ActiveSkillActivationWave(0.25f, 45f, 1f, ring));
            _executor.Schedule(Activation(level, null, 2f));

            _executor.Tick(0f, true);
            Assert.AreEqual(4, launcher.Projectiles.Count);
            _executor.Tick(0.25f, true);
            Assert.AreEqual(8, launcher.Projectiles.Count);
            Assert.AreNotEqual(launcher.Projectiles[0].Direction, launcher.Projectiles[4].Direction);
        }

        private EnemyRuntime SpawnEnemy(Vector2 position)
        {
            var definition = new EnemyDefinition($"FIXTURE-ENEMY-{_enemies.Count}", 10f, 0.5f, 0f, 0f, 1f);
            var enemy = EnemyFactory.Spawn(definition, position, _owner.transform, _runController);
            _enemies.Add(enemy);
            Physics2D.SyncTransforms();
            return enemy;
        }

        private ActiveSkillActivation Activation(
            ActiveSkillLevelDefinition level,
            IEnemyDamageReceiver target,
            float damage,
            Vector2? origin = null)
        {
            var actualOrigin = origin ?? Vector2.zero;
            var direction = target != null ? target.Position - actualOrigin : Vector2.right;
            return new ActiveSkillActivation(
                "FIXTURE-SKILL-EXECUTOR",
                1,
                actualOrigin,
                direction,
                target,
                damage,
                level,
                _owner.transform);
        }

        private static ActiveSkillLevelDefinition Level(
            ActiveSkillTargetingMode targeting,
            params ActiveSkillActivationWave[] waves)
        {
            return new ActiveSkillLevelDefinition(1f, 1f, targeting, waves);
        }

        private sealed class RecordingLauncher : IActiveSkillProjectileLauncher
        {
            public List<ActiveSkillProjectile> Projectiles { get; } = new List<ActiveSkillProjectile>();
            public void Launch(ActiveSkillProjectile projectile) => Projectiles.Add(projectile);
        }
    }
}
