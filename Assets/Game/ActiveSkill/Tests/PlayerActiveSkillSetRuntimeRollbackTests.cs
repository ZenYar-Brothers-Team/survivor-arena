using System;
using System.Linq;
using Game.Character;
using Game.Enemy;
using Game.Progression;
using Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    public class PlayerActiveSkillSetRuntimeRollbackTests
    {
        private const string StartingId = "FIXTURE-SKILL-BOLT";
        private const string SecondId = "FIXTURE-SKILL-RING";

        private GameObject _playerObject;
        private GameObject _runObject;
        private PlayerCharacterRuntime _player;
        private RunController _runController;
        private PlayerExperienceRuntime _experience;
        private LevelUpDraftRuntime _draft;
        private PlayerActiveSkillSetRuntime _skillSet;
        private BuildEntryDefinition _starting;
        private BuildEntryDefinition _second;

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
            _experience = _playerObject.AddComponent<PlayerExperienceRuntime>();
            _experience.Initialize(_player, _runController, new ExperienceSettings(60f, 5f));

            _starting = new BuildEntryDefinition(StartingId, BuildEntryKind.ActiveSkill, "Bolt");
            _second = new BuildEntryDefinition(SecondId, BuildEntryKind.ActiveSkill, "Ring");
            _draft = _playerObject.AddComponent<LevelUpDraftRuntime>();
            _draft.Initialize(_experience, _runController, new[] { _starting, _second }, _starting, 2);
            _skillSet = _playerObject.AddComponent<PlayerActiveSkillSetRuntime>();
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
        public void Initialize_WhenBuildEntryMissingFromCatalog_ThrowsWithoutLeavingSubscriptionOrState()
        {
            // The build already holds the starting skill, but the catalog only knows the second one.
            var incompleteCatalog = FixtureActiveSkillCatalog.Create()
                .Where(definition => definition.Id.ToString() == SecondId)
                .ToArray();

            Assert.Throws<InvalidOperationException>(() => _skillSet.Initialize(
                _player,
                _runController,
                _draft,
                incompleteCatalog,
                new NoTargetProvider(),
                new NoOpExecutor()));

            // A later draft selection must not reach the half-initialized runtime.
            _experience.AddPickedUpExperience(5f);
            Assert.IsTrue(_draft.Select(SecondId));
            Assert.AreEqual(0, _skillSet.SkillCount, "Failed Initialize must not stay subscribed to SelectionApplied.");
        }

        [Test]
        public void Initialize_AfterFailedAttempt_CanBeRetriedWithACompleteCatalog()
        {
            var incompleteCatalog = FixtureActiveSkillCatalog.Create()
                .Where(definition => definition.Id.ToString() == SecondId)
                .ToArray();
            Assert.Throws<InvalidOperationException>(() => _skillSet.Initialize(
                _player,
                _runController,
                _draft,
                incompleteCatalog,
                new NoTargetProvider(),
                new NoOpExecutor()));

            Assert.DoesNotThrow(() => _skillSet.Initialize(
                _player,
                _runController,
                _draft,
                FixtureActiveSkillCatalog.Create(),
                new NoTargetProvider(),
                new NoOpExecutor()));

            Assert.AreEqual(1, _skillSet.SkillCount);
        }

        private sealed class NoTargetProvider : IActiveSkillTargetProvider
        {
            public bool TryGetTarget(Vector2 origin, out IEnemyDamageReceiver target)
            {
                target = null;
                return false;
            }
        }

        private sealed class NoOpExecutor : IActiveSkillEffectExecutor
        {
            public void Schedule(ActiveSkillActivation activation)
            {
            }

            public void Tick(float deltaTime, bool isRunning)
            {
            }
        }
    }
}
