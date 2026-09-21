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
            var health=view.State.Cards.Single(c=>c.Id=="META-001");StringAssert.Contains("100 currency",health.Detail);Assert.IsFalse(health.CanBuy);
            var condition=view.State.Cards.Single(c=>c.Id=="SKILL-016");StringAssert.Contains("15:00",condition.Detail);Assert.IsFalse(condition.CanBuy);
            view.Close();Assert.AreEqual(1,navigation.Selections);Assert.IsFalse(view.State.Visible);
            presenter.Dispose();view.Shop();Assert.IsFalse(view.State.Visible);
        }
        [Test] public void Uxml_AllSemanticElementsExist()
        {
            var tree=Resources.Load<VisualTreeAsset>("UI/MetaScreen");Assert.IsNotNull(tree);var root=tree.CloneTree();
            foreach(var name in new[]{GameplayUiElementIds.MetaBody,GameplayUiElementIds.MetaCards,GameplayUiElementIds.MetaCharacter,
                GameplayUiElementIds.MetaOpen,GameplayUiElementIds.MetaClose,GameplayUiElementIds.MetaRetry,GameplayUiElementIds.MetaSelection,
                GameplayUiElementIds.MetaQuit,GameplayUiElementIds.MetaSave,GameplayUiElementIds.MetaReset,GameplayUiElementIds.MetaTitle,
                GameplayUiElementIds.MetaSummary,GameplayUiElementIds.MetaMessage})Assert.IsNotNull(root.Q(name),name);
            Assert.IsNotNull(Resources.Load<StyleSheet>("UI/MetaScreenStyles"));
        }
    }
}
