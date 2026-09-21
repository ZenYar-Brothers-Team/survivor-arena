using System;
using System.Collections;
using Game.Content;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class UiFoundationSmokeTests
    {
        [UnityTest]
        public IEnumerator Foundation_ReferenceAndSmallViewport_CardsStayBoundedAndSubmitOnce()
        {
            foreach (var size in new[] { new Vector2Int(1920, 1080), new Vector2Int(1280, 720) })
            {
                var host = new GameObject("UI foundation fixture harness");
                var panel = ScriptableObject.CreateInstance<PanelSettings>();
                var target = new RenderTexture(size.x, size.y, 0);
                UiToolkitGameplayView view = null;
                try
                {
                    panel.targetTexture = target;
                    panel.clearColor = true;
                    panel.colorClearValue = new Color(0.035f, 0.045f, 0.06f, 1f);
                    panel.scaleMode = PanelScaleMode.ConstantPixelSize;
                    panel.themeStyleSheet = Resources.Load<ThemeStyleSheet>("UI/GameplayTheme");
                    var document = host.AddComponent<UIDocument>();
                    document.panelSettings = panel;
                    document.visualTreeAsset = Resources.Load<VisualTreeAsset>("UI/GameplayUi");
                    // Let UIDocument finish attaching its target panel before retaining the tree.
                    yield return null;
                    var root = document.rootVisualElement;
                    root.styleSheets.Add(Resources.Load<StyleSheet>("UI/GameplayUiStyles"));
                    view = new UiToolkitGameplayView(root);
                    view.RenderHud(new HudViewState(75, 100, 0.5f, 6, 120,
                        new WaveViewState(1, 4, "Fixture", Game.Enemy.WavePhaseTag.Ordinary)));
                    view.SetDevelopmentControlsVisible(true);
                    var longText = string.Join(" ", new string[40]).Replace(" ", "Long fixture description ");
                    var projection = new RecipeProjectionViewState("Fixture recipe", 1, 2, 2, true, false,
                        new[] { "✓ Component", "◐ Option Lv.2 / required Lv.4 → ✓" });
                    view.RenderDraft(new DraftViewState(true, 1, 1, new[] {
                        new DraftOptionViewState(new ContentId("FIXTURE-A"), "A very long fixture title for layout checking", longText,
                            recipes: new[] { projection }),
                        new DraftOptionViewState(new ContentId("FIXTURE-SET"), "Fixture set", "Effect · no active/passive slot", isSet: true)
                    }, Guid.NewGuid(), "TRAVELER BOOK", "Next: Level 3 · 2 queued"));
                    var intents = 0;
                    view.DraftOptionSelected += (_, __) => intents++;
                    yield return null;
                    yield return null;
                    var card = root.Q<Button>(GameplayUiElementIds.DraftSelectButton(0));
                    var last = root.Q<Button>(GameplayUiElementIds.DraftSelectButton(2));
                    Assert.Greater(card.worldBound.width, 0);
                    Assert.Less(card.worldBound.width, size.x / 2f);
                    Assert.GreaterOrEqual(card.worldBound.xMin, 0);
                    Assert.LessOrEqual(last.worldBound.xMax, size.x);
                    Assert.LessOrEqual(last.worldBound.yMax, size.y);
                    Assert.IsFalse(last.enabledSelf);
                    card.Focus();
                    yield return null;
                    StringAssert.Contains("not yet acquired", root.Q<Label>(GameplayUiElementIds.DraftDetails).text);
                    Capture(target, $"ip10a-draft-{size.x}x{size.y}");
                    Submit(card);
                    Assert.AreEqual(1, intents);
                    // A details scroll must remain below all choices, not cover them.
                    Assert.GreaterOrEqual(root.Q<Label>(GameplayUiElementIds.DraftDetails).worldBound.yMin, card.worldBound.yMax);
                    view.RenderDraft(new DraftViewState(false, 0, 0, Array.Empty<DraftOptionViewState>()));
                    var slots = new BuildSlotViewState[6];
                    for (var i = 0; i < slots.Length; i++)
                        slots[i] = new BuildSlotViewState($"Fixture slot {i}", 6, true, "Maximum level effect");
                    view.RenderBuild(new BuildViewState(slots, slots,
                        new[] { new SetBuildViewState("Acquired fixture", "Set effect") },
                        new[] { new SetRecipeProgressViewState("Partial threshold", 0, 2, false, false,
                            "Component Lv.2 / required Lv.4", true) }));
                    view.RenderRunOverlay(new RunOverlayViewState(true, "PAUSED", true));
                    root.Q(GameplayUiElementIds.PauseBuild).Add(new ContentCard(
                        new ContentCardViewState("Locked fixture", "Unlock condition", isLocked: true)));
                    yield return null;
                    yield return null;
                    Assert.LessOrEqual(root.Q<Button>(GameplayUiElementIds.RunOverlayResumeButton).worldBound.yMax, size.y);
                    Capture(target, $"ip10a-pause-{size.x}x{size.y}");
                    var buildScroll = root.Q<ScrollView>(GameplayUiElementIds.PauseBuild);
                    buildScroll.scrollOffset = new Vector2(0, buildScroll.verticalScroller.highValue);
                    yield return null;
                    yield return null;
                    Capture(target, $"ip10a-pause-details-{size.x}x{size.y}");
                    view.RenderRunOverlay(new RunOverlayViewState(false, "", false));
                    Submit(root.Q<Button>(GameplayUiElementIds.DevelopmentToggleButton));
                    yield return null;
                    var dev = root.Q(GameplayUiElementIds.DevelopmentPanel).worldBound;
                    if (size.x == 1920)
                    {
                        Assert.LessOrEqual(dev.width, size.x * 0.25f);
                        Assert.LessOrEqual(dev.height, size.y * 0.45f);
                    }
                    view.SetDevelopmentControlsVisible(false);
                    Assert.AreEqual(DisplayStyle.None, root.Q(GameplayUiElementIds.DevelopmentPanel).style.display.value);
                }
                finally
                {
                    view?.Dispose();
                    Object.DestroyImmediate(host);
                    Object.DestroyImmediate(panel);
                    Object.DestroyImmediate(target);
                }
            }
        }
        private static void Submit(Button button)
        {
            using var submit = NavigationSubmitEvent.GetPooled();
            submit.target = button;
            button.SendEvent(submit);
        }

        private static void Capture(RenderTexture target, string name)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            var previous = RenderTexture.active;
            var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                image.Apply();
                var directory = System.IO.Path.Combine(Application.dataPath, "../TestResults");
                System.IO.Directory.CreateDirectory(directory);
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(directory, name + ".png"), image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                Object.DestroyImmediate(image);
            }
        }
    }
}
