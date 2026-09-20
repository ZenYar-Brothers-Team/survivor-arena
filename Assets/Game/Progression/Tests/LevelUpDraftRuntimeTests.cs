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
            character.Initialize(new CharacterBaseStats(100f, 3f), _runController);
            _experience = _player.AddComponent<PlayerExperienceRuntime>();
            _experience.Initialize(character, _runController, new ExperienceSettings(60f, 5f));
            _draftRuntime = _player.AddComponent<LevelUpDraftRuntime>();

            var starting = Active("FIXTURE-ACTIVE-START");
            _draftRuntime.Initialize(
                _experience,
                _runController,
                new[]
                {
                    starting,
                    Passive("FIXTURE-PASSIVE-ONE"),
                    Active("FIXTURE-ACTIVE-TWO"),
                    Passive("FIXTURE-PASSIVE-TWO")
                },
                starting,
                offerCount: 3,
                draftRandom: new SeededDraftRandom(123),
                initialRerolls: 2,
                initialBanishes: 2);
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

        [Test]
        public void Reroll_ReplacesOpenDraftAndStopsAtConfiguredCounter()
        {
            _experience.AddPickedUpExperience(5f);
            var firstSession = _draftRuntime.CurrentDraft;
            var firstIds = new System.Collections.Generic.HashSet<Game.Content.ContentId>();
            foreach (var option in firstSession.Options)
                firstIds.Add(option.Definition.Id);

            Assert.IsTrue(_draftRuntime.Reroll());
            Assert.AreNotSame(firstSession, _draftRuntime.CurrentDraft);
            var changed = false;
            foreach (var option in _draftRuntime.CurrentDraft.Options)
                changed |= !firstIds.Contains(option.Definition.Id);
            Assert.IsTrue(changed, "Reroll must change the offer set when an eligible alternative exists.");
            Assert.AreEqual(1, _draftRuntime.RemainingRerolls);
            Assert.IsTrue(_draftRuntime.IsDraftOpen);
            Assert.AreEqual(RunState.Paused, _runController.Model.State);

            Assert.IsTrue(_draftRuntime.Reroll());
            Assert.IsFalse(_draftRuntime.Reroll());
            Assert.AreEqual(0, _draftRuntime.RemainingRerolls);
        }

        [Test]
        public void Banish_RemovesOfferedEntryWithoutClosingDraftAndPersistsForRun()
        {
            _experience.AddPickedUpExperience(5f);
            var banishedId = _draftRuntime.CurrentDraft.Options[0].Definition.Id;

            Assert.IsTrue(_draftRuntime.Banish(banishedId));
            Assert.IsTrue(_draftRuntime.Controls.IsBanished(banishedId));
            Assert.AreEqual(1, _draftRuntime.RemainingBanishes);
            Assert.IsTrue(_draftRuntime.IsDraftOpen);
            Assert.AreEqual(RunState.Paused, _runController.Model.State);
            foreach (var option in _draftRuntime.CurrentDraft.Options)
                Assert.AreNotEqual(banishedId, option.Definition.Id);

            Assert.IsFalse(_draftRuntime.Banish("FIXTURE-NOT-OFFERED"));
            Assert.AreEqual(1, _draftRuntime.RemainingBanishes);
        }

        [Test]
        public void Controls_ResetOnlyAfterPendingDraftIsResolved()
        {
            _experience.AddPickedUpExperience(5f);
            Assert.IsTrue(_draftRuntime.Reroll());
            Assert.Throws<System.InvalidOperationException>(() => _draftRuntime.ResetControlsForNewRun());

            Assert.IsTrue(_draftRuntime.Select(_draftRuntime.CurrentDraft.Options[0].Definition.Id));
            _draftRuntime.ResetControlsForNewRun();

            Assert.AreEqual(2, _draftRuntime.RemainingRerolls);
            Assert.AreEqual(2, _draftRuntime.RemainingBanishes);
        }

        [Test]
        public void BanishingLastEligibleEntry_ResolvesDraftWithoutPermanentPause()
        {
            var isolatedPlayer = new GameObject("Isolated Player");
            try
            {
                var character = isolatedPlayer.AddComponent<PlayerCharacterRuntime>();
                character.Initialize(new CharacterBaseStats(100f, 3f), _runController);
                var experience = isolatedPlayer.AddComponent<PlayerExperienceRuntime>();
                experience.Initialize(character, _runController, new ExperienceSettings(60f, 1f));
                var draft = isolatedPlayer.AddComponent<LevelUpDraftRuntime>();
                var onlyEntry = Active("FIXTURE-ONLY-ENTRY");
                draft.Initialize(
                    experience,
                    _runController,
                    new[] { onlyEntry },
                    onlyEntry,
                    offerCount: 3,
                    initialBanishes: 1);

                experience.AddPickedUpExperience(1f);
                Assert.IsTrue(draft.Banish(onlyEntry.Id));

                Assert.IsFalse(draft.IsDraftOpen);
                Assert.AreEqual(0, draft.PendingDraftCount);
                Assert.AreEqual(RunState.Running, _runController.Model.State);

                experience.AddPickedUpExperience(1f);

                Assert.AreEqual(3, experience.Progression.Level);
                Assert.IsFalse(draft.IsDraftOpen);
                Assert.AreEqual(0, draft.PendingDraftCount);
                Assert.AreEqual(RunState.Running, _runController.Model.State);
            }
            finally
            {
                Object.DestroyImmediate(isolatedPlayer);
            }
        }

        [Test]
        public void RerollWithNoReplacement_ConsumesDraftWithoutThrowingOrLeavingPause()
        {
            var isolatedPlayer = new GameObject("Reroll Exhaustion Player");
            try
            {
                var character = isolatedPlayer.AddComponent<PlayerCharacterRuntime>();
                character.Initialize(new CharacterBaseStats(100f, 3f), _runController);
                var experience = isolatedPlayer.AddComponent<PlayerExperienceRuntime>();
                experience.Initialize(character, _runController, new ExperienceSettings(60f, 1f));
                var draft = isolatedPlayer.AddComponent<LevelUpDraftRuntime>();
                var active = Active("FIXTURE-REROLL-STARTING-ACTIVE");
                var set = new SetDefinition(
                    "FIXTURE-REROLL-SET",
                    "Fixture Reroll Set",
                    0.5f,
                    new SetRecipeComponent(active.Id, BuildEntryKind.ActiveSkill, 1));
                draft.Initialize(
                    experience,
                    _runController,
                    new BuildEntryDefinition[] { active, set },
                    active,
                    offerCount: 3,
                    draftRandom: new SequenceDraftRandom(0f, 1f),
                    initialRerolls: 1);
                UpgradeToMaximum(draft.Build, active);

                experience.AddPickedUpExperience(1f);
                Assert.IsTrue(draft.IsDraftOpen);
                Assert.AreEqual(set.Id, draft.CurrentDraft.Options[0].Definition.Id);

                Assert.DoesNotThrow(() => Assert.IsTrue(draft.Reroll()));
                Assert.AreEqual(0, draft.RemainingRerolls);
                Assert.IsFalse(draft.IsDraftOpen);
                Assert.AreEqual(0, draft.PendingDraftCount);
                Assert.AreEqual(RunState.Running, _runController.Model.State);
            }
            finally
            {
                Object.DestroyImmediate(isolatedPlayer);
            }
        }

        [Test]
        public void LevelUpWithFullMaxedBuild_SkipsEveryUnavailableDraftButKeepsLevels()
        {
            var isolatedPlayer = new GameObject("Full Build Player");
            try
            {
                var character = isolatedPlayer.AddComponent<PlayerCharacterRuntime>();
                character.Initialize(new CharacterBaseStats(100f, 3f), _runController);
                var experience = isolatedPlayer.AddComponent<PlayerExperienceRuntime>();
                experience.Initialize(character, _runController, new ExperienceSettings(60f, 1f));
                var draft = isolatedPlayer.AddComponent<LevelUpDraftRuntime>();
                var definitions = new BuildEntryDefinition[]
                {
                    Active("FIXTURE-FULL-ACTIVE-1"),
                    Active("FIXTURE-FULL-ACTIVE-2"),
                    Active("FIXTURE-FULL-ACTIVE-3"),
                    Active("FIXTURE-FULL-ACTIVE-4"),
                    Active("FIXTURE-FULL-ACTIVE-5"),
                    Active("FIXTURE-FULL-ACTIVE-6"),
                    Passive("FIXTURE-FULL-PASSIVE-1"),
                    Passive("FIXTURE-FULL-PASSIVE-2"),
                    Passive("FIXTURE-FULL-PASSIVE-3"),
                    Passive("FIXTURE-FULL-PASSIVE-4"),
                    Passive("FIXTURE-FULL-PASSIVE-5"),
                    Passive("FIXTURE-FULL-PASSIVE-6")
                };
                draft.Initialize(experience, _runController, definitions, definitions[0], offerCount: 3);
                for (var i = 0; i < definitions.Length; i++)
                {
                    if (!draft.Build.TryGetEntry(definitions[i].Id, out _))
                        draft.Build.Apply(definitions[i]);
                    UpgradeToMaximum(draft.Build, definitions[i]);
                }

                Assert.AreEqual(PlayerBuild.ActiveSlotCapacity, draft.Build.ActiveCount);
                Assert.AreEqual(PlayerBuild.PassiveSlotCapacity, draft.Build.PassiveCount);

                experience.AddPickedUpExperience(2f);

                Assert.AreEqual(3, experience.Progression.Level);
                Assert.IsFalse(draft.IsDraftOpen);
                Assert.AreEqual(0, draft.PendingDraftCount);
                Assert.AreEqual(RunState.Running, _runController.Model.State);
            }
            finally
            {
                Object.DestroyImmediate(isolatedPlayer);
            }
        }

        [Test]
        public void SelectingSet_CreatesExtraAbilityWithoutOccupyingActiveOrPassiveSlot()
        {
            var isolatedPlayer = new GameObject("Set Player");
            try
            {
                var character = isolatedPlayer.AddComponent<PlayerCharacterRuntime>();
                character.Initialize(new CharacterBaseStats(100f, 3f), _runController);
                var experience = isolatedPlayer.AddComponent<PlayerExperienceRuntime>();
                experience.Initialize(character, _runController, new ExperienceSettings(60f, 1f));
                var draft = isolatedPlayer.AddComponent<LevelUpDraftRuntime>();
                var active = Active("FIXTURE-SET-STARTING-ACTIVE");
                var set = new SetDefinition(
                    "FIXTURE-SELECTABLE-SET",
                    "Fixture Selectable Set",
                    1f,
                    new SetRecipeComponent(active.Id, BuildEntryKind.ActiveSkill, 1));
                draft.Initialize(
                    experience,
                    _runController,
                    new BuildEntryDefinition[] { active, set },
                    active,
                    offerCount: 3,
                    setDefinitions: new[] { set },
                    setAbilityFactory: new FixtureSetExtraAbilityFactory());

                experience.AddPickedUpExperience(1f);
                Assert.IsTrue(draft.Select(set.Id));

                Assert.AreEqual(1, draft.Build.ActiveCount);
                Assert.AreEqual(0, draft.Build.PassiveCount);
                Assert.AreEqual(1, draft.Build.SetCount);
                Assert.AreEqual(1, draft.Sets.Count);
            }
            finally
            {
                Object.DestroyImmediate(isolatedPlayer);
            }
        }

        [Test]
        public void Shutdown_ClearsOpenDraftAndPendingState_SoReinitializeStartsClean()
        {
            _experience.AddPickedUpExperience(5f);
            Assert.IsTrue(_draftRuntime.IsDraftOpen);
            Assert.AreEqual(1, _draftRuntime.PendingDraftCount);

            _draftRuntime.Shutdown();

            Assert.IsFalse(_draftRuntime.IsDraftOpen);
            Assert.AreEqual(0, _draftRuntime.PendingDraftCount);
            Assert.IsNull(_draftRuntime.Build);
            Assert.AreEqual(0, _draftRuntime.RemainingRerolls);

            var starting = Active("FIXTURE-ACTIVE-START");
            _draftRuntime.Initialize(
                _experience,
                _runController,
                new[] { starting, Passive("FIXTURE-PASSIVE-ONE") },
                starting,
                offerCount: 2,
                draftRandom: new SeededDraftRandom(1),
                initialRerolls: 1,
                initialBanishes: 1);

            Assert.IsFalse(_draftRuntime.IsDraftOpen);
            Assert.AreEqual(0, _draftRuntime.PendingDraftCount);
            Assert.AreEqual(1, _draftRuntime.RemainingRerolls);
            Assert.IsNotNull(_draftRuntime.Build);
        }

        private static BuildEntryDefinition Active(string id)
        {
            return new BuildEntryDefinition(id, BuildEntryKind.ActiveSkill, id);
        }

        private static BuildEntryDefinition Passive(string id)
        {
            return new BuildEntryDefinition(id, BuildEntryKind.PassiveItem, id);
        }

        private static void UpgradeToMaximum(PlayerBuild build, BuildEntryDefinition definition)
        {
            while (build.TryGetEntry(definition.Id, out var entry) && !entry.IsMaxLevel)
                build.Apply(definition);
        }

        private static void InvokeAwake(MonoBehaviour behaviour)
        {
            behaviour.GetType()
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(behaviour, null);
        }
    }
}
