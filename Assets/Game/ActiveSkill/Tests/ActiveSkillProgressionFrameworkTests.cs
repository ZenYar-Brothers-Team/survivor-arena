using System;
using System.Collections.Generic;
using System.Linq;
using Game.Character;
using Game.Enemy;
using Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    public class ActiveSkillProgressionFrameworkTests
    {
        private GameObject _runObject;
        private RunController _runController;
        private GameObject _playerObject;
        private PlayerCharacterRuntime _player;

        [SetUp]
        public void SetUp()
        {
            _runObject = new GameObject("RunController");
            _runController = _runObject.AddComponent<RunController>();
            TestLifecycle.InvokeAwake(_runController);
            _runController.Model.Start();

            _playerObject = new GameObject("Player");
            _player = _playerObject.AddComponent<PlayerCharacterRuntime>();
            _player.Initialize(new CharacterBaseStats(100f, 3f), _runController);
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
                UnityEngine.Object.DestroyImmediate(_playerObject);
            if (_runObject != null)
                UnityEngine.Object.DestroyImmediate(_runObject);
        }

        [Test]
        public void Definition_RequiresExactlySixLevels_AndExposesLevelConfiguration()
        {
            var level = CreateLevel(1f, 2f, new AreaEffect(1f));

            Assert.Throws<ArgumentException>(() =>
                new ActiveSkillProgressionDefinition("FIXTURE-SKILL", "Fixture", level));

            var definition = new ActiveSkillProgressionDefinition(
                "FIXTURE-SKILL",
                "Fixture",
                level, level, level, level, level, level);

            Assert.AreEqual(6, definition.Levels.Count);
            Assert.AreSame(level, definition.GetLevel(6));
            Assert.Throws<ArgumentOutOfRangeException>(() => definition.GetLevel(7));
        }

        [Test]
        public void FixtureCatalog_CoversCompatibilityPatternsWithoutProductionIds()
        {
            var catalog = FixtureActiveSkillCatalog.Create();
            var effects = catalog
                .SelectMany(skill => skill.Levels)
                .SelectMany(level => level.Waves)
                .SelectMany(wave => wave.Effects)
                .ToList();
            var projectileEffects = effects.OfType<ProjectileBurstEffect>().ToList();

            Assert.IsTrue(catalog.All(skill => skill.Id.ToString().StartsWith("FIXTURE-")));
            Assert.IsTrue(catalog.All(skill => skill.Levels.Count == 6));
            Assert.IsTrue(projectileEffects.Any(effect => effect.Layout == ProjectileLayout.Fan));
            Assert.IsTrue(projectileEffects.Any(effect => effect.Layout == ProjectileLayout.Ring));
            Assert.IsTrue(projectileEffects.Any(effect => effect.Layout == ProjectileLayout.Cross));
            Assert.IsTrue(projectileEffects.Any(effect => effect.PierceCount > 0));
            Assert.IsTrue(effects.Any(effect => effect is BeamEffect));
            Assert.IsTrue(effects.Any(effect => effect is OrbitEffect));
            Assert.IsTrue(effects.Any(effect => effect is BoomerangEffect));
            Assert.IsTrue(effects.Any(effect => effect is ChainEffect));
            Assert.IsTrue(effects.Any(effect => effect is AreaEffect));
            Assert.IsTrue(effects.Any(effect => effect is MineEffect));
            Assert.IsTrue(catalog.SelectMany(skill => skill.Levels).Any(level => level.Waves.Count > 1));
        }

        [Test]
        public void Instance_LevelChangeCanAlterNumbersAndQualitativePattern()
        {
            var definition = FixtureActiveSkillCatalog.Create().Single(skill => skill.Id.ToString() == "FIXTURE-SKILL-BOLT");
            var instance = new ActiveSkillInstance(definition);
            var executor = new RecordingEffectExecutor();
            var target = new FakeReceiver(Vector2.right);

            Assert.IsTrue(instance.Tick(0f, true, _player, new FixedTargetProvider(target), executor));
            var levelOne = executor.LastActivation.LevelDefinition;
            instance.SetLevel(6);
            Assert.IsTrue(instance.Tick(levelOne.CooldownSeconds, true, _player, new FixedTargetProvider(target), executor));
            var levelSix = executor.LastActivation.LevelDefinition;

            var firstEffect = (ProjectileBurstEffect)levelOne.Waves[0].Effects[0];
            var finalEffect = (ProjectileBurstEffect)levelSix.Waves[0].Effects[0];
            Assert.Greater(levelSix.BaseDamage, levelOne.BaseDamage);
            Assert.Greater(finalEffect.ProjectileCount, firstEffect.ProjectileCount);
            Assert.Greater(finalEffect.PierceCount, firstEffect.PierceCount);
            Assert.Throws<ArgumentOutOfRangeException>(() => instance.SetLevel(5));
            Assert.AreEqual(2, instance.TriggerCount);
        }

        [Test]
        public void DirectionGenerator_CreatesFanRingAndRotatedCross()
        {
            var fan = ProjectileDirectionGenerator.Create(ProjectileLayout.Fan, 3, Vector2.right, 60f);
            var ring = ProjectileDirectionGenerator.Create(ProjectileLayout.Ring, 4, Vector2.right);
            var cross = ProjectileDirectionGenerator.Create(ProjectileLayout.Cross, 4, Vector2.right, rotationDegrees: 45f);

            Assert.AreEqual(-30f, Vector2.SignedAngle(Vector2.right, fan[0]), 0.01f);
            Assert.AreEqual(30f, Vector2.SignedAngle(Vector2.right, fan[2]), 0.01f);
            Assert.AreEqual(90f, Vector2.Angle(ring[0], ring[1]), 0.01f);
            Assert.AreEqual(45f, Vector2.SignedAngle(Vector2.right, cross[0]), 0.01f);
        }

        private static ActiveSkillLevelDefinition CreateLevel(float damage, float cooldown, IActiveSkillEffect effect)
        {
            return new ActiveSkillLevelDefinition(
                damage,
                cooldown,
                ActiveSkillTargetingMode.Self,
                new ActiveSkillActivationWave(0f, 0f, 1f, effect));
        }

        private sealed class RecordingEffectExecutor : IActiveSkillEffectExecutor
        {
            public ActiveSkillActivation LastActivation { get; private set; }

            public void Schedule(ActiveSkillActivation activation) => LastActivation = activation;
            public void Tick(float deltaTime, bool isRunning) { }
        }

        private sealed class FixedTargetProvider : IActiveSkillTargetProvider
        {
            private readonly IEnemyDamageReceiver _target;
            public FixedTargetProvider(IEnemyDamageReceiver target) => _target = target;
            public bool TryGetTarget(Vector2 origin, out IEnemyDamageReceiver target)
            {
                target = _target;
                return target != null;
            }
        }

        private sealed class FakeReceiver : IEnemyDamageReceiver
        {
            public bool IsAlive => true;
            public Vector2 Position { get; }
            public FakeReceiver(Vector2 position) => Position = position;
            public float ApplyDamage(EnemyDamageRequest request) => request.Amount;
        }
    }
}
