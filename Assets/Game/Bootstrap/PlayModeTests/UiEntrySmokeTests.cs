using System.Collections;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class UiEntrySmokeTests
    {
        [UnityTest]
        public IEnumerator EntryScreens_TwoResolutions_InspectAndConfirmRemainSeparate()
        {
            foreach (var size in new[] { new Vector2Int(1920, 1080), new Vector2Int(1280, 720) })
            {
                ProductionSmokeScene.Load();
                yield return null; yield return null;
                var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                var target = new RenderTexture(size.x, size.y, 24);
                try
                {
                    var document = root.SelectionDocument;
                    document.panelSettings.targetTexture = target;
                    yield return null; yield return null;
                    var start = document.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart);
                    AssertFolioBackdrop(document.rootVisualElement, size);
                    AssertBounded(start, size);
                    UiFoundationSmokeTests.Capture(target, $"ui-entry-character-{size.x}x{size.y}");
                    UiFoundationSmokeTests.Submit(document.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectCard("CHAR-002")));
                    yield return null; yield return null;
                    Assert.IsFalse(start.enabledSelf);
                    Assert.AreEqual("CHAR-001", root.Selection.SelectedId.ToString());
                    var mysteryCard = document.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectCard("CHAR-002"));
                    Assert.AreEqual("?", mysteryCard.Q<Label>(className: "entry-choice-name").text);
                    Assert.IsEmpty(mysteryCard.Q<Label>(GameplayUiElementIds.CardStatus).text);
                    var detail = document.rootVisualElement.Q(className: "entry-character-detail");
                    AssertPanelTexture(detail);
                    Assert.IsTrue(detail.ClassListContains("entry-mystery"));
                    Assert.AreEqual(DisplayStyle.None, detail.Q(className: "entry-character-copy").resolvedStyle.display);
                    Assert.IsEmpty(document.rootVisualElement.Q<Label>(className: "entry-footer-detail").text);
                    foreach (var label in document.rootVisualElement.Query<Label>().ToList())
                        StringAssert.DoesNotContain("Бугор", label.text ?? "");
                    UiFoundationSmokeTests.Capture(target, $"ui-entry-locked-{size.x}x{size.y}");
                    UiFoundationSmokeTests.Submit(document.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectCard("CHAR-001")));
                    Assert.IsFalse(detail.ClassListContains("entry-mystery"));
                    UiFoundationSmokeTests.Submit(start);
                    yield return null;
                    document = root.FieldSelectionDocument;
                    Assert.IsNotNull(document);
                    document.panelSettings.targetTexture = target;
                    yield return null; yield return null;
                    AssertFolioBackdrop(document.rootVisualElement, size);
                    AssertBounded(document.rootVisualElement.Q<Button>(GameplayUiElementIds.FieldSelectStart), size);
                    foreach (var card in document.rootVisualElement.Query<Button>(className: "entry-field-choice").ToList())
                    {
                        // Designed cards fit; the separate DEV section is intentionally below and scrolls into view.
                        if (!card.name.Contains("FIELD-DEV-")) AssertBounded(card, size);
                        foreach (var label in card.Query<Label>().ToList())
                            StringAssert.DoesNotContain("/5", label.text ?? "");
                    }
                    var testCards = document.rootVisualElement.Query<Button>(className: "entry-field-choice").ToList()
                        .FindAll(card => card.name.Contains("FIELD-DEV-"));
                    if (testCards.Count > 0)
                    {
                        var fieldScroll = document.rootVisualElement.Q<ScrollView>();
                        Assert.Greater(fieldScroll.verticalScroller.highValue, 0, "DEV fields must be reachable by scrolling.");
                        fieldScroll.scrollOffset = new Vector2(0, fieldScroll.verticalScroller.highValue);
                        yield return null; yield return null;
                        AssertBounded(testCards[testCards.Count - 1], size);
                        fieldScroll.scrollOffset = Vector2.zero;
                        yield return null;
                    }
                    Assert.IsNull(document.rootVisualElement.Q(className: "entry-sword"), "Field cards no longer show a difficulty row.");
                    UiFoundationSmokeTests.Capture(target, $"ui-entry-fields-{size.x}x{size.y}");
                    document.panelSettings.targetTexture = null;
                    using (var stress = new FieldSelectScreen(root.transform, root.FieldSelection, root.Catalog.Registry))
                    {
                        stress.Document.panelSettings.targetTexture = target;
                        var cards = new System.Collections.Generic.List<FieldSelectCardViewState>();
                        for (var i = 0; i < 10; i++)
                            cards.Add(new FieldSelectCardViewState("FIXTURE-GRID-" + i,
                                new ContentCardViewState("Пограничные руины " + i, "", "", null, true, false, false), "", difficulty: 5));
                        stress.Render(cards, true);
                        yield return null; yield return null;
                        var grid = stress.Document.rootVisualElement.Q(GameplayUiElementIds.FieldSelectCards);
                        foreach (var card in grid.Query<Button>().ToList()) AssertBounded(card, size);
                        var scroll = stress.Document.rootVisualElement.Q<ScrollView>();
                        UiFoundationSmokeTests.Capture(target, $"ui-entry-ten-fields-{size.x}x{size.y}");
                        Assert.LessOrEqual(scroll.verticalScroller.highValue, 1, $"Ten fields must fit without scrolling at {size}.");
                        for (var i = 10; i < 21; i++)
                            cards.Add(new FieldSelectCardViewState("FIXTURE-GRID-" + i,
                                new ContentCardViewState("Поле " + i, "", "", null, true, false, false), ""));
                        stress.Render(cards, true);
                        yield return null; yield return null;
                        Assert.Greater(scroll.verticalScroller.highValue, 0);
                        scroll.scrollOffset = new Vector2(0, scroll.verticalScroller.highValue);
                        yield return null;
                        AssertBounded(stress.Document.rootVisualElement.Q<Button>(GameplayUiElementIds.FieldSelectStart), size);
                        stress.Document.panelSettings.targetTexture = null;
                    }
                    root.MainMenu();
                    yield return null;
                    document = root.ShellDocument;
                    document.panelSettings.targetTexture = target;
                    yield return null; yield return null;
                    Assert.IsNotNull(document.rootVisualElement.Q(GameplayUiElementIds.EntryMenuArt));
                    UiFoundationSmokeTests.Capture(target, $"ui-entry-menu-{size.x}x{size.y}");
                    document.panelSettings.targetTexture = null;
                }
                finally
                {
                    root.Shutdown();
                    Object.DestroyImmediate(target);
                }
            }
        }

        private static void AssertBounded(VisualElement element, Vector2Int size)
        {
            Assert.Greater(element.worldBound.width, 0);
            Assert.GreaterOrEqual(element.worldBound.xMin, -1);
            Assert.GreaterOrEqual(element.worldBound.yMin, -1);
            var label = element.name + " " + element.worldBound;
            Assert.LessOrEqual(element.worldBound.xMax, size.x + 1, label);
            Assert.LessOrEqual(element.worldBound.yMax, size.y + 1, label);
        }

        private static void AssertFolioBackdrop(VisualElement root, Vector2Int size)
        {
            var backdrop = root.Q(className: "folio-backdrop-visual");
            Assert.IsNotNull(backdrop);
            Assert.That(backdrop.worldBound.width, Is.EqualTo(size.x).Within(1));
            Assert.That(backdrop.worldBound.height, Is.EqualTo(size.y).Within(1));
        }

        private static void AssertPanelTexture(VisualElement panel)
        {
            var texture = panel.Q(className: "folio-panel-texture");
            Assert.IsNotNull(texture);
            Assert.GreaterOrEqual(texture.worldBound.width, panel.worldBound.width * 0.9f);
            Assert.GreaterOrEqual(texture.worldBound.height, panel.worldBound.height * 0.9f);
            Assert.AreEqual(Resources.Load<Sprite>("Art/UI/Folio/ui-folio-surface-background"),
                texture.style.backgroundImage.value.sprite);
        }
    }
}
