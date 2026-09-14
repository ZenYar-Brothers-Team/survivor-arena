using System.Reflection;
using Game.Character;
using Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace Game.Progression.Tests
{
    public class LevelUpDraftRuntimeTests
    {
        private GameObject _runObject;
        private GameObject _player;
        private RunController _runController;
        private PlayerExperienceRuntime _experience;
        private LevelUpDraftRuntime _draftRuntime;

        [SetUp]
        public void SetUp()
        {
            _runObject = new GameObject("RunController");
            _runController = _runObject.AddComponent<RunController>();
            InvokeAwake(_runController);
            _runController.Model.Start();

            _player = new GameObject("Player");
            var character = _player.AddComponent<PlayerCharacterRuntime>();
            InvokeAwake(character);
            _experience = _player.AddComponent<PlayerExperienceRuntime>();
            _experience.ConfigureForTests(character, _runController, 5f);
            _draftRuntime = _player.AddComponent<LevelUpDraftRuntime>();

            var starting = Active("FIXTURE-ACTIVE-START");
            _draftRuntime.Initialize(
                _experience,
                _runController,
                new[]
                {
                    starting,
                    Passive("FIXTURE-PASSIVE-ONE"),
                    Active("FIXTURE-ACTIVE-TWO")
                },
                starting,
                offerCount: 3);
        }

        [TearDown]
        public void TearDown()
        {
            if (_player != null)
                Object.DestroyImmediate(_player);
            if (_runObject != null)
                Object.DestroyImmediate(_runObject);
        }

        [Test]
        public void LevelUp_OpensValidDraftWhilePaused_AndChoiceResumesRun()
        {
            _experience.AddPickedUpExperience(5f);

            Assert.AreEqual(RunState.Paused, _runController.Model.State);
            Assert.IsTrue(_draftRuntime.IsDraftOpen);
            Assert.Greater(_draftRuntime.CurrentDraft.Options.Count, 0);
            var selected = _draftRuntime.CurrentDraft.Options[0];

            Assert.IsTrue(_draftRuntime.Select(selected.Definition.Id));
            Assert.AreEqual(RunState.Running, _runController.Model.State);
            Assert.IsFalse(_draftRuntime.IsDraftOpen);
            Assert.IsTrue(_draftRuntime.Build.TryGetEntry(selected.Definition.Id, out var entry));
            Assert.AreEqual(selected.ResultingLevel, entry.Level);
        }

        [Test]
        public void InvalidChoice_KeepsDraftOpenAndRunPaused()
        {
            _experience.AddPickedUpExperience(5f);

            Assert.IsFalse(_draftRuntime.Select("FIXTURE-NOT-OFFERED"));
            Assert.IsTrue(_draftRuntime.IsDraftOpen);
            Assert.AreEqual(RunState.Paused, _runController.Model.State);
        }

        [Test]
        public void MultipleLevelUps_QueueOneDraftPerLevel_AndResumeAfterLastChoice()
        {
            _experience.AddPickedUpExperience(10f);

            Assert.AreEqual(2, _draftRuntime.PendingDraftCount);
            Assert.IsTrue(_draftRuntime.Select(_draftRuntime.CurrentDraft.Options[0].Definition.Id));
            Assert.AreEqual(1, _draftRuntime.PendingDraftCount);
            Assert.IsTrue(_draftRuntime.IsDraftOpen);
            Assert.AreEqual(RunState.Paused, _runController.Model.State);

            Assert.IsTrue(_draftRuntime.Select(_draftRuntime.CurrentDraft.Options[0].Definition.Id));
            Assert.AreEqual(0, _draftRuntime.PendingDraftCount);
            Assert.AreEqual(RunState.Running, _runController.Model.State);
        }

        private static BuildEntryDefinition Active(string id)
        {
            return new BuildEntryDefinition(id, BuildEntryKind.ActiveSkill, id);
        }

        private static BuildEntryDefinition Passive(string id)
        {
            return new BuildEntryDefinition(id, BuildEntryKind.PassiveItem, id);
        }

        private static void InvokeAwake(MonoBehaviour behaviour)
        {
            behaviour.GetType()
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(behaviour, null);
        }
    }
}
