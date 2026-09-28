using System.Linq;
using System.Threading.Tasks;
using Game.Content;
using Game.Meta;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Tests
{
    public sealed class RunResultsTests
    {
        private static RunModel Run()
        {
            var run = new RunModel(900);
            run.ConfigureSelection(new RunSelectionSnapshot("CHAR-001", "FIELD-001", "FIXTURE-ENV", "FIXTURE-WAVE"));
            run.RegisterOutcomeContributor(new ResultsTestContributor("experience", new RunOutcomeContribution(level: 20)));
            run.RegisterOutcomeContributor(new ResultsTestContributor("draft", new RunOutcomeContribution(
                draftTotals: new RunDraftSnapshot(3, 2, 1, 0, 90), sets: new[] { new RunBuildEntrySnapshot("SET-001", 1) })));
            run.RegisterOutcomeContributor(new ResultsTestContributor("ordinary-enemy-kills", new RunOutcomeContribution(kills: 42)));
            run.Start(); run.Tick(900);
            return run;
        }

        [Test]
        public async Task Results_SavedReceiptNamesKindsAndRetry_AreProjectedWithoutWalletMixing()
        {
            var profile = new ProfileService(MetaCatalog.Load(), new MemoryProfileStore()); await profile.LoadAsync();
            var run = Run(); await profile.ApplyAsync(run.Outcome, true);
            var view = new FakeMetaView(); var navigation = new FakeProfileNavigation();
            using var presenter = new MetaPresenter(profile, view, navigation);
            presenter.ShowResult(run.Outcome);
            var result = view.State.Result;
            Assert.AreEqual(190, result.Total); Assert.AreEqual(90, result.BookReward);
            Assert.AreEqual(42, result.Kills); Assert.AreEqual(20, result.Level);
            Assert.AreEqual("Победа", result.Outcome);
            Assert.IsTrue(result.Unlocks.Any(c => c.Kind == "Карта"));
            Assert.IsTrue(result.Unlocks.Any(c => c.Kind == "Умение"));
            Assert.IsTrue(result.Unlocks.Any(c => c.Kind == "Сет"));
            Assert.AreEqual("Персонаж", RunResultsProjection.Content("CHAR-002", profile.Catalog).Kind);
            Assert.AreEqual("Тяжёлый боезапас", result.Sets.Single().Name);
            view.Retry(); Assert.AreEqual(1, navigation.Retries);
            var next = Run(); presenter.ShowResult(next.Outcome);
            Assert.IsNull(view.State.Result.Total, "Prior run receipt must never appear on a new result.");
            Assert.IsEmpty(view.State.Result.Unlocks);
            Assert.IsNotEmpty(view.State.Result.SaveStatus);
        }

        [Test]
        public void ResultsAsset_HasSemanticElementsAndUnlocksBeforeSets()
        {
            var root = Resources.Load<VisualTreeAsset>("UI/RunResults").CloneTree();
            foreach (var id in new[] { GameplayUiElementIds.ResultsOutcome, GameplayUiElementIds.ResultsSelection,
                GameplayUiElementIds.ResultsTime, GameplayUiElementIds.ResultsLevel, GameplayUiElementIds.ResultsKills,
                GameplayUiElementIds.ResultsRewardCaption, GameplayUiElementIds.ResultsTotal, GameplayUiElementIds.ResultsLevelReward,
                GameplayUiElementIds.ResultsBookReward, GameplayUiElementIds.ResultsCollection, GameplayUiElementIds.ResultsUnlocksGroup,
                GameplayUiElementIds.ResultsUnlocks, GameplayUiElementIds.ResultsSets, GameplayUiElementIds.ResultsEmpty })
                Assert.IsNotNull(root.Q(id), id);
            var collection = root.Q<ScrollView>(GameplayUiElementIds.ResultsCollection).contentContainer;
            Assert.Less(collection.IndexOf(root.Q(GameplayUiElementIds.ResultsUnlocksGroup)), collection.IndexOf(root.Q(GameplayUiElementIds.ResultsSets)));
            Assert.IsNotNull(Resources.Load<StyleSheet>("UI/RunResultsStyles"));
        }

        [TestCase(RunCompletionReason.Aborted, "Забег прерван")]
        [TestCase(RunCompletionReason.Error, "Забег остановлен")]
        [TestCase(RunCompletionReason.Defeat, "Поражение")]
        public async Task IncompleteResult_DoesNotInventFactsRewardsOrUnlocks(RunCompletionReason reason, string title)
        {
            var profile = new ProfileService(MetaCatalog.Load(), new MemoryProfileStore()); await profile.LoadAsync();
            var run = new RunModel(); run.Start();
            if (reason == RunCompletionReason.Defeat) run.Kill(); else run.Stop(reason);
            Assert.IsFalse(await profile.ApplyAsync(run.Outcome, true));
            var result = RunResultsProjection.Create(run.Outcome, profile);
            Assert.AreEqual(title, result.Outcome);
            Assert.IsNull(result.Level); Assert.IsNull(result.Kills); Assert.IsNull(result.Total);
            Assert.IsEmpty(result.Unlocks); Assert.IsTrue(result.CanRetrySave);
            Assert.AreEqual("Не удалось сохранить результат", result.SaveStatus);
        }
    }
}
