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
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.PauseButton));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.BuildPanel));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.ActiveSlots));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.PassiveSlots));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.DraftOverlay));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.DraftRerollButton));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.RunOverlay));
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.DevelopmentPanel));
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
