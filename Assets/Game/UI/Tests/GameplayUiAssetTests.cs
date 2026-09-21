using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace Game.UI.Tests
{
    public sealed class GameplayUiAssetTests
    {
        [Test]
        public void Uxml_ContainsStableSemanticElements()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Resources/UI/GameplayUi.uxml");
            Assert.IsNotNull(asset);
            var root = asset.CloneTree();

            Assert.IsNotNull(root.Q<ProgressBar>(GameplayUiElementIds.HealthBar));
            Assert.IsNotNull(root.Q<ProgressBar>(GameplayUiElementIds.BossBar));
            Assert.IsNotNull(root.Q<ProgressBar>(GameplayUiElementIds.ExperienceBar));
            Assert.IsNotNull(root.Q<Label>(GameplayUiElementIds.WaveLabel));
            Assert.IsNotNull(root.Q<Label>(GameplayUiElementIds.WaveObservation));
            Assert.IsNotNull(root.Q<Label>(GameplayUiElementIds.StatsObservation));
            Assert.IsNotNull(root.Q<Label>(GameplayUiElementIds.SkillObservation));
            Assert.IsNotNull(root.Q<Label>(GameplayUiElementIds.ExperienceObservation));
            Assert.IsNotNull(root.Q<Label>(GameplayUiElementIds.StatsObservation));
            Assert.IsNotNull(root.Q<Label>(GameplayUiElementIds.SkillObservation));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.PauseButton));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.BuildPanel));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.ActiveSlots));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.PassiveSlots));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.Sets));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.SetRecipeProgress));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.DraftOverlay));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.DraftRerollButton));
            Assert.IsNotNull(root.Q<Label>(GameplayUiElementIds.DraftHeading));
            Assert.IsNotNull(root.Q<Label>(GameplayUiElementIds.DraftQueue));
            Assert.IsNotNull(root.Q<Label>(GameplayUiElementIds.BookCurrency));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.AddBookButton));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.RunOverlay));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.DevelopmentPanel));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.DevelopmentToggleButton));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.DevelopmentCloseButton));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.DevelopmentRunTab));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.DevelopmentBuildTab));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.DevelopmentPresentationTab));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.DevelopmentRunPane));
            Assert.IsNotNull(root.Q<ScrollView>(GameplayUiElementIds.DevelopmentBuildPane));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.DevelopmentPresentationPane));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.CharacterSelection));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.PresentationLiveButton));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.PresentationIdleButton));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.PresentationLeftButton));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.PresentationRightButton));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.PresentationResetButton));
        }

        [Test]
        public void ShortDraft_RendersThreePositionsWithDisabledEmptyCards()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Resources/UI/GameplayUi.uxml");
            var root = asset.CloneTree();
            using var view = new UiToolkitGameplayView(root);
            view.SetDevelopmentControlsVisible(false);
            view.RenderDraft(new DraftViewState(true, 1, 1, new[]
            {
                new DraftOptionViewState(new Game.Content.ContentId("FIXTURE-OPTION"), "Fixture", "level 1 -> 2"),
                new DraftOptionViewState(default, "No available option", "", false),
                new DraftOptionViewState(default, "No available option", "", false)
            }, System.Guid.NewGuid(), "TRAVELER BOOK", "Next: Level 3"));
            Assert.AreEqual(3, root.Q<VisualElement>(GameplayUiElementIds.DraftOptions).childCount);
            Assert.IsTrue(root.Q<Button>(GameplayUiElementIds.DraftSelectButton(0)).enabledSelf);
            Assert.IsFalse(root.Q<Button>(GameplayUiElementIds.DraftSelectButton(1)).enabledSelf);
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.DraftBanishModeButton));
            Assert.IsNotNull(root.Q<Label>(GameplayUiElementIds.DraftControlHint));
            Assert.AreEqual("TRAVELER BOOK", root.Q<Label>(GameplayUiElementIds.DraftHeading).text);
            Assert.AreEqual("Next: Level 3", root.Q<Label>(GameplayUiElementIds.DraftQueue).text);
            Assert.AreEqual(DisplayStyle.None, root.Q<VisualElement>(GameplayUiElementIds.DevelopmentPanel).style.display.value);
        }

        [Test]
        public void BanishMode_SameRevisionRerendersCardsAndControlState()
        {
            var root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Resources/UI/GameplayUi.uxml").CloneTree();
            using var view = new UiToolkitGameplayView(root);
            var revision = System.Guid.NewGuid();
            var options = new[] { new DraftOptionViewState(new Game.Content.ContentId("FIXTURE-A"), "A", "") };
            view.RenderDraft(new DraftViewState(true, 1, 1, options, revision));
            view.RenderDraft(new DraftViewState(true, 1, 1, options, revision, isBanishMode: true));
            Assert.IsTrue(root.Q<VisualElement>(GameplayUiElementIds.DraftOverlay).ClassListContains("draft-banish-mode"));
            Assert.AreEqual("Cancel banish", root.Q<Button>(GameplayUiElementIds.DraftBanishModeButton).text);
            Assert.IsFalse(root.Q<Button>(GameplayUiElementIds.DraftRerollButton).enabledSelf);
            StringAssert.Contains("Choose a card to banish", root.Q<Label>(GameplayUiElementIds.DraftControlHint).text);
            view.RenderDraft(new DraftViewState(true, 0, 0, options, revision));
            Assert.IsFalse(root.Q<Button>(GameplayUiElementIds.DraftBanishModeButton).enabledSelf);
            Assert.IsFalse(root.Q<VisualElement>(GameplayUiElementIds.DraftOverlay).ClassListContains("draft-banish-mode"));
        }

        [Test]
        public void RuntimeTheme_IsAvailableForStandardControls()
        {
            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Assets/Game/UI/Resources/UI/GameplayTheme.tss");
            Assert.IsNotNull(theme);
        }

        [Test]
        public void RuntimeStyles_LoadStandaloneSheetWithoutUxmlSubassetCollision()
        {
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Game/UI/Resources/UI/GameplayUiStyles.uss");
            Assert.IsNotNull(sheet);
            Assert.AreSame(sheet, UnityEngine.Resources.Load<StyleSheet>("UI/GameplayUiStyles"));
        }
    }
}
