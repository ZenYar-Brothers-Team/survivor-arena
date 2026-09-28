using System.Collections;
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
            ProductionSmokeScene.Load(store); yield return null; yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            RenderTexture target = null;
            try
            {
                root.ReturnToProfileSelection(); root.Meta(); yield return null;
                var document = root.ProfileDocument; var ui = document.rootVisualElement;
                foreach (var size in new[] { new Vector2Int(1920, 1080), new Vector2Int(1280, 720) })
                {
                    target = new RenderTexture(size.x, size.y, 24); document.panelSettings.targetTexture = target;
                    yield return null; yield return null;
                    var list = ui.Q<ScrollView>(GameplayUiElementIds.MetaUpgradeList);
                    Assert.AreEqual(12, list.contentContainer.childCount);
                    Bounded(ui.Q(GameplayUiElementIds.MetaClose), size);
                    Bounded(ui.Q(GameplayUiElementIds.MetaRefund), size);
                    Bounded(ui.Q(GameplayUiElementIds.MetaBuy("META-003")), size);
                    var toggle = ui.Q<Toggle>(GameplayUiElementIds.MetaShopToggle);
                    Assert.GreaterOrEqual(toggle.Q(className: "unity-toggle__input").worldBound.width, 20);
                    if (size.x == 1280) Assert.Greater(list.verticalScroller.highValue, 0);
                    UiFoundationSmokeTests.Capture(target, $"meta-personal-{size.x}x{size.y}");
                    list.scrollOffset = new Vector2(0, list.verticalScroller.highValue); yield return null;
                    Bounded(ui.Q(GameplayUiElementIds.MetaBuy("META-014")), size);
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
                Assert.AreEqual(4, draft.RemainingRerolls); Assert.AreEqual(3, draft.RemainingBanishes);
            }
            finally
            {
                if (root.ProfileDocument != null) root.ProfileDocument.panelSettings.targetTexture = null;
                if (target != null) Object.Destroy(target);
                root.Shutdown();
            }
        }
        private static void Submit(VisualElement root, string id) => UiFoundationSmokeTests.Submit(root.Q<Button>(id));
        private static void Bounded(VisualElement element, Vector2Int size)
        {
            Assert.Greater(element.worldBound.width, 0); Assert.Greater(element.worldBound.height, 0);
            Assert.GreaterOrEqual(element.worldBound.xMin, 0); Assert.GreaterOrEqual(element.worldBound.yMin, 0);
            Assert.LessOrEqual(element.worldBound.xMax, size.x + 1); Assert.LessOrEqual(element.worldBound.yMax, size.y + 1);
        }
    }
}
