using System;
using System.Collections;
using System.Linq;
using Game.Progression;
using Game.Run;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class RunResultsSmokeTests
    {
        [UnityTest]
        public IEnumerator ProductionBookResults_ReceiptRetryAndTwoResolutionCollections()
        {
            ProductionSmokeScene.Load(); yield return null; yield return null;
            var composition = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            CharacterSelectionSmokeDriver.StartDefault(composition); yield return null;
            var draft = Object.FindAnyObjectByType<LevelUpDraftRuntime>();
            var run = Object.FindAnyObjectByType<RunController>();
            RenderTexture target = null;
            try
            {
                Assert.IsTrue(draft.RequestBook(Guid.NewGuid(), run.Model.RunId, "PICKUP-002"));
                Assert.IsTrue(draft.Select(draft.CurrentDraft.Options[0].Definition.Id, draft.Revision));
                Assert.AreEqual(20, draft.BookCurrency);
                composition.QuitProfileRun(); yield return null; yield return null;
                Assert.AreEqual(20, composition.Profile.LastReceipt.BookReward);
                var document = composition.ProfileDocument;
                var ui = document.rootVisualElement;
                Assert.AreEqual("20", ui.Q<Label>(GameplayUiElementIds.ResultsBookReward).text);
                foreach (var size in new[] { new Vector2Int(1920, 1080), new Vector2Int(1280, 720) })
                {
                    target = new RenderTexture(size.x, size.y, 24);
                    document.panelSettings.targetTexture = target;
                    yield return null; yield return null;
                    Bounded(ui.Q(GameplayUiElementIds.MetaRetry), size);
                    var backdrop = ui.Q(GameplayUiElementIds.MetaBackdrop);
                    Assert.AreEqual(DisplayStyle.Flex, backdrop.resolvedStyle.display);
                    Assert.AreEqual(1f, backdrop.resolvedStyle.backgroundColor.a);
                    Assert.That(backdrop.worldBound.width, Is.EqualTo(size.x).Within(1));
                    Assert.That(backdrop.worldBound.height, Is.EqualTo(size.y).Within(1));
                    Assert.IsNotNull(backdrop.Q(className: "folio-backdrop-visual"));
                    Assert.IsNotNull(ui.Q(GameplayUiElementIds.MetaBody).Q(className: "folio-panel-texture"));
                    Bounded(ui.Q(GameplayUiElementIds.ResultsTotal), size);
                    UiFoundationSmokeTests.Capture(target, $"results-production-{size.x}x{size.y}");
                    var allSets = composition.Catalog.Sets.Select(s => RunResultsProjection.Content(s.Id.ToString(), composition.Profile.Catalog, composition.Catalog.Registry)).ToArray();
                    var mixed = new[] { "SET-002", "SKILL-008", "PASSIVE-013", "CHAR-002", "FIELD-002" }
                        .Select(id => RunResultsProjection.Content(id, composition.Profile.Catalog, composition.Catalog.Registry)).ToArray();
                    Assert.IsTrue(mixed.All(c => c.Icon != null));
                    using (var stress = new MetaScreen(composition.transform))
                    {
                        stress.Document.panelSettings.targetTexture = target;
                        foreach (var pending in new[] { false, true })
                        {
                            var state = new RunResultsViewState("Победа", "Клёпка · Деревенская окраина", "15:00", 38, 1746,
                                pending ? (long?)null : 190, pending ? (long?)null : 90, pending ? (long?)null : 280,
                                true, pending, pending ? "Не удалось сохранить результат" : "", allSets, pending ? Array.Empty<ResultContentViewState>() : mixed);
                            stress.Render(new MetaViewState(true, "", "", "", !pending, !pending, true, false, false, false,
                                Array.Empty<MetaCardViewState>(), Array.Empty<string>(), null, result: state));
                            yield return null; yield return null;
                            var tree = stress.Document.rootVisualElement;
                            var scroll = tree.Q<ScrollView>(GameplayUiElementIds.ResultsCollection);
                            Assert.Greater(scroll.verticalScroller.highValue, 0);
                            Bounded(tree.Q(GameplayUiElementIds.MetaRetry), size);
                            Bounded(tree.Q(GameplayUiElementIds.ResultsBookReward), size);
                            Assert.AreEqual(!pending, tree.Q<Button>(GameplayUiElementIds.MetaRetry).enabledSelf);
                            UiFoundationSmokeTests.Capture(target, $"results-{(pending ? "error" : "mixed")}-{size.x}x{size.y}");
                            scroll.scrollOffset = new Vector2(0, scroll.verticalScroller.highValue);
                            yield return null;
                            Bounded(tree.Q(GameplayUiElementIds.MetaRetry), size);
                        }
                        stress.Document.panelSettings.targetTexture = null;
                    }
                    document.panelSettings.targetTexture = null;
                    Object.Destroy(target); target = null;
                }
                var oldId = run.Model.RunId;
                UiFoundationSmokeTests.Submit(ui.Q<Button>(GameplayUiElementIds.MetaRetry)); yield return null;
                Assert.AreNotEqual(oldId, run.Model.RunId);
                Assert.AreEqual(DisplayStyle.None, ui.Q(GameplayUiElementIds.MetaBackdrop).resolvedStyle.display);
                Assert.AreEqual("CHAR-001", run.Model.Selection.CharacterId.ToString());
                Assert.AreEqual("FIELD-001", run.Model.Selection.FieldId.ToString());
            }
            finally
            {
                if (composition.ProfileDocument != null) composition.ProfileDocument.panelSettings.targetTexture = null;
                if (target != null) Object.Destroy(target);
                composition.Shutdown();
            }
        }

        private static void Bounded(VisualElement element, Vector2Int size)
        {
            Assert.Greater(element.worldBound.width, 0);
            Assert.Greater(element.worldBound.height, 0);
            Assert.GreaterOrEqual(element.worldBound.xMin, 0);
            Assert.GreaterOrEqual(element.worldBound.yMin, 0);
            Assert.LessOrEqual(element.worldBound.xMax, size.x + 1);
            Assert.LessOrEqual(element.worldBound.yMax, size.y + 1);
        }
    }
}
