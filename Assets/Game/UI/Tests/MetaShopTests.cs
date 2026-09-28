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
            Assert.IsTrue(view.State.Cards.Where(c => c.Cap > 0).All(c => c.Icon == null));
            Assert.IsTrue(view.State.Cards.Where(c => c.HiddenCharacter).All(c => c.Text == "?"));
        }
        [Test] public void ShopAssets_UseSemanticIdsAndNoCharacterDropdown()
        {
            var root = Resources.Load<VisualTreeAsset>("UI/MetaShop").CloneTree();
            foreach (var id in new[] { GameplayUiElementIds.MetaShop, GameplayUiElementIds.MetaWallet, GameplayUiElementIds.MetaRoster,
                GameplayUiElementIds.MetaPortrait, GameplayUiElementIds.MetaUpgradeList, GameplayUiElementIds.MetaUnlockList,
                GameplayUiElementIds.MetaRefund, GameplayUiElementIds.MetaRefundConfirm, GameplayUiElementIds.MetaRefundCancel })
                Assert.IsNotNull(root.Q(id), id);
            Assert.IsNull(root.Q<DropdownField>());
            Assert.IsNotNull(Resources.Load<StyleSheet>("UI/MetaShopStyles"));
        }
    }
}
