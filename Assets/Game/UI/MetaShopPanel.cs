using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
namespace Game.UI
{
    internal sealed class MetaShopPanel
    {
        public VisualElement Root { get; }
        private readonly Action<string> _select;
        private readonly Action<MetaCardViewState> _buy;
        private readonly Action _cancel;
        private bool _unlocks;
        private bool _modal;
        public MetaShopPanel(Action<string> select, Action<MetaCardViewState> buy, Action<bool> toggle,
            Action refund, Action confirm, Action cancel)
        {
            _select = select; _buy = buy; _cancel = cancel;
            Root = Resources.Load<VisualTreeAsset>("UI/MetaShop").CloneTree();
            Root.AddToClassList("shop-host");
            Root.Q<Button>(GameplayUiElementIds.MetaTabUpgrades).clicked += () => Tab(false);
            Root.Q<Button>(GameplayUiElementIds.MetaTabUnlocks).clicked += () => Tab(true);
            Root.Q<Toggle>(GameplayUiElementIds.MetaShopToggle).RegisterValueChangedCallback(e => toggle(e.newValue));
            Root.Q<Button>(GameplayUiElementIds.MetaRefund).clicked += refund;
            Root.Q<Button>(GameplayUiElementIds.MetaRefundConfirm).clicked += confirm;
            Root.Q<Button>(GameplayUiElementIds.MetaRefundCancel).clicked += cancel;
            Root.Q(GameplayUiElementIds.MetaRefundModal).RegisterCallback<KeyDownEvent>(e =>
            { if (e.keyCode == KeyCode.Escape) { e.StopPropagation(); _cancel(); }
                else if (e.keyCode == KeyCode.Tab) { e.PreventDefault(); e.StopPropagation();
                    var focused = Root.panel?.focusController?.focusedElement;
                    Root.Q<Button>(focused == Root.Q<Button>(GameplayUiElementIds.MetaRefundCancel) ?
                        GameplayUiElementIds.MetaRefundConfirm : GameplayUiElementIds.MetaRefundCancel).Focus(); } });
            Tab(false);
        }
        private static void Hidden(VisualElement element, bool hidden) => element.EnableInClassList("shop-hidden", hidden);
        private void Tab(bool unlocks)
        {
            if (_modal) return;
            _unlocks = unlocks;
            Hidden(Root.Q(GameplayUiElementIds.MetaHeroPane), unlocks);
            Hidden(Root.Q(GameplayUiElementIds.MetaUpgradePane), unlocks);
            Hidden(Root.Q(GameplayUiElementIds.MetaUnlockList), !unlocks);
            Hidden(Root.Q(GameplayUiElementIds.MetaRefund), unlocks);
            Hidden(Root.Q(GameplayUiElementIds.MetaRefundReason), unlocks);
            Root.Q(GameplayUiElementIds.MetaTabUpgrades).EnableInClassList("shop-selected", !unlocks);
            Root.Q(GameplayUiElementIds.MetaTabUnlocks).EnableInClassList("shop-selected", unlocks);
        }
        public void Render(MetaViewState state)
        {
            Hidden(Root, state.Shop == null);
            if (state.Shop == null) { _modal = false; _unlocks = false; return; }
            var shop = state.Shop;
            var focus = (Root.panel?.focusController?.focusedElement as VisualElement)?.name;
            Root.Q<Label>(GameplayUiElementIds.MetaWallet).text = shop.Currency.ToString("N0") + " золота";
            Root.Q<Label>(GameplayUiElementIds.MetaInvested).text = "Вложено\n" + shop.Invested.ToString("N0") + " золота";
            var hero = shop.Heroes.FirstOrDefault(h => h.Id == state.SelectedCharacter);
            Root.Q<Label>(GameplayUiElementIds.MetaHeroName).text = hero?.Name ?? "Нет открытых героев";
            Root.Q<Image>(GameplayUiElementIds.MetaPortrait).sprite = hero?.Icon;
            var roster = Root.Q<ScrollView>(GameplayUiElementIds.MetaRoster);
            var rosterOffset = roster.scrollOffset; roster.Clear();
            foreach (var item in shop.Heroes)
            {
                var button = new Button(() => _select(item.Id)) { name = GameplayUiElementIds.MetaHero(item.Id), tooltip = item.Name };
                button.AddToClassList("shop-hero-button"); button.EnableInClassList("shop-selected", item.Id == state.SelectedCharacter);
                button.Add(new Image { sprite = item.Icon, pickingMode = PickingMode.Ignore });
                if (item.Icon == null) button.Add(new Label(item.Name));
                button.SetEnabled(state.CanContinue && !shop.ConfirmRefund); roster.Add(button);
            }
            roster.scrollOffset = rosterOffset;
            var toggle = Root.Q<Toggle>(GameplayUiElementIds.MetaShopToggle);
            toggle.SetValueWithoutNotify(state.UpgradesDisabled); toggle.SetEnabled(state.CanToggleUpgrades && !shop.ConfirmRefund);
            var list = Root.Q<ScrollView>(GameplayUiElementIds.MetaUpgradeList);
            var offset = list.scrollOffset; list.Clear();
            foreach (var card in state.Cards.Where(c => c.Cap > 0))
            {
                var row = new VisualElement { name = GameplayUiElementIds.MetaCard(card.Id) }; row.AddToClassList("shop-card");
                var heading = new VisualElement(); heading.AddToClassList("shop-card-heading");
                heading.Add(Label(card.Text, "shop-card-title")); heading.Add(Label(card.Level + " / " + card.Cap, "shop-level")); row.Add(heading);
                var bars = new VisualElement(); bars.AddToClassList("shop-bars");
                for (var i = 0; i < card.Cap; i++) { var bar = new VisualElement(); bar.AddToClassList("shop-bar"); bar.EnableInClassList("shop-filled", i < card.Level); bars.Add(bar); }
                row.Add(bars); row.Add(Label(card.Bonus + (card.NextBonus == null ? "" : " → " + card.NextBonus), "shop-bonus"));
                var actions = new VisualElement(); actions.AddToClassList("shop-card-actions");
                actions.Add(Label(card.Level == card.Cap ? "Максимум" : card.Price.ToString("N0") + " ◈", "shop-price"));
                var buy = new Button(() => _buy(card)) { name = GameplayUiElementIds.MetaBuy(card.Id), text = "Улучшить" };
                buy.AddToClassList("shop-primary"); buy.SetEnabled(card.CanBuy && !shop.ConfirmRefund); actions.Add(buy); row.Add(actions);
                var detail = Label(card.Detail, "shop-reason"); Hidden(detail, string.IsNullOrEmpty(card.Detail)); row.Add(detail); list.Add(row);
            }
            list.scrollOffset = offset;
            var collection = Root.Q<ScrollView>(GameplayUiElementIds.MetaUnlockList);
            var collectionOffset = collection.scrollOffset; collection.Clear();
            foreach (var group in state.Cards.Where(c => c.Cap == 0).GroupBy(c => c.Group))
            {
                collection.Add(Label(group.Key, "shop-group-title"));
                foreach (var card in group)
                {
                    var row = new VisualElement(); row.AddToClassList("shop-unlock");
                    var icon = new Image { sprite = card.Icon }; icon.AddToClassList("shop-unlock-icon");
                    if (card.HiddenCharacter) icon.tintColor = Color.black;
                    row.Add(icon); var copy = new VisualElement(); copy.AddToClassList("shop-unlock-copy");
                    copy.Add(Label(card.Text, "shop-card-title")); copy.Add(Label(card.Detail, "shop-reason")); row.Add(copy);
                    if (card.Price > 0) { var buy = new Button(() => _buy(card)) { text = "Открыть · " + card.Price, name = GameplayUiElementIds.MetaBuy(card.Id) }; buy.SetEnabled(card.CanBuy && !shop.ConfirmRefund); row.Add(buy); }
                    collection.Add(row);
                }
            }
            collection.scrollOffset = collectionOffset;
            Root.Q<Label>(GameplayUiElementIds.MetaRefundReason).text = shop.RefundReason ?? "Комиссия " + shop.RefundFee.ToString("N0") + " золота";
            Root.Q<Button>(GameplayUiElementIds.MetaRefund).SetEnabled(shop.RefundReason == null && !shop.ConfirmRefund);
            Root.Q<Button>(GameplayUiElementIds.MetaTabUpgrades).SetEnabled(!shop.ConfirmRefund);
            Root.Q<Button>(GameplayUiElementIds.MetaTabUnlocks).SetEnabled(!shop.ConfirmRefund);
            var wasModal = _modal; _modal = shop.ConfirmRefund;
            Hidden(Root.Q(GameplayUiElementIds.MetaRefundModal), !_modal);
            if (_modal)
            {
                Root.Q<Label>(GameplayUiElementIds.MetaRefundDetail).text = (hero?.Name ?? "") + "\nВернётся: " + shop.Invested.ToString("N0") +
                    "\nКомиссия: −" + shop.RefundFee.ToString("N0") + "\nЗолото после сброса: " +
                    checked(shop.Currency + shop.Invested - shop.RefundFee).ToString("N0");
                if (!wasModal) Root.Q<Button>(GameplayUiElementIds.MetaRefundCancel).Focus();
            }
            else if (wasModal) Root.Q<Button>(GameplayUiElementIds.MetaRefund).Focus();
            else if (!string.IsNullOrEmpty(focus)) Root.Q(focus)?.Focus();
            if (!_modal) Tab(_unlocks);
        }
        private static Label Label(string text, string css) { var label = new Label(text); label.AddToClassList(css); return label; }
    }
}
