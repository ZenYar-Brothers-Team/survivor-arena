using Game.Pickup;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;
namespace Game.UI.Tests
{
    public sealed class PickupPresenterTests
    {
        [TestCase(true, 2)]
        [TestCase(false, 0)]
        public void Presenter_CommandsAreDevelopmentOnly_FeedbackIsReleaseSafe(bool development, int expectedDrops)
        {
            var harness = new PickupUiHarness { Snapshot = new PickupSnapshot(2, 1, 0, 0, 0, 1, "Potion +20 HP") };
            using var presenter = new PickupPresenter(harness, harness, development);
            harness.Potion(); harness.Book(); Assert.AreEqual(expectedDrops, harness.Drops);
            Assert.AreEqual("Potion +20 HP", harness.Rendered.Feedback); Assert.AreEqual(development, harness.Development);
            presenter.Dispose(); harness.Potion(); harness.Book();
            Assert.AreEqual(expectedDrops, harness.Drops);
            harness.Snapshot = default; harness.Change(); Assert.AreEqual("Potion +20 HP", harness.Rendered.Feedback);
        }
        [Test]
        public void View_SemanticIdsAndReleaseVisibility_WorkWithoutDevelopmentPanel()
        {
            var root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Resources/UI/GameplayUi.uxml").CloneTree();
            using var view = new UiToolkitPickupView(root);
            view.Render(new PickupSnapshot(2, 1, 0, 0, 0, 1, "Potion +0 HP"), false);
            Assert.AreEqual("Potion +0 HP", root.Q<Label>(GameplayUiElementIds.PickupFeedback).text);
            Assert.AreEqual(DisplayStyle.None, root.Q<Button>(GameplayUiElementIds.DropPotion).style.display.value);
            Assert.AreEqual(DisplayStyle.None, root.Q<Label>(GameplayUiElementIds.PickupObservation).style.display.value);
            view.Render(default, true);
            Assert.AreEqual(DisplayStyle.None, root.Q<Label>(GameplayUiElementIds.PickupFeedback).style.display.value);
            Assert.AreEqual(DisplayStyle.Flex, root.Q<Button>(GameplayUiElementIds.DropBook).style.display.value);
        }
    }
}
