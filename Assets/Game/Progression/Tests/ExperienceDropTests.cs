using Game.Character;
using Game.Run;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;

namespace Game.Progression.Tests
{
    public class ExperienceDropTests
    {
        private GameObject _runObject;
        private GameObject _player;
        private RunController _runController;
        private PlayerCharacterRuntime _character;
        private PlayerExperienceRuntime _experience;
        private ExperienceDropRuntime _drop;

        [SetUp]
        public void SetUp()
        {
            _runObject = new GameObject("RunController");
            _runController = _runObject.AddComponent<RunController>();
            InvokeAwake(_runController);
            _runController.Model.Start();

            _player = new GameObject("Player");
            _character = _player.AddComponent<PlayerCharacterRuntime>();
            InvokeAwake(_character);
            _experience = _player.AddComponent<PlayerExperienceRuntime>();
            _experience.ConfigureForTests(_character, _runController, 100f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_drop != null)
                Object.DestroyImmediate(_drop.gameObject);
            if (_player != null)
                Object.DestroyImmediate(_player);
            if (_runObject != null)
                Object.DestroyImmediate(_runObject);
        }

        [Test]
        public void Pickup_AwardsExperienceOnceAndConsumesDrop()
        {
            _drop = ExperienceDropFactory.Spawn(4f, Vector2.one, 60f, _experience, _runController);

            Assert.IsTrue(_drop.TryPickup());
            Assert.AreEqual(4f, _experience.Progression.CurrentExperience);
            Assert.IsTrue(_drop == null);
        }

        [Test]
        public void Expiry_WithBaseZeroRecovery_DoesNotAwardExperience()
        {
            _drop = ExperienceDropFactory.Spawn(4f, Vector2.zero, 2f, _experience, _runController);

            Assert.IsTrue(_drop.TickForTests(2f));
            Assert.AreEqual(0f, _experience.Progression.CurrentExperience);
            Assert.IsTrue(_drop == null);
        }

        [Test]
        public void Expiry_UsesRecoveryHookWithoutDoubleAwarding()
        {
            _character.SetModifier("test-recovery", new CharacterStatModifier(disappearingXpRecoveryBonus: 0.5f));
            _drop = ExperienceDropFactory.Spawn(8f, Vector2.zero, 1f, _experience, _runController);

            _drop.TickForTests(1f);

            Assert.AreEqual(4f, _experience.Progression.CurrentExperience);
        }

        [Test]
        public void Pause_FreezesExpiryAndBlocksPickup()
        {
            _drop = ExperienceDropFactory.Spawn(4f, Vector2.zero, 1f, _experience, _runController);
            _runController.Model.Pause();

            Assert.IsFalse(_drop.TickForTests(10f));
            Assert.IsFalse(_drop.TryPickup());
            Assert.AreEqual(0f, _experience.Progression.CurrentExperience);

            _runController.Model.Resume();
            Assert.IsTrue(_drop.TickForTests(1f));
        }

        private static void InvokeAwake(MonoBehaviour behaviour)
        {
            behaviour.GetType()
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(behaviour, null);
        }
    }
}
