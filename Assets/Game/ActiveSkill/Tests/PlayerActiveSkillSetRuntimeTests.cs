using System.Collections.Generic;
using Game.Character;
using Game.Enemy;
using Game.Progression;
using Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    public class PlayerActiveSkillSetRuntimeTests
    {
        private GameObject _playerObject;
        private GameObject _runObject;
        private PlayerCharacterRuntime _player;
        private RunController _runController;
        private PlayerExperienceRuntime _experience;
        private LevelUpDraftRuntime _draft;
        private PlayerActiveSkillSetRuntime _skillSet;
        private RecordingExecutor _executor;

        [SetUp]
        public void SetUp()
        {
            _runObject = new GameObject("RunController");
            _runController = _runObject.AddComponent<RunController>();
            TestLifecycle.InvokeAwake(_runController);
            _runController.Model.Start();

            _playerObject = new GameObject("Player");
            _player = _playerObject.AddComponent<PlayerCharacterRuntime>();
            _player.Initialize(new CharacterBaseStats(100f, 3f));
            _experience = _playerObject.AddComponent<PlayerExperienceRuntime>();
            _experience.Initialize(_player, _runController, 5f);

            var catalog = FixtureActiveSkillCatalog.Create();
            var starting = new BuildEntryDefinition("FIXTURE-SKILL-BOLT", BuildEntryKind.ActiveSkill, "Bolt");
            var second = new BuildEntryDefinition("FIXTURE-SKILL-RING", BuildEntryKind.ActiveSkill, "Ring");
            _draft = _playerObject.AddComponent<LevelUpDraftRuntime>();
            _draft.Initialize(_experience, _runController, new[] { starting, second }, starting, 2);

            _executor = new RecordingExecutor();
            _skillSet = _playerObject.AddComponent<PlayerActiveSkillSetRuntime>();
            _skillSet.Initialize(
                _player,
                _runController,
                _draft,
                catalog,
                new FixedTargetProvider(new FakeReceiver(Vector2.right)),
                _executor);
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
                Object.DestroyImmediate(_playerObject);
            if (_runObject != null)
                Object.DestroyImmediate(_runObject);
        }

        [Test]
        public void DraftAcquisitionAndUpgrade_SynchronizeConcurrentSkillInstances()
        {
            Assert.AreEqual(1, _skillSet.SkillCount);
            Assert.IsTrue(_skillSet.Tick(0f));

            _experience.AddPickedUpExperience(5f);
            Assert.IsTrue(_draft.Select("FIXTURE-SKILL-RING"));
            Assert.AreEqual(2, _skillSet.SkillCount);
            Assert.IsTrue(_skillSet.Tick(0f));
            Assert.IsTrue(_skillSet.TryGetSkill("FIXTURE-SKILL-RING", out var ring));
            Assert.AreEqual(1, ring.Level);

            _experience.AddPickedUpExperience(5f);
            Assert.IsTrue(_draft.Select("FIXTURE-SKILL-RING"));
            Assert.AreEqual(2, ring.Level);
            Assert.GreaterOrEqual(_executor.Activations.Count, 2);
        }

        [Test]
        public void Pause_StopsCooldownAndScheduledEffectTicks()
        {
            Assert.IsTrue(_skillSet.Tick(0f));
            var tickCalls = _executor.RunningTickCount;
            _runController.Model.Pause();

            Assert.IsFalse(_skillSet.Tick(100f));
            Assert.AreEqual(tickCalls, _executor.RunningTickCount);
        }

        private sealed class RecordingExecutor : IActiveSkillEffectExecutor
        {
            public List<ActiveSkillActivation> Activations { get; } = new List<ActiveSkillActivation>();
            public int RunningTickCount { get; private set; }
            public void Schedule(ActiveSkillActivation activation) => Activations.Add(activation);
            public void Tick(float deltaTime, bool isRunning)
            {
                if (isRunning)
                    RunningTickCount++;
            }
        }

        private sealed class FixedTargetProvider : IActiveSkillTargetProvider
        {
            private readonly IEnemyDamageReceiver _target;
            public FixedTargetProvider(IEnemyDamageReceiver target) => _target = target;
            public bool TryGetTarget(Vector2 origin, out IEnemyDamageReceiver target)
            {
                target = _target;
                return true;
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
