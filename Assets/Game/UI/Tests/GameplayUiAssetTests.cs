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
            Assert.IsNotNull(root.Q<ProgressBar>(GameplayUiElementIds.ExperienceBar));
            Assert.IsNotNull(root.Q<Label>(GameplayUiElementIds.WaveLabel));
            Assert.IsNotNull(root.Q<Label>(GameplayUiElementIds.WaveObservation));
            Assert.IsNotNull(root.Q<Label>(GameplayUiElementIds.StatsObservation));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.PauseButton));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.BuildPanel));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.ActiveSlots));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.PassiveSlots));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.Sets));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.SetRecipeProgress));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.DraftOverlay));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.DraftRerollButton));
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
        public void RuntimeTheme_IsAvailableForStandardControls()
        {
            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Assets/Game/UI/Resources/UI/GameplayTheme.tss");
            Assert.IsNotNull(theme);
        }
    }
}
