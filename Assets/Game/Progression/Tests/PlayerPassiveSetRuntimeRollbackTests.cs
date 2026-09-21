using System;
using System.Linq;
using System.Reflection;
using Game.Character;
using Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace Game.Progression.Tests
{
    public class PlayerPassiveSetRuntimeRollbackTests
    {
        private GameObject _playerObject;
        private GameObject _runObject;
        private PlayerCharacterRuntime _player;
        private RunController _runController;
        private PlayerExperienceRuntime _experience;
        private LevelUpDraftRuntime _draft;
        private PlayerPassiveSetRuntime _passives;

        [SetUp]
        public void SetUp()
        {
            _runObject = new GameObject("RunController");
            _runController = _runObject.AddComponent<RunController>();
            InvokeAwake(_runController);
            _runController.Model.Start();

            _playerObject = new GameObject("Player");
            _player = _playerObject.AddComponent<PlayerCharacterRuntime>();
            _player.Initialize(new CharacterBaseStats(100f, 3f), _runController);
            _experience = _playerObject.AddComponent<PlayerExperienceRuntime>();
            _experience.Initialize(_player, _runController, new ExperienceSettings(60f, 5f));

            var starting = new BuildEntryDefinition("FIXTURE-SKILL-BOLT", BuildEntryKind.ActiveSkill, "Bolt");
            _draft = _playerObject.AddComponent<LevelUpDraftRuntime>();
            _draft.Initialize(_experience, _runController, new[] { starting }, starting, 1);
            _passives = _playerObject.AddComponent<PlayerPassiveSetRuntime>();
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
        public void Initialize_WhenALaterBuildPassiveIsMissing_RemovesAlreadyAppliedModifiers()
        {
            var catalog = FixturePassiveCatalog.Create();
            // Known passive first (its modifier gets applied), then one the catalog does not have.
            _draft.Build.Apply(Passive(catalog[0].Id.ToString()));
            _draft.Build.Apply(Passive("FIXTURE-PASSIVE-UNKNOWN"));

            Assert.Throws<InvalidOperationException>(() => _passives.Initialize(_player, _draft, catalog));

            Assert.AreEqual(0, _passives.PassiveCount);
            Assert.AreEqual(0, _player.Stats.ModifierCount, "A failed Initialize must not leave modifiers applied.");
        }

        [Test]
        public void Initialize_AfterFailedAttempt_CanBeRetriedWithACompleteCatalog()
        {
            var catalog = FixturePassiveCatalog.Create();
            _draft.Build.Apply(Passive(catalog[0].Id.ToString()));

            // The catalog lacks the passive in the build, so the first attempt fails.
            Assert.Throws<InvalidOperationException>(() =>
                _passives.Initialize(_player, _draft, catalog.Skip(1).ToArray()));

            // Retrying on the same instance with the full catalog must not trip over
            // leftovers (e.g. duplicate ids) from the failed attempt.
            Assert.DoesNotThrow(() => _passives.Initialize(_player, _draft, catalog));
            Assert.AreEqual(1, _passives.PassiveCount);
        }

        [Test]
        public void Initialize_RepeatedAndAfterShutdown_ReplacesCatalogWithoutStacking()
        {
            var catalog = FixturePassiveCatalog.Create();
            _draft.Build.Apply(catalog[0]);
            _passives.Initialize(_player, _draft, catalog);
            _passives.Initialize(_player, _draft, catalog);
            Assert.AreEqual(110f, _player.Stats.MaxHealth, 0.001f);
            Assert.AreEqual(1, _player.Stats.ModifierCount);
            _passives.Shutdown();
            _passives.Shutdown();
            Assert.AreEqual(100f, _player.Stats.MaxHealth);
            _draft.Build.Apply(catalog[0]);
            _passives.Initialize(_player, _draft, catalog);
            Assert.AreEqual(120f, _player.Stats.MaxHealth, 0.001f);
            Assert.AreEqual(1, _player.Stats.ModifierCount);
        }

        private static BuildEntryDefinition Passive(string id) =>
            new BuildEntryDefinition(id, BuildEntryKind.PassiveItem, id);

        private static void InvokeAwake(object behaviour)
        {
            behaviour.GetType()
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(behaviour, null);
        }
    }
}
