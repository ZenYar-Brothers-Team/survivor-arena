using Game.UI;
using NUnit.Framework;
using UnityEngine.UIElements;
namespace Game.Bootstrap.PlayModeTests
{
    public static class CharacterSelectionSmokeDriver
    {
        public static void StartDefault(GameplayCompositionRoot root)
        {
            Assert.IsFalse(root.IsInitialized);
            var button = root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart);
            Assert.IsNotNull(button);
            Assert.IsTrue(button.enabledSelf);
            using var submit = NavigationSubmitEvent.GetPooled();
            submit.target = button;
            button.SendEvent(submit);
            Assert.IsFalse(root.IsInitialized);
            StartField(root);
        }
        public static void StartField(GameplayCompositionRoot root)
        {
            var button = root.FieldSelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.FieldSelectStart);
            Assert.IsNotNull(button);
            Assert.IsTrue(button.enabledSelf);
            using var submit = NavigationSubmitEvent.GetPooled();
            submit.target = button;
            button.SendEvent(submit);
            Assert.IsTrue(root.IsInitialized);
        }
    }
}
