using System;
using System.Collections;
using System.Linq;
using Game.Content;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class UiLayoutR2SmokeTests
    {
        [UnityTest]
        public IEnumerator GameplaySpace_DevelopmentControlsRejectKeyboardFocus_AndDoNotConsumePause()
        {
            var host = new GameObject("Pause shortcut focus harness");
            var panel = ScriptableObject.CreateInstance<PanelSettings>();
            var target = new RenderTexture(1280, 720, 0);
            UiToolkitGameplayView view = null;
            try
            {
                panel.targetTexture = target;
                panel.themeStyleSheet = Resources.Load<ThemeStyleSheet>("UI/GameplayTheme");
                var doc = host.AddComponent<UIDocument>();
                doc.panelSettings = panel;
                doc.visualTreeAsset = Resources.Load<VisualTreeAsset>("UI/GameplayUi");
                yield return null;
                view = new UiToolkitGameplayView(doc.rootVisualElement);
                view.SetDevelopmentControlsVisible(true);
                view.RenderRunOverlay(new RunOverlayViewState(false, "", false));
                var developmentButton = doc.rootVisualElement.Q<Button>(GameplayUiElementIds.DevelopmentToggleButton);
                developmentButton.Focus();
                yield return null;
                Assert.IsFalse(developmentButton.focusable, "DEV controls must not steal gameplay keys.");
                Assert.AreNotSame(developmentButton, doc.rootVisualElement.panel.focusController.focusedElement);
                Assert.IsFalse(view.ConsumePauseShortcut(true), "A focused gameplay control cannot block Space pause.");
            }
            finally
            {
                view?.Dispose();
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(panel);
                Object.DestroyImmediate(target);
            }
        }

        [UnityTest]
        public IEnumerator DenseRecipes_OnePauseScroll_HoverInspectsAndCardSubmitSelects()
        {
            foreach (var size in new[] { new Vector2Int(1920, 1080), new Vector2Int(1280, 720) })
            {
                var host = new GameObject("R2 density harness");
                var panel = ScriptableObject.CreateInstance<PanelSettings>();
                var target = new RenderTexture(size.x, size.y, 0);
                UiToolkitGameplayView view = null;
                try
                {
                    panel.targetTexture = target; panel.scaleMode = PanelScaleMode.ConstantPixelSize;
                    panel.clearColor = true; panel.colorClearValue = new Color(.14f, .10f, .17f, 1);
                    panel.themeStyleSheet = Resources.Load<ThemeStyleSheet>("UI/GameplayTheme");
                    var doc = host.AddComponent<UIDocument>(); doc.panelSettings = panel;
                    doc.visualTreeAsset = Resources.Load<VisualTreeAsset>("UI/GameplayUi"); yield return null;
                    var root = doc.rootVisualElement; root.styleSheets.Add(Resources.Load<StyleSheet>("UI/GameplayUiStyles"));
                    view = new UiToolkitGameplayView(root);
                    var choices = 0; var resumes = 0;
                    view.DraftOptionSelected += (_, __) => choices++; view.PauseRequested += () => resumes++;
                    var slots = Enumerable.Range(0, 6).Select(i => new BuildSlotViewState("Длинное имя умения", 6, true)).ToArray();
                    var components = string.Join("\n", Enumerable.Range(1, 6).Select(i => $"✓ Компонент {i}  2/3"));
                    var recipes = Enumerable.Range(0, 16).Select(i => new SetRecipeProgressViewState($"Тестовый сет {i}", 1, 6,
                        false, false, hasProgress: true, components: components, isMissed: i >= 12, effect: $"Краткий эффект {i}.")).ToArray();
                    recipes[0] = new SetRecipeProgressViewState("Неначатый сет", 0, 6, false, false,
                        hasProgress: false, ownedComponents: 0,
                        components: string.Join("\n", Enumerable.Range(1, 6).Select(i => $"○ Компонент {i}  0/3")),
                        effect: "Краткий эффект неначатого сета.");
                    var received = Enumerable.Range(0, 4).Select(i => new SetBuildViewState($"Полученный {i}", $"Полученный эффект {i}.")).ToArray();
                    view.RenderBuild(new BuildViewState(slots, slots, received, recipes));
                    view.RenderRunOverlay(new RunOverlayViewState(true, "Передышка", true));
                    yield return null; yield return null;
                    var pauseMaterial = root.Q(className: "pause-panel").Q(className: "folio-panel-texture");
                    Assert.IsNotNull(pauseMaterial);
                    Assert.AreEqual(Resources.Load<Sprite>("Art/UI/Folio/ui-folio-surface-background"),
                        pauseMaterial.style.backgroundImage.value.sprite);
                    Assert.AreEqual(0.48f, pauseMaterial.resolvedStyle.opacity, 0.001f);
                    Assert.IsNotNull(root.Q(className: "pause-left").Q(className: "folio-panel-texture"));
                    Assert.IsNotNull(root.Q(className: "pause-right").Q(className: "folio-panel-texture"));
                    var scroll = root.Q<ScrollView>(GameplayUiElementIds.PauseBuild);
                    Assert.Greater(scroll.verticalScroller.highValue, 0);
                    Assert.IsTrue(scroll.Contains(root.Q(GameplayUiElementIds.ReceivedSets)));
                    Assert.IsTrue(scroll.Contains(root.Q(GameplayUiElementIds.MissedSets)));
                    Assert.AreEqual(1, scroll.Query<ScrollView>().ToList().Count, "No nested set-section scrolls.");
                    var cards = root.Q(GameplayUiElementIds.PauseRecipes).Children().ToArray();
                    Assert.AreEqual(12, cards.Length, "Attainable recipes include zero-owned sets.");
                    Assert.AreEqual("Неначатый сет", cards[0].Q<Label>(GameplayUiElementIds.CardTitle).text);
                    Assert.AreEqual("0/6 · Не начат", cards[0].Q<Label>(className: "recipe-progress").text);
                    var columns = size.x == 1920 ? 3 : 2;
                    Assert.AreEqual(cards[0].worldBound.yMin, cards[columns - 1].worldBound.yMin, 1);
                    Assert.Greater(cards[columns].worldBound.yMin, cards[0].worldBound.yMin);
                    var footer = root.Q(GameplayUiElementIds.PauseFooter).worldBound;
                    var lastSlot = root.Q(GameplayUiElementIds.PauseSlots).Query(className: "pause-slot").ToList().Last();
                    Assert.LessOrEqual(lastSlot.worldBound.yMax, footer.yMin);
                    var trigger = root.Q(GameplayUiElementIds.ReceivedSets).Q<Button>();
                    UiFoundationSmokeTests.Submit(trigger); yield return null;
                    var popup = root.Q(GameplayUiElementIds.SetPopup);
                    Assert.AreEqual(DisplayStyle.Flex, popup.resolvedStyle.display);
                    Assert.IsTrue(view.ConsumePauseShortcut(true));
                    Assert.AreEqual(DisplayStyle.Flex, popup.resolvedStyle.display, "Space opening a focused set must not close it again in global routing.");
                    StringAssert.Contains("Полученный эффект", root.Q<Label>(GameplayUiElementIds.SetPopupEffect).text);
                    Assert.LessOrEqual(popup.worldBound.yMax, footer.yMin);
                    UiFoundationSmokeTests.Capture(target, $"r2-density-popup-{size.x}");
                    Assert.IsTrue(view.ConsumePauseShortcut(false));
                    Assert.AreSame(trigger, root.panel.focusController.focusedElement);
                    Assert.AreEqual(0, resumes);
                    scroll.scrollOffset = new Vector2(0, scroll.verticalScroller.highValue); yield return null; yield return null;
                    Assert.AreEqual(footer, root.Q(GameplayUiElementIds.PauseFooter).worldBound);
                    trigger = root.Q(GameplayUiElementIds.MissedSets).Q<Button>();
                    UiFoundationSmokeTests.Submit(trigger); yield return null;
                    Assert.LessOrEqual(popup.worldBound.yMax, footer.yMin);
                    Assert.LessOrEqual(popup.worldBound.xMax, size.x);
                    UiFoundationSmokeTests.Capture(target, $"r2-density-bottom-{size.x}");
                    scroll.scrollOffset = Vector2.zero; yield return null;
                    Assert.AreEqual(DisplayStyle.None, popup.resolvedStyle.display);
                    UiFoundationSmokeTests.Capture(target, $"r2-density-zero-progress-{size.x}");
                    UiFoundationSmokeTests.Submit((Button)cards[0]); yield return null;
                    Assert.AreEqual(DisplayStyle.Flex, popup.resolvedStyle.display);
                    Assert.AreEqual("Краткий эффект неначатого сета.", root.Q<Label>(GameplayUiElementIds.SetPopupEffect).text);
                    Assert.IsTrue(view.ConsumePauseShortcut(false));
                    Assert.AreSame(cards[0], root.panel.focusController.focusedElement);
                    Assert.AreEqual(0, choices);
                    Assert.AreEqual(0, resumes);
                    view.RenderRunOverlay(new RunOverlayViewState(false, "", false));
                    yield return null;
                    Assert.IsFalse(view.ConsumePauseShortcut(true), "Hidden pause controls cannot swallow the next gameplay Space.");
                    var projections = Enumerable.Range(0, 10).Select(i => new RecipeProjectionViewState($"Рецепт {i}", 1, 2, 3,
                        false, false, new[] { "✓ Компонент 1  2→3/3", "○ Компонент 2  0/2" }, $"Эффект рецепта {i}.")).ToArray();
                    var options = new[] { new DraftOptionViewState(new ContentId("TEST-A"), "Умение A", "+1 рикошет. Повторный удар слабее", recipes: projections),
                        new DraftOptionViewState(new ContentId("TEST-B"), "Умение B", "Урон +20%", recipes: projections) };
                    view.RenderDraft(new DraftViewState(true, 1, 1, options, Guid.NewGuid())); yield return null; yield return null;
                    Assert.IsNotNull(root.Q(className: "draft-panel").Q(className: "folio-panel-texture"));
                    var inspectB = root.Q<Button>(GameplayUiElementIds.DraftSelectButton(1));
                    UiFoundationSmokeTests.Hover(inspectB); yield return null;
                    Assert.IsFalse(inspectB.focusable, "Draft cards ignore keyboard navigation.");
                    Assert.AreEqual("Рецепт 0", root.Q<Label>(GameplayUiElementIds.DraftRecipeTitle).text.Split('·')[0].Trim());
                    Assert.AreEqual(0, choices);
                    Assert.Greater(root.Q<ScrollView>(GameplayUiElementIds.DraftRecipeList).verticalScroller.highValue, 0);
                    UiFoundationSmokeTests.Submit(root.Q<Button>(GameplayUiElementIds.DraftRecipeButton(8)));
                    Assert.AreEqual("Эффект рецепта 8.", root.Q<Label>(GameplayUiElementIds.DraftRecipeEffect).text);
                    Assert.AreEqual(0, choices);
                    yield return null; yield return null;
                    UiFoundationSmokeTests.Capture(target, $"r2-draft-density-{size.x}");
                    UiFoundationSmokeTests.Submit(inspectB);
                    Assert.AreEqual(1, choices);
                    view.RenderDraft(new DraftViewState(true, 1, 1, options, Guid.NewGuid(), isBanishMode: true));
                    StringAssert.Contains("исключить", root.Q<Label>(GameplayUiElementIds.DraftControlHint).text);
                }
                finally { view?.Dispose(); Object.DestroyImmediate(host); Object.DestroyImmediate(panel); Object.DestroyImmediate(target); }
            }
        }
    }
}
