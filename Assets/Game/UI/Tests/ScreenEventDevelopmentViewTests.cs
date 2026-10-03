using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Game.UI.Tests
{
    public sealed class ScreenEventDevelopmentViewTests
    {
        private static VisualElement Tree()
        {
            var root = new VisualElement();
            root.Add(new Button { name = GameplayUiElementIds.DevelopmentEventsTab });
            root.Add(new VisualElement { name = GameplayUiElementIds.ScreenEventList });
            root.Add(new Label { name = GameplayUiElementIds.ScreenEventSummary });
            return root;
        }

        private static IReadOnlyList<ScreenEventDevelopmentEntry> Entries() => new[]
        {
            new ScreenEventDevelopmentEntry("SCREEN-EVENT-001", "Копьё", false),
            new ScreenEventDevelopmentEntry("SCREEN-EVENT-012", "Небесный суд", true)
        };

        [Test]
        public void SetEntries_CreatesOneNamedButtonPerEvent_RareOnesMarked()
        {
            var root = Tree();
            using var view = new UiToolkitScreenEventDevelopmentView(root);
            view.SetEntries(Entries());
            Assert.AreEqual("Копьё", root.Q<Button>(GameplayUiElementIds.ScreenEventStart("SCREEN-EVENT-001")).text);
            Assert.AreEqual("Небесный суд (редкое)", root.Q<Button>(GameplayUiElementIds.ScreenEventStart("SCREEN-EVENT-012")).text);
            view.SetEntries(Entries());
            Assert.AreEqual(2, root.Q(GameplayUiElementIds.ScreenEventList).childCount, "Handing the list over again replaces the buttons.");
        }

        [Test]
        public void Render_ShowsTheSummaryAndTab_AndEnablesButtonsOnRequest()
        {
            var root = Tree();
            using var view = new UiToolkitScreenEventDevelopmentView(root);
            view.SetEntries(Entries());
            view.Render("Событие идёт", false, true);
            Assert.AreEqual("Событие идёт", root.Q<Label>(GameplayUiElementIds.ScreenEventSummary).text);
            Assert.AreEqual(DisplayStyle.Flex, root.Q<Button>(GameplayUiElementIds.DevelopmentEventsTab).style.display.value);
            Assert.IsFalse(root.Q<Button>(GameplayUiElementIds.ScreenEventStart("SCREEN-EVENT-001")).enabledSelf);
            view.Render("", true, false);
            Assert.AreEqual(DisplayStyle.None, root.Q<Button>(GameplayUiElementIds.DevelopmentEventsTab).style.display.value);
            Assert.IsTrue(root.Q<Button>(GameplayUiElementIds.ScreenEventStart("SCREEN-EVENT-001")).enabledSelf);
        }
    }
}
