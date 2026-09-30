using System.Linq;
using System.Threading.Tasks;
using Game.Meta;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
namespace Game.UI.Tests
{
    public sealed class MetaPresenterTests
    {
        [Test] public async Task Shop_ProjectsPricesConditionsAndDisabledBuy_FromProfile()
        {
            var profile=new ProfileService(MetaCatalog.Load(),new MemoryProfileStore());await profile.LoadAsync();
            var view=new FakeMetaView();var navigation=new FakeProfileNavigation();using var presenter=new MetaPresenter(profile,view,navigation);
            Assert.IsFalse(view.State.Visible);view.Shop();Assert.IsTrue(view.State.Visible);
            var health=view.State.Cards.Single(c=>c.Id=="META-003");Assert.AreEqual(100,health.Price);Assert.IsFalse(health.CanBuy);
            var condition=view.State.Cards.Single(c=>c.Id=="SKILL-016");StringAssert.Contains("Открыть карту",condition.Detail);Assert.IsFalse(condition.CanBuy);
            view.Close();Assert.AreEqual(1,navigation.Selections);Assert.IsFalse(view.State.Visible);
            presenter.Dispose();view.Shop();Assert.IsFalse(view.State.Visible);
        }
        [Test] public void UnlockCollection_FormatsAchievementProgressAndPaidPrice()
        {
            var catalog = MetaCatalog.Load();
            StringAssert.Contains("200/500", MetaShopProjection.Condition(catalog.Unlocks["FIELD-002"], catalog, 200));
            Assert.AreEqual(200, catalog.Unlocks["SET-020"].Price);
        }
        [Test] public async Task UpgradesToggle_OnlyInShop_DisablesUpgradesAndMarksCards()
        {
            var profile=new ProfileService(MetaCatalog.Load(),new MemoryProfileStore());await profile.LoadAsync();
            var view=new FakeMetaView();using var presenter=new MetaPresenter(profile,view,new FakeProfileNavigation());
            Assert.IsFalse(view.State.ShowUpgradesToggle);
            view.Shop();Assert.IsTrue(view.State.ShowUpgradesToggle);Assert.IsTrue(view.State.CanToggleUpgrades);Assert.IsFalse(view.State.UpgradesDisabled);
            Assert.AreEqual("Не хватает монет",view.State.Cards.Single(c=>c.Id=="META-003").Detail);
            view.DisableUpgrades(true);await Task.Yield();
            Assert.IsTrue(profile.UpgradesDisabled);Assert.IsTrue(view.State.UpgradesDisabled);
            Assert.AreEqual("Не хватает монет",view.State.Cards.Single(c=>c.Id=="META-003").Detail);
            Assert.IsNotNull(view.State.Shop); Assert.AreEqual(12,view.State.Cards.Count(c=>c.Cap>0));
            StringAssert.Contains("without meta bonuses",view.State.Summary);
            view.DisableUpgrades(false);await Task.Yield();
            Assert.IsFalse(profile.UpgradesDisabled);Assert.AreEqual("Здоровье",view.State.Cards.Single(c=>c.Id=="META-003").Text);
        }
        [Test] public async Task UpgradesToggle_WhileSaving_KeepsPurchaseReasonStable()
        {
            var catalog=MetaCatalog.Load();var codec=new ProfileCodec(catalog);var store=new DelayedProfileStore(codec.Encode(codec.Create()));
            var profile=new ProfileService(catalog,store);await profile.LoadAsync();
            var view=new FakeMetaView();using var presenter=new MetaPresenter(profile,view,new FakeProfileNavigation());
            view.Shop();Assert.AreEqual("Не хватает монет",view.State.Cards.Single(c=>c.Id=="META-003").Detail);
            view.DisableUpgrades(true);
            Assert.AreEqual(ProfileState.Saving,profile.State);
            Assert.AreEqual("Не хватает монет",view.State.Cards.Single(c=>c.Id=="META-003").Detail);
            store.CompleteWrite();await Task.Yield();await Task.Yield();
            Assert.AreEqual(ProfileState.Ready,profile.State);
            Assert.AreEqual("Не хватает монет",view.State.Cards.Single(c=>c.Id=="META-003").Detail);
        }
        [Test] public void Uxml_AllSemanticElementsExist()
        {
            var tree=Resources.Load<VisualTreeAsset>("UI/MetaScreen");Assert.IsNotNull(tree);var root=tree.CloneTree();
            foreach(var name in new[]{GameplayUiElementIds.MetaBody,GameplayUiElementIds.MetaCards,GameplayUiElementIds.MetaCharacter,
                GameplayUiElementIds.MetaOpen,GameplayUiElementIds.MetaClose,GameplayUiElementIds.MetaRetry,GameplayUiElementIds.MetaSelection,
                GameplayUiElementIds.MetaQuit,GameplayUiElementIds.MetaSave,GameplayUiElementIds.MetaReset,GameplayUiElementIds.MetaTitle,
                GameplayUiElementIds.MetaSummary,GameplayUiElementIds.MetaMessage,GameplayUiElementIds.MetaUpgradesDisabled})Assert.IsNotNull(root.Q(name),name);
            Assert.IsNotNull(Resources.Load<StyleSheet>("UI/MetaScreenStyles"));
        }
    }
}
