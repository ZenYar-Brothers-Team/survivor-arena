using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Character;
using Game.Content;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Progression.Tests
{
    public sealed class DraftRequestTests
    {
        private GameObject _root;
        private RunController _run;
        private PlayerExperienceRuntime _xp;
        private LevelUpDraftRuntime _draft;
        private BuildEntryDefinition _active;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Draft request fixture");
            _run = _root.AddComponent<RunController>();
            typeof(RunController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_run, null);
            _run.Model.Start();
            var player = _root.AddComponent<PlayerCharacterRuntime>();
            player.Initialize(new CharacterBaseStats(100f, 3f), _run);
            _xp = _root.AddComponent<PlayerExperienceRuntime>();
            _xp.Initialize(player, _run, new ExperienceSettings(60f, 5f));
            _active = new BuildEntryDefinition("FIXTURE-QUEUE-ACTIVE", BuildEntryKind.ActiveSkill, "Fixture skill");
            _draft = _root.AddComponent<LevelUpDraftRuntime>();
            _draft.Initialize(_xp, _run, new[] { _active }, _active, 3,
                new SeededDraftRandom(13), 2, 2, emptyBookCurrency: 7);
        }

        [TearDown]
        public void TearDown()
        {
            _draft.Shutdown();
            _xp.Shutdown();
            Object.DestroyImmediate(_root);
        }

        private bool Book(Guid? pickup = null, Guid? run = null) => _draft.RequestBook(
            pickup ?? Guid.NewGuid(), run ?? _run.Model.RunId, new ContentId("FIXTURE-BOOK"));

        private void UpgradeTo(int level)
        {
            while (_draft.Build.TryGetEntry(_active.Id, out var entry) && entry.Level < level)
                _draft.Build.Apply(_active);
        }

        [Test]
        public void EmptyBook_AwardsImmediatelyOnce_OrdinaryEmptyLevelDoesNotAwardCurrency()
        {
            UpgradeTo(6);
            var pickup = Guid.NewGuid();
            DraftResolution resolution = null;
            _draft.RequestResolved += value => resolution = value;
            Assert.IsTrue(Book(pickup));
            Assert.AreEqual(7, _draft.BookCurrency);
            Assert.AreEqual(DraftResolutionKind.BookCurrency, resolution.Kind);
            Assert.AreEqual(pickup, resolution.Request.PickupId);
            Assert.AreEqual(1, _xp.Progression.Level);
            Assert.AreEqual(0f, _xp.Progression.CurrentExperience);
            Assert.AreEqual(0, _draft.PendingDraftCount);
            Assert.AreEqual(RunState.Running, _run.Model.State);
            Assert.IsFalse(Book(pickup));
            _xp.AddPickedUpExperience(10f);
            Assert.AreEqual(3, _xp.Progression.Level);
            Assert.AreEqual(7, _draft.BookCurrency);
            Assert.AreEqual(RunState.Running, _run.Model.State);
            Assert.IsTrue(Book());
            Assert.AreEqual(14, _draft.BookCurrency);
        }

        [Test]
        public void Requests_FollowAcceptedOrder_AndBookPreservesXpAndSharedControls()
        {
            _xp.AddPickedUpExperience(10f);
            var level = _xp.Progression.Level;
            Assert.IsTrue(Book());
            Assert.AreEqual(level, _xp.Progression.Level);
            _xp.AddPickedUpExperience(5f);
            Assert.AreEqual(4, _draft.PendingDraftCount);
            Assert.AreEqual(2, _draft.CurrentRequest.EarnedLevel);
            Assert.AreEqual(3, _draft.NextRequest.EarnedLevel);
            _draft.Select(_active.Id, _draft.Revision);
            Assert.AreEqual(3, _draft.CurrentRequest.EarnedLevel);
            _draft.Select(_active.Id, _draft.Revision);
            Assert.AreEqual(DraftOrigin.Book, _draft.CurrentRequest.Origin);
            Assert.AreEqual(2, _draft.RemainingRerolls);
            _draft.Reroll(_draft.Revision);
            Assert.AreEqual(1, _draft.RemainingRerolls);
            _draft.Select(_active.Id, _draft.Revision);
            Assert.AreEqual(4, _draft.CurrentRequest.EarnedLevel);
            Assert.AreEqual(1, _draft.RemainingRerolls);
            _draft.Select(_active.Id, _draft.Revision);
            Assert.AreEqual(RunState.Running, _run.Model.State);
            Assert.AreEqual(0, _draft.BookCurrency);
        }

        [Test]
        public void MultiLevelAward_EnqueuesWholeBatchBeforeDraftOpenedCallback()
        {
            var once = false;
            _draft.DraftOpened += _ =>
            {
                if (once) return;
                once = true;
                Assert.AreEqual(2, _draft.PendingDraftCount);
                Book();
            };
            _xp.AddPickedUpExperience(10f);
            _draft.Select(_active.Id);
            Assert.AreEqual(DraftOrigin.LevelUp, _draft.CurrentRequest.Origin);
            Assert.AreEqual(3, _draft.CurrentRequest.EarnedLevel);
            _draft.Select(_active.Id);
            Assert.AreEqual(DraftOrigin.Book, _draft.CurrentRequest.Origin);
        }

        [Test]
        public void StaleRevision_CannotSelectNextRequestOrSpendItsCounters()
        {
            _xp.AddPickedUpExperience(10f);
            var revision = _draft.Revision;
            var old = _draft.CurrentDraft;
            Assert.IsTrue(_draft.Select(_active.Id, revision));
            Assert.IsFalse(_draft.Select(_active.Id, revision));
            Assert.IsFalse(_draft.Reroll(revision));
            Assert.IsFalse(_draft.Banish(_active.Id, revision));
            Assert.IsFalse(old.TrySelect(_active.Id, out _));
            Assert.AreEqual(1, _draft.PendingDraftCount);
            Assert.AreEqual(2, _draft.RemainingRerolls);
            Assert.AreEqual(2, _draft.RemainingBanishes);
        }

        [Test]
        public void Reroll_CancelsOldSessionAndItsPreviewRemainsImmutable()
        {
            Book();
            var old = _draft.CurrentDraft;
            var preview = old.Options[0].Preview;
            Assert.IsTrue(_draft.Reroll(old.Revision));
            Assert.IsFalse(old.TrySelect(_active.Id, out _));
            Assert.IsFalse(_draft.Select(_active.Id, old.Revision));
            Assert.AreEqual(1, preview.CurrentLevel);
            Assert.AreEqual(2, preview.NextLevel);
            Assert.IsTrue(_draft.Build.TryGetEntry(_active.Id, out var entry));
            Assert.AreEqual(1, entry.Level);
        }

        [Test]
        public void PendingBook_BecomingEmptyAfterEarlierChoice_DoesNotAwardPickupCurrencyAgain()
        {
            UpgradeTo(5);
            _xp.AddPickedUpExperience(5f);
            Book();
            Assert.AreEqual(2, _draft.PendingDraftCount);
            _draft.Select(_active.Id);
            Assert.AreEqual(0, _draft.PendingDraftCount);
            Assert.AreEqual(0, _draft.BookCurrency);
            Assert.AreEqual(RunState.Running, _run.Model.State);
        }

        [Test]
        public void Banish_ExhaustingBookDoesNotCreateAnotherPickupReward()
        {
            Book();
            Assert.IsTrue(_draft.Banish(_active.Id, _draft.Revision));
            Assert.AreEqual(0, _draft.BookCurrency);
            Assert.IsFalse(_draft.IsDraftOpen);
            Assert.IsTrue(Book());
            Assert.AreEqual(7, _draft.BookCurrency, "A new pickup with the now empty pool is compensated.");
        }

        [Test]
        public void ManualPause_SurvivesLastSelectionAndShutdown()
        {
            _run.Model.Pause();
            Book(); // Already accepted reward; the world pickup itself cannot start while paused.
            _draft.Select(_active.Id);
            Assert.IsTrue(_run.Model.IsPausedBy(RunPauseReasons.Manual));
            Assert.IsFalse(_run.Model.IsPausedBy(RunPauseReasons.LevelUpDraft));
            Book();
            _draft.Shutdown();
            Assert.AreEqual(RunState.Paused, _run.Model.State);
            Assert.IsFalse(_run.Model.IsPausedBy(RunPauseReasons.LevelUpDraft));
        }

        [Test]
        public void Stop_CancelsOpenAndPendingChoices_AndCapturesEarnedBuild()
        {
            _xp.AddPickedUpExperience(10f);
            Book();
            var session = _draft.CurrentDraft;
            var resolved = new List<DraftResolution>();
            _draft.RequestResolved += resolved.Add;
            var outcome = _run.Model.Stop();
            Assert.AreEqual(3, resolved.Count);
            Assert.IsTrue(resolved.TrueForAll(x => x.Kind == DraftResolutionKind.Cancelled));
            Assert.AreEqual(0, _draft.PendingDraftCount);
            Assert.IsFalse(session.TrySelect(_active.Id, out _));
            Assert.IsFalse(Book());
            Assert.AreEqual(3, outcome.Contributions["draft"].DraftTotals.CancelledRequests);
            Assert.AreEqual(1, outcome.Contributions["draft"].Build[0].Level);
        }

        [Test]
        public void TerminalInsideSelectionCallback_PreservesAppliedChoiceAndCancelsRest()
        {
            _xp.AddPickedUpExperience(10f);
            _draft.SelectionApplied += _ => _run.Model.Stop();
            _draft.Select(_active.Id);
            var result = _run.Model.Outcome.Contributions["draft"];
            Assert.AreEqual(2, result.Build[0].Level);
            Assert.AreEqual(1, result.DraftTotals.Selections);
            Assert.AreEqual(1, result.DraftTotals.CancelledRequests);
            Assert.IsFalse(_draft.IsDraftOpen);
        }

        [Test]
        public void CurrencyCallback_CanFinishRunWithoutLosingTheImmediateAward()
        {
            UpgradeTo(6);
            _draft.RequestResolved += _ => _run.Model.Stop();
            Book();
            Assert.AreEqual(7, _run.Model.Outcome.Contributions["draft"].DraftTotals.BookCurrency);
            Assert.IsFalse(Book());
        }

        [Test]
        public void Book_WithOnlyFailedSetCheck_OpensSetOfferInsteadOfCurrency()
        {
            _draft.Shutdown();
            var set = new SetDefinition("FIXTURE-BOOK-SET", "Fixture set", 0f,
                new SetRecipeComponent(_active.Id, BuildEntryKind.ActiveSkill, 1));
            _draft.Initialize(_xp, _run, new BuildEntryDefinition[] { _active, set }, _active, 3,
                new SeededDraftRandom(13), setDefinitions: new[] { set },
                setAbilityFactory: new FixtureSetExtraAbilityFactory(), emptyBookCurrency: 7);
            UpgradeTo(6);
            var pickup = Guid.NewGuid();
            Assert.IsTrue(Book(pickup));
            Assert.IsFalse(Book(pickup));
            Assert.AreEqual(1, _draft.PendingDraftCount);
            Assert.AreEqual(set.Id, _draft.CurrentDraft.Options[0].Definition.Id);
            Assert.AreEqual(0, _draft.BookCurrency);
            _draft.Select(set.Id);
            Assert.IsFalse(Book(pickup));
            Assert.AreEqual(0, _draft.BookCurrency);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Controls_PreserveAllChecksOnBanish_RerollAndNextRequestCheckAgain(bool bookOrigin)
        {
            _draft.Shutdown();
            var provider = new RecordingSetOfferProvider { RejectAfterFirstCall = true };
            var sets = new List<SetDefinition>();
            var definitions = new List<BuildEntryDefinition> { _active,
                new BuildEntryDefinition("FIXTURE-P1", BuildEntryKind.PassiveItem, "P1"),
                new BuildEntryDefinition("FIXTURE-P2", BuildEntryKind.PassiveItem, "P2"),
                new BuildEntryDefinition("FIXTURE-P3", BuildEntryKind.PassiveItem, "P3") };
            foreach (var suffix in new[] { "D", "B", "C", "A" })
            {
                var set = new SetDefinition("FIXTURE-SET-" + suffix, suffix, 1f,
                    new SetRecipeComponent(_active.Id, BuildEntryKind.ActiveSkill, 1));
                sets.Add(set);
                definitions.Add(set);
            }
            _draft.Initialize(_xp, _run, definitions, _active, 3, new FixedDraftRandom(0f),
                2, 2, setDefinitions: sets, setAbilityFactory: new FixtureSetExtraAbilityFactory(),
                setOffers: provider, emptyBookCurrency: 7);
            if (bookOrigin) Book();
            else _xp.AddPickedUpExperience(5f);
            Book(); // Queue another request; it must not be checked until opened.
            _run.Model.Pause();
            Assert.AreEqual(1, provider.Calls);
            CollectionAssert.AreEqual(new[] { "FIXTURE-SET-A", "FIXTURE-SET-B", "FIXTURE-SET-C", "FIXTURE-SET-D" }, provider.LastOrder);
            var request = _draft.CurrentRequest;
            var old = _draft.CurrentDraft;
            Assert.IsTrue(_draft.Banish(new ContentId("FIXTURE-SET-B"), old.Revision));
            Assert.AreSame(request, _draft.CurrentRequest);
            Assert.AreEqual(1, provider.Calls, "Banish cannot recheck any set.");
            Assert.AreEqual(new ContentId("FIXTURE-SET-D"), _draft.CurrentDraft.Options[2].Definition.Id,
                "The successful overflow set must survive the initial three-slot limit.");
            Assert.IsFalse(_draft.Banish(new ContentId("FIXTURE-SET-A"), old.Revision));
            Assert.IsFalse(_draft.Reroll(old.Revision));
            Assert.IsFalse(old.TrySelect(new ContentId("FIXTURE-SET-A"), out _));
            Assert.AreEqual(1, _draft.RemainingBanishes);
            var revision = _draft.Revision;
            Assert.IsTrue(_draft.Reroll(revision));
            Assert.IsFalse(_draft.Reroll(revision));
            Assert.AreEqual(2, provider.Calls);
            Assert.AreEqual(1, _draft.RemainingRerolls);
            foreach (var option in _draft.CurrentDraft.Options)
                Assert.AreNotEqual(BuildEntryKind.Set, option.Definition.Kind);
            Assert.IsTrue(_draft.Select(_draft.CurrentDraft.Options[0].Definition.Id));
            Assert.AreEqual(3, provider.Calls, "Next queued request gets fresh checks.");
            Assert.IsTrue(_draft.Controls.IsBanished(new ContentId("FIXTURE-SET-B")));
            Assert.AreEqual(1, _draft.RemainingRerolls);
            Assert.IsTrue(_draft.Select(_draft.CurrentDraft.Options[0].Definition.Id));
            Assert.AreEqual(0, _draft.BookCurrency);
            Assert.AreEqual(0, _draft.PendingDraftCount);
            Assert.IsTrue(_run.Model.IsPausedBy(RunPauseReasons.Manual));
            Assert.IsFalse(_run.Model.IsPausedBy(RunPauseReasons.LevelUpDraft));
            _draft.Shutdown();
            _draft.Initialize(_xp, _run, definitions, _active, 3, new FixedDraftRandom(0f),
                2, 2, setDefinitions: sets, setAbilityFactory: new FixtureSetExtraAbilityFactory(),
                setOffers: provider, emptyBookCurrency: 7);
            Assert.IsFalse(_draft.Controls.IsBanished(new ContentId("FIXTURE-SET-B")));
            Assert.AreEqual(2, _draft.RemainingBanishes);
            Book();
            Assert.AreEqual(4, provider.Calls, "Reinitialize must discard check state.");
        }

        [Test]
        public void InvalidSourceAndOldRun_DoNotConsumePickupIdentity()
        {
            UpgradeTo(6);
            var pickup = Guid.NewGuid();
            Assert.IsFalse(Book(pickup, Guid.NewGuid()));
            Assert.IsFalse(_draft.RequestBook(pickup, _run.Model.RunId, default));
            Assert.IsTrue(Book(pickup));
            Assert.AreEqual(7, _draft.BookCurrency);
        }
    }
}
