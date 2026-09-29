using System.Linq;
using System.Threading.Tasks;
using Game.Meta;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
namespace Game.UI.Tests
{
    public sealed class MetaShopTests
    {
        [Test] public async Task Refund_ConfirmationAndCancellation_KeepEconomyInProfile()
        {
            var catalog = MetaCatalog.Load(); var codec = new ProfileCodec(catalog); var data = codec.Create(); data.Currency = 1000;
            var store = new MemoryProfileStore(); await store.WriteAsync(codec.Encode(data));
            var profile = new ProfileService(catalog, store); await profile.LoadAsync();
            await profile.PurchaseAsync("META-003", 0, "CHAR-001");
            var view = new FakeMetaView(); using var presenter = new MetaPresenter(profile, view, new FakeProfileNavigation());
            view.Shop(); view.Refund(); Assert.IsTrue(view.State.Shop.ConfirmRefund); Assert.AreEqual(900, profile.Currency);
            view.CancelRefund(); Assert.IsFalse(view.State.Shop.ConfirmRefund); Assert.AreEqual(1, profile.Level("META-003", "CHAR-001"));
            view.Refund(); view.ConfirmRefund(); await Task.Yield();
            Assert.AreEqual(0, profile.Currency); Assert.IsFalse(view.State.Shop.ConfirmRefund);
            Assert.AreEqual(0, view.State.Shop.Invested); Assert.IsNotNull(view.State.Shop.RefundReason);
            Assert.AreEqual(12, view.State.Cards.Count(c => c.Cap > 0));
            CollectionAssert.AreEqual(new[] { "META-004", "META-006", "META-005", "META-003", "META-013", "META-007", "META-014", "META-008", "META-011", "META-012", "META-009", "META-010" },
                view.State.Cards.Where(c => c.Cap > 0).Select(c => c.Id));
            Assert.IsTrue(view.State.Cards.Where(c => c.Cap > 0).All(c => c.Icon == null));
            Assert.IsTrue(view.State.Cards.Where(c => c.HiddenCharacter).All(c => c.Text == "?"));
        }
        [Test] public void Bonus_FractionalPercentPerLevel_KeepsOneDecimal()
        {
            // DECISION-0093: +4.5% per level must not display as +5%.
            var catalog = MetaCatalog.Load();
            var damage = catalog.Upgrades.Values.Single(u => u.Stat == "damage");
            var reduction = catalog.Upgrades.Values.Single(u => u.Stat == "damageReduction");
            Assert.AreEqual("+4,5%", MetaShopProjection.Bonus(damage, 1));
            Assert.AreEqual("+45%", MetaShopProjection.Bonus(damage, 10));
            Assert.AreEqual("+3 п.п.", MetaShopProjection.Bonus(reduction, 1));
        }
        [Test] public void ShopAssets_UseSemanticIdsAndNoCharacterDropdown()
        {
            var root = Resources.Load<VisualTreeAsset>("UI/MetaShop").CloneTree();
            foreach (var id in new[] { GameplayUiElementIds.MetaShop, GameplayUiElementIds.MetaWallet, GameplayUiElementIds.MetaRoster,
                GameplayUiElementIds.MetaPortrait, GameplayUiElementIds.MetaUpgradeList, GameplayUiElementIds.MetaUnlockList,
                GameplayUiElementIds.MetaRefund, GameplayUiElementIds.MetaRefundConfirm, GameplayUiElementIds.MetaRefundCancel })
                Assert.IsNotNull(root.Q(id), id);
            Assert.AreEqual(GameplayUiElementIds.MetaUnlockState, root.Q<DropdownField>().name);
            Assert.IsNotNull(Resources.Load<StyleSheet>("UI/MetaShopStyles"));
        }
        [Test] public async Task Unlocks_FreshProfile_IncludesInitialAndKeepsOnlyCharactersHidden()
        {
            var catalog = MetaCatalog.Load(); var profile = new ProfileService(catalog, new MemoryProfileStore());
            await profile.LoadAsync();
            var view = new FakeMetaView(); using var presenter = new MetaPresenter(profile, view, new FakeProfileNavigation());
            view.Shop(); var cards = view.State.Cards.Where(c => c.Cap == 0).ToArray();
            Assert.AreEqual(catalog.Unlocks.Count, cards.Length);
            Assert.AreEqual(70, cards.Length);
            CollectionAssert.AreEquivalent(new[] { "SKILL-001", "SKILL-002", "SKILL-003", "SKILL-004", "SKILL-005", "SKILL-006", "SKILL-007", "SKILL-010", "SKILL-013", "SKILL-014" }, cards.Where(c => c.Kind == "skill" && c.Owned).Select(c => c.Id));
            CollectionAssert.AreEquivalent(new[] { "PASSIVE-001", "PASSIVE-002", "PASSIVE-003", "PASSIVE-004", "PASSIVE-005", "PASSIVE-007", "PASSIVE-008", "PASSIVE-009", "PASSIVE-011", "PASSIVE-012" }, cards.Where(c => c.Kind == "passive" && c.Owned).Select(c => c.Id));
            CollectionAssert.AreEquivalent(new[] { "SET-001", "SET-004", "SET-006", "SET-010", "SET-017" }, cards.Where(c => c.Kind == "set" && c.Owned).Select(c => c.Id));
            foreach (var card in cards)
            {
                Assert.AreEqual(profile.IsUnlocked(card.Id), card.Owned);
                Assert.AreEqual(card.Kind == "character" && !card.Owned, card.HiddenCharacter);
                Assert.AreEqual(card.HiddenCharacter ? "?" : catalog.Unlocks[card.Id].Name, card.Text);
            }
            Assert.AreEqual(10, cards.Count(c => MetaShopProjection.MatchesUnlock(c, "field", 0)));
            Assert.AreEqual(20, cards.Count(c => MetaShopProjection.MatchesUnlock(c, "set", 0)));
            Assert.AreEqual(30, cards.Count(c => MetaShopProjection.MatchesUnlock(c, "ability", 0)));
            Assert.AreEqual(cards.Count(c => c.Owned), cards.Count(c => MetaShopProjection.MatchesUnlock(c, "all", 3)));
            Assert.AreEqual(cards.Count(c => !c.Owned), cards.Count(c => MetaShopProjection.MatchesUnlock(c, "all", 1)));
            Assert.AreEqual(0, cards.Count(c => MetaShopProjection.MatchesUnlock(c, "all", 2)));
        }
    }
}
