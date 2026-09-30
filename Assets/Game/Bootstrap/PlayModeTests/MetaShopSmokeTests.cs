using System.Collections;
using System.Linq;
using Game.Meta;
using Game.Progression;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class MetaShopSmokeTests
    {
        [UnityTest]
        public IEnumerator PersonalShop_TwoResolutions_HeroRefundAndRunCounters()
        {
            var catalog = MetaCatalog.Load(); var codec = new ProfileCodec(catalog); var data = codec.Create();
            data.Currency = 10000;
            foreach (var rule in catalog.Unlocks.Values)
                if (rule.Kind == "character") data.Unlocked.Add(rule.Id);
            var store = new MemoryProfileStore();
            var write = store.WriteAsync(codec.Encode(data));
            while (!write.IsCompleted) yield return null;
            Assert.IsFalse(write.IsFaulted);
            ProductionSmokeScene.Load(store, false); yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            RenderTexture target = null;
            try
            {
                Assert.IsNull(root.Catalog, "Must test opening Meta before character selection.");
                root.ReturnToProfileSelection(); root.Meta(); yield return null;
                var document = root.ProfileDocument; var ui = document.rootVisualElement;
                foreach (var size in new[] { new Vector2Int(1920, 1080), new Vector2Int(1280, 720) })
                {
                    target = new RenderTexture(size.x, size.y, 24); document.panelSettings.targetTexture = target;
                    yield return null; yield return null;
                    var list = ui.Q<ScrollView>(GameplayUiElementIds.MetaUpgradeList);
                    var backdrop = ui.Q(GameplayUiElementIds.MetaBackdrop);
                    Assert.AreEqual(DisplayStyle.Flex, backdrop.resolvedStyle.display);
                    Assert.AreEqual(1f, backdrop.resolvedStyle.backgroundColor.a);
                    Assert.That(backdrop.worldBound.width, Is.EqualTo(size.x).Within(1));
                    Assert.That(backdrop.worldBound.height, Is.EqualTo(size.y).Within(1));
                    Assert.IsNotNull(backdrop.Q(className: "folio-backdrop-visual"));
                    Assert.IsNotNull(ui.Q(GameplayUiElementIds.MetaBody).Q(className: "folio-panel-texture"));
                    Assert.AreEqual(12, list.contentContainer.childCount);
                    foreach (var upgrade in catalog.Upgrades.Values)
                    {
                        var icon = list.Q<Image>(GameplayUiElementIds.MetaUpgradeIcon(upgrade.Id));
                        Assert.IsNotNull(icon.sprite, upgrade.Id);
                        Assert.AreEqual(32, icon.worldBound.width, 0.1f);
                        var upgradeRow = list.Q(GameplayUiElementIds.MetaCard(upgrade.Id));
                        var title = upgradeRow.Q<Label>(className: "shop-card-title");
                        Assert.LessOrEqual(icon.worldBound.xMax, title.worldBound.xMin);
                        Assert.LessOrEqual(title.worldBound.xMax, upgradeRow.Q(className: "shop-level").worldBound.xMin);
                    }
                    Assert.IsNotNull(ui.Q<Image>(GameplayUiElementIds.MetaPortrait).sprite);
                    foreach (var portrait in ui.Q<ScrollView>(GameplayUiElementIds.MetaRoster).Query<Image>().ToList()) Assert.IsNotNull(portrait.sprite);
                    Bounded(ui.Q(GameplayUiElementIds.MetaClose), size);
                    Bounded(ui.Q(GameplayUiElementIds.MetaRefund), size);
                    Bounded(ui.Q(GameplayUiElementIds.MetaBuy("META-003")), size);
                    var toggle = ui.Q<Toggle>(GameplayUiElementIds.MetaShopToggle);
                    Assert.GreaterOrEqual(toggle.Q(className: "unity-toggle__input").worldBound.width, 20);
                    if (size.x == 1280) Assert.Greater(list.verticalScroller.highValue, 0);
                    UiFoundationSmokeTests.Capture(target, $"meta-personal-{size.x}x{size.y}");
                    list.scrollOffset = new Vector2(0, list.verticalScroller.highValue); yield return null;
                    Bounded(ui.Q(GameplayUiElementIds.MetaBuy("META-010")), size);
                    var row = list.Q(GameplayUiElementIds.MetaCard("META-010"));
                    var position = row.worldBound.position; var scrollPosition = list.scrollOffset;
                    ui.Q<Toggle>(GameplayUiElementIds.MetaShopToggle).value = true; yield return null; yield return null;
                    Assert.AreSame(row,list.Q(GameplayUiElementIds.MetaCard("META-010")),"Toggle must not rebuild rows.");
                    Assert.AreEqual(scrollPosition,list.scrollOffset);
                    Assert.Less(Vector2.Distance(position,row.worldBound.position),1);
                    ui.Q<Toggle>(GameplayUiElementIds.MetaShopToggle).value = false; yield return null;
                    list.scrollOffset = Vector2.zero;
                    document.panelSettings.targetTexture = null; Object.Destroy(target); target = null;
                }
                Submit(ui, GameplayUiElementIds.MetaBuy("META-003")); yield return null;
                Assert.AreEqual(1, root.Profile.Level("META-003", "CHAR-001"));
                Submit(ui, GameplayUiElementIds.MetaHero("CHAR-002")); yield return null;
                Assert.IsFalse(ui.Q<Button>(GameplayUiElementIds.MetaRefund).enabledSelf);
                Submit(ui, GameplayUiElementIds.MetaHero("CHAR-001")); yield return null;
                Submit(ui, GameplayUiElementIds.MetaRefund); yield return null;
                Assert.IsFalse(ui.Q<Button>(GameplayUiElementIds.MetaBuy("META-003")).enabledSelf);
                Assert.IsFalse(ui.Q<Button>(GameplayUiElementIds.MetaClose).enabledSelf);
                Submit(ui, GameplayUiElementIds.MetaRefundCancel); yield return null;
                Assert.AreEqual(9900, root.Profile.Currency);
                Submit(ui, GameplayUiElementIds.MetaRefund); Submit(ui, GameplayUiElementIds.MetaRefundConfirm); yield return null;
                Assert.AreEqual(9000, root.Profile.Currency); Assert.AreEqual(0, root.Profile.Invested("CHAR-001"));
                ui.Q<Toggle>(GameplayUiElementIds.MetaShopToggle).value = true; yield return null;
                Assert.IsTrue(root.Profile.UpgradesDisabled);
                ui.Q<Toggle>(GameplayUiElementIds.MetaShopToggle).value = false; yield return null;
                Assert.IsFalse(root.Profile.UpgradesDisabled);
                Submit(ui, GameplayUiElementIds.MetaBuy("META-009")); Submit(ui, GameplayUiElementIds.MetaBuy("META-010")); yield return null;
                Submit(ui, GameplayUiElementIds.MetaClose); root.Play(); CharacterSelectionSmokeDriver.StartDefault(root); yield return null;
                var draft = Object.FindAnyObjectByType<LevelUpDraftRuntime>();
                Assert.AreEqual(2, draft.RemainingRerolls); Assert.AreEqual(2, draft.RemainingBanishes);
            }
            finally
            {
                if (root.ProfileDocument != null) root.ProfileDocument.panelSettings.targetTexture = null;
                if (target != null) Object.Destroy(target);
                root.Shutdown();
            }
        }
        private static void Submit(VisualElement root, string id) => UiFoundationSmokeTests.Submit(root.Q<Button>(id));
        [UnityTest]
        public IEnumerator Unlocks_FiltersAndScroll_TwoResolutions()
        {
            ProductionSmokeScene.Load(new MemoryProfileStore(), false); yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            RenderTexture target = null;
            try
            {
                root.ReturnToProfileSelection(); root.Meta(); yield return null;
                var document = root.ProfileDocument; var ui = document.rootVisualElement;
                Submit(ui, GameplayUiElementIds.MetaTabUnlocks); yield return null;
                foreach (var size in new[] { new Vector2Int(1920, 1080), new Vector2Int(1280, 720) })
                {
                    target = new RenderTexture(size.x, size.y, 24); document.panelSettings.targetTexture = target;
                    yield return null; yield return null;
                    var list = ui.Q<ScrollView>(GameplayUiElementIds.MetaUnlockList);
                    Submit(ui, GameplayUiElementIds.MetaUnlockType("all")); yield return null;
                    Assert.AreEqual(70, list.Query(className: "shop-unlock").ToList().Count);
                    Assert.AreEqual(0,list.Query(className:"shop-unknown").ToList().Count);
                    Assert.AreEqual(70, list.Query<Image>().ToList().Count);
                    foreach(var icon in list.Query<Image>().ToList())
                        Assert.IsTrue(icon.sprite != null || icon.image != null, "Each unlock has a sprite or blurred image.");
                    Assert.Greater(list.verticalScroller.highValue, 0);
                    Bounded(ui.Q(GameplayUiElementIds.MetaUnlockFilters), size);
                    Bounded(ui.Q(GameplayUiElementIds.MetaUnlockTotal), size);
                    Bounded(ui.Q(GameplayUiElementIds.MetaClose), size);
                    UiFoundationSmokeTests.Capture(target, $"meta-unlocks-{size.x}x{size.y}");
                    foreach (var kind in new[] { "character", "field", "ability", "set" })
                    {
                        Submit(ui, GameplayUiElementIds.MetaUnlockType(kind)); yield return null;
                        Assert.AreEqual(kind == "ability" ? 10 : kind == "set" ? 15 : 10, list.Query(className: "shop-unlock").ToList().Count);
                        Assert.IsTrue(ui.Q<Button>(GameplayUiElementIds.MetaUnlockType(kind)).ClassListContains("shop-selected"));
                        if (kind == "field") Assert.AreEqual(10, list.Query<Image>().ToList().Count);
                        if(kind == "ability")
                        {
                            Assert.AreEqual("Закрыто · 10",ui.Q<Button>(GameplayUiElementIds.MetaUnlockState(1)).text);
                            Assert.IsTrue(ui.Q<Button>(GameplayUiElementIds.MetaUnlockState(1)).ClassListContains("shop-selected"));
                            Assert.IsFalse(ui.Q<Button>(GameplayUiElementIds.MetaUnlockState(0)).ClassListContains("shop-selected"));
                            Assert.AreEqual(10,list.Query(className:"shop-unlock").ToList().Count);
                        }
                        Assert.LessOrEqual(list.Query(className: "shop-unlock").ToList().Max(e => e.worldBound.xMax), size.x);
                        UiFoundationSmokeTests.Capture(target, $"meta-unlocks-{kind}-{size.x}x{size.y}");
                    }
                    Submit(ui, GameplayUiElementIds.MetaUnlockState(2)); yield return null;
                    Assert.AreEqual(0, list.Query(className: "shop-unlock").ToList().Count);
                    Assert.IsNotNull(list.Q(className: "shop-unlock-empty"));
                    Submit(ui, GameplayUiElementIds.MetaUnlockState(0)); yield return null;
                    list.scrollOffset = new Vector2(0, list.verticalScroller.highValue); yield return null;
                    Bounded(list.Query(className: "shop-unlock").ToList().Last(), size);
                    document.panelSettings.targetTexture = null; Object.Destroy(target); target = null;
                }
            }
            finally
            {
                if (root.ProfileDocument != null) root.ProfileDocument.panelSettings.targetTexture = null;
                if (target != null) Object.Destroy(target);
                root.Shutdown();
            }
        }
        private static void Bounded(VisualElement element, Vector2Int size)
        {
            Assert.Greater(element.worldBound.width, 0); Assert.Greater(element.worldBound.height, 0);
            Assert.GreaterOrEqual(element.worldBound.xMin, 0); Assert.GreaterOrEqual(element.worldBound.yMin, 0);
            Assert.LessOrEqual(element.worldBound.xMax, size.x + 1); Assert.LessOrEqual(element.worldBound.yMax, size.y + 1);
        }
    }
}
