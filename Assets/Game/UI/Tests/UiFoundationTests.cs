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
            StringAssert.Contains("1/2", recipe.Summary);
            StringAssert.DoesNotContain("→", recipe.Progress);
            Assert.AreEqual("Завершит рецепт", recipe.Status);
            StringAssert.Contains("required Lv.4", recipe.Detail);
            Assert.AreEqual("Связанных сетов: 3", card.Q<Label>(GameplayUiElementIds.CardMore).text);
            Assert.IsFalse(card.ConfirmButton.enabledSelf);
            Assert.IsFalse(recipe.IsAcquired);
            Assert.AreEqual(1, recipe.Current);
        }

        [Test]
        public void DraftCard_TypeLabel_UsesSemanticElementAndVisualClass()
        {
            var active = new DraftCard(new DraftOptionViewState(
                new ContentId("FIXTURE-ACTIVE"), "Active fixture", "New", typeLabel: "Active"), null);
            var passive = new DraftCard(new DraftOptionViewState(
                new ContentId("FIXTURE-PASSIVE"), "Passive fixture", "New", typeLabel: "Passive"), null);
            var set = new DraftCard(new DraftOptionViewState(
                new ContentId("FIXTURE-SET"), "Set fixture", "New", isSet: true, typeLabel: "Set"), null);

            Assert.AreEqual("Активное", active.Q<Label>(GameplayUiElementIds.CardType).text);
            Assert.AreEqual("Пассивное", passive.Q<Label>(GameplayUiElementIds.CardType).text);
            Assert.AreEqual("Сет", set.Q<Label>(GameplayUiElementIds.CardType).text);
            foreach (var card in new[] { active, passive, set })
            {
                var header = card.Q(GameplayUiElementIds.CardHeader);
                Assert.IsNotNull(header);
                Assert.IsTrue(header.Contains(card.Q<Image>(GameplayUiElementIds.CardIcon)));
                Assert.IsTrue(header.Contains(card.Q<Label>(GameplayUiElementIds.CardType)));
                Assert.IsTrue(header.Contains(card.Q<Label>(GameplayUiElementIds.CardLevel)));
                Assert.IsFalse(header.Contains(card.Q<Label>(GameplayUiElementIds.CardTitle)));
            }
            Assert.IsTrue(active.ClassListContains("draft-type-active"));
            Assert.IsTrue(passive.ClassListContains("draft-type-passive"));
            Assert.IsTrue(set.ClassListContains("draft-type-set"));
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
            Assert.IsTrue(titles.Exists(label => label.text == "No progress"));
            Assert.IsFalse(titles.Exists(label => label.text == "Acquired"));
            var pauseBuild = root.Q(GameplayUiElementIds.PauseBuild);
            Assert.AreEqual(3, pauseBuild.Query<Label>(className: "pause-section-title").ToList().Count);
            Assert.IsNotNull(root.Q(GameplayUiElementIds.PauseSlots).Q(className: "pause-build-grid-active"));
            Assert.IsNotNull(root.Q(GameplayUiElementIds.PauseSlots).Q(className: "pause-build-grid-passive"));
            Assert.AreEqual(DisplayStyle.None, root.Q(GameplayUiElementIds.ReceivedSets).parent.style.display.value);
            Assert.AreEqual(DisplayStyle.None, root.Q(GameplayUiElementIds.MissedSets).parent.style.display.value);
        }

        [Test]
        public void PauseRecipes_OwnedBelowThreshold_CountsOwnedComponentsLikeDraft()
        {
            // 2026-09-30 OBS-03: pause showed 0/3 while the draft inspector showed 1/3 for the same build.
            var root = CreateRoot();
            using var view = new UiToolkitGameplayView(root);
            var recipes = new[] { new SetRecipeProgressViewState("Partial", 0, 3, false, false,
                hasProgress: true, ownedComponents: 2, components: "✓ First 1/3\n✓ Second 1/2\n○ Third 0/2") };
            view.RenderBuild(new BuildViewState(Array.Empty<BuildSlotViewState>(), Array.Empty<BuildSlotViewState>(),
                Array.Empty<SetBuildViewState>(), recipes));

            var card = root.Q(GameplayUiElementIds.PauseRecipes);
            Assert.AreEqual("2/3 · В процессе", card.Q<Label>(className: "recipe-progress").text);
        }

        [Test]
        public void PauseRecipes_ZeroOwnedAttainable_ShowsZeroAndKeepsAcquiredAndMissedSeparate()
        {
            var root = CreateRoot();
            using var view = new UiToolkitGameplayView(root);
            var recipes = new[] {
                new SetRecipeProgressViewState("Not started", 0, 3, false, false,
                    hasProgress: false, ownedComponents: 0, components: "○ First 0/3"),
                new SetRecipeProgressViewState("Acquired", 3, 3, false, true),
                new SetRecipeProgressViewState("Missed without progress", 0, 3, false, false,
                    hasProgress: false, isMissed: true) };
            view.RenderBuild(new BuildViewState(Array.Empty<BuildSlotViewState>(), Array.Empty<BuildSlotViewState>(),
                new[] { new SetBuildViewState("Acquired", "Acquired effect") }, recipes));

            var available = root.Q(GameplayUiElementIds.PauseRecipes);
            Assert.AreEqual(1, available.childCount);
            Assert.AreEqual("Not started", available.Q<Label>(GameplayUiElementIds.CardTitle).text);
            Assert.AreEqual("0/3 · Не начат", available.Q<Label>(className: "recipe-progress").text);
            Assert.AreEqual("○ First 0/3", available.Q<Label>(className: "recipe-components").text);
            var received = root.Q(GameplayUiElementIds.ReceivedSets);
            Assert.AreEqual(1, received.childCount);
            Assert.AreEqual("Acquired", received.Q<Label>(GameplayUiElementIds.CardTitle).text);
            var missed = root.Q(GameplayUiElementIds.MissedSets);
            Assert.AreEqual(1, missed.childCount);
            Assert.AreEqual("Missed without progress", missed.Q<Label>(GameplayUiElementIds.CardTitle).text);
        }
    }
}
