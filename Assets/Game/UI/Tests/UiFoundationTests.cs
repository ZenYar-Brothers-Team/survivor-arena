using System;
using System.Collections.Generic;
using Game.Content;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace Game.UI.Tests
{
    public sealed class UiFoundationTests
    {
        private static VisualElement CreateRoot() => AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/Game/UI/Resources/UI/GameplayUi.uxml").CloneTree();

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void Draft_ShortPool_PreservesThreePositions(int count)
        {
            var root = CreateRoot();
            using var view = new UiToolkitGameplayView(root);
            var options = new List<DraftOptionViewState>();
            for (var i = 0; i < count; i++) options.Add(new DraftOptionViewState(new ContentId($"FIXTURE-{i}"), "Option", "Current → next"));
            view.RenderDraft(new DraftViewState(true, 0, 0, options, Guid.NewGuid()));
            Assert.AreEqual(3, root.Q(GameplayUiElementIds.DraftOptions).childCount);
            for (var i = 0; i < 3; i++) Assert.AreEqual(i < count, root.Q<Button>(GameplayUiElementIds.DraftSelectButton(i)).enabledSelf);
        }

        [Test]
        public void Projection_CompletingRecipe_ShowsCurrentAndProjectedWithoutAcquisition()
        {
            var components = new List<string> { "✓ First", "◐ Current Lv.2 / required Lv.4 → ✓ Lv.4" };
            var recipe = new RecipeProjectionViewState("Fixture recipe", 1, 2, 2, true, false, components);
            var recipes = new List<RecipeProjectionViewState> { recipe, recipe, recipe };
            var option = new DraftOptionViewState(new ContentId("FIXTURE-PROJECTION"), "Fixture", "Lv.3 → 4", recipes: recipes);
            components.Clear(); recipes.Clear();
            var card = new DraftCard(option, null);
            StringAssert.Contains("1/2 → 2/2", card.Details);
            StringAssert.Contains("not yet acquired", card.Details);
            StringAssert.Contains("required Lv.4", card.Details);
            Assert.AreEqual("+1 more", card.Q<Label>(GameplayUiElementIds.CardMore).text);
            Assert.IsTrue(card.ClassListContains("draft-card-completing"));
            Assert.IsFalse(recipe.IsAcquired);
            Assert.AreEqual(1, recipe.Current);
        }

        [Test]
        public void ContentCard_LockedSelected_UsesIndependentStates()
        {
            var card = new ContentCard(new ContentCardViewState("Fixture", "Unlock condition", isSelected: true, isLocked: true));
            Assert.IsFalse(card.enabledSelf);
            Assert.IsTrue(card.ClassListContains("content-card-selected"));
            Assert.IsTrue(card.ClassListContains("content-card-locked"));
            Assert.AreEqual("LOCKED", card.Q<Label>(GameplayUiElementIds.CardStatus).text);
        }

        [Test]
        public void Build_UnchangedSnapshot_RetainsElementsAndFiltersPauseRecipes()
        {
            var root = CreateRoot();
            using var view = new UiToolkitGameplayView(root);
            var slots = new List<BuildSlotViewState> { new BuildSlotViewState("Max fixture", 6, true, "Long effect") };
            var recipes = new[] {
                new SetRecipeProgressViewState("No progress", 0, 2, false, false),
                new SetRecipeProgressViewState("Partial threshold", 0, 2, false, false, hasProgress: true),
                new SetRecipeProgressViewState("Fulfilled", 2, 2, true, false),
                new SetRecipeProgressViewState("Acquired", 2, 2, false, true) };
            var state = new BuildViewState(slots, slots, Array.Empty<SetBuildViewState>(), recipes);
            slots.Clear();
            view.RenderBuild(state);
            var element = root.Q(GameplayUiElementIds.ActiveSlot(0));
            view.RenderBuild(state);
            Assert.AreSame(element, root.Q(GameplayUiElementIds.ActiveSlot(0)));
            Assert.IsFalse(element.focusable);
            var titles = root.Q(GameplayUiElementIds.PauseBuild).Query<Label>(GameplayUiElementIds.CardTitle).ToList();
            Assert.IsTrue(titles.Exists(label => label.text == "Fulfilled"));
            Assert.IsTrue(titles.Exists(label => label.text == "Partial threshold"));
            Assert.IsFalse(titles.Exists(label => label.text == "No progress" || label.text == "Acquired"));
        }

        [Test]
        public void Notification_ZeroDeltaAndExpiry_DoesNotBlockOrAdvanceWhilePaused()
        {
            var label = new Label();
            var notification = new UiNotification(label);
            notification.Show("TRAVELER APPEARED", 2);
            notification.Tick(0);
            Assert.AreEqual(DisplayStyle.Flex, label.style.display.value);
            Assert.AreEqual(PickingMode.Ignore, label.pickingMode);
            Assert.IsFalse(label.focusable);
            notification.Show("BOSS INCOMING", 1);
            notification.Tick(1);
            Assert.AreEqual(DisplayStyle.None, label.style.display.value);
            Assert.AreEqual("BOSS INCOMING", label.text);
        }
    }
}
