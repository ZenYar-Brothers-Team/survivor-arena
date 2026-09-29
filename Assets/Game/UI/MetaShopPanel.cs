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
        private readonly MetaUnlockPanel _collection;
        public MetaShopPanel(Action<string> select, Action<MetaCardViewState> buy, Action<bool> toggle,
            Action refund, Action confirm, Action cancel)
        {
            _select = select; _buy = buy; _cancel = cancel;
            Root = Resources.Load<VisualTreeAsset>("UI/MetaShop").CloneTree();
            Root.AddToClassList("shop-host");
            _collection = new MetaUnlockPanel(Root, buy);
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
            Hidden(Root.Q(GameplayUiElementIds.MetaUnlockPane), !unlocks);
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
            var rosterOffset = roster.scrollOffset;
            var heroNames = shop.Heroes.Select(h => GameplayUiElementIds.MetaHero(h.Id)).ToArray();
            if (!roster.contentContainer.Children().Select(e => e.name).SequenceEqual(heroNames)) roster.Clear();
            foreach (var item in shop.Heroes)
            {
                var button = roster.Q<Button>(GameplayUiElementIds.MetaHero(item.Id));
                if (button == null)
                {
                    button = new Button(() => _select(item.Id)) { name = GameplayUiElementIds.MetaHero(item.Id), tooltip = item.Name };
                    button.AddToClassList("shop-hero-button");
                    button.Add(new Image { pickingMode = PickingMode.Ignore }); roster.Add(button);
                }
                button.EnableInClassList("shop-selected", item.Id == state.SelectedCharacter);
                button.Q<Image>().sprite = item.Icon;
                button.SetEnabled(state.CanContinue && !shop.ConfirmRefund);
            }
            roster.scrollOffset = rosterOffset;
            var toggle = Root.Q<Toggle>(GameplayUiElementIds.MetaShopToggle);
            toggle.SetValueWithoutNotify(state.UpgradesDisabled); toggle.SetEnabled(state.CanToggleUpgrades && !shop.ConfirmRefund);
            var list = Root.Q<ScrollView>(GameplayUiElementIds.MetaUpgradeList);
            var offset = list.scrollOffset;
            var upgrades = state.Cards.Where(c => c.Cap > 0).ToArray();
            if (!list.contentContainer.Children().Select(e => e.name).SequenceEqual(upgrades.Select(c => GameplayUiElementIds.MetaCard(c.Id)))) list.Clear();
            foreach (var card in upgrades)
            {
                var row = list.Q<VisualElement>(GameplayUiElementIds.MetaCard(card.Id));
                if (row == null)
                {
                    row = new VisualElement { name = GameplayUiElementIds.MetaCard(card.Id) }; row.AddToClassList("shop-card");
                    var heading = new VisualElement(); heading.AddToClassList("shop-card-heading");
                    var icon = new Image { name = GameplayUiElementIds.MetaUpgradeIcon(card.Id), pickingMode = PickingMode.Ignore };
                    icon.AddToClassList("shop-stat-icon"); heading.Add(icon);
                    heading.Add(Label(card.Text, "shop-card-title")); heading.Add(Label("", "shop-level")); row.Add(heading);
                    var bars = new VisualElement(); bars.AddToClassList("shop-bars");
                    for (var i = 0; i < card.Cap; i++) { var bar = new VisualElement(); bar.AddToClassList("shop-bar"); bars.Add(bar); }
                    row.Add(bars); row.Add(Label("", "shop-bonus"));
                    var actions = new VisualElement(); actions.AddToClassList("shop-card-actions"); actions.Add(Label("", "shop-price"));
                    var target = row;
                    var buy = new Button(() => _buy((MetaCardViewState)target.userData)) { name = GameplayUiElementIds.MetaBuy(card.Id), text = "Улучшить" };
                    buy.AddToClassList("shop-primary"); actions.Add(buy); row.Add(actions);
                    row.Add(Label(" ", "shop-reason")); list.Add(row);
                }
                row.userData = card;
                row.Q<Image>().sprite = card.Icon;
                Hidden(row.Q<Image>(), card.Icon == null);
                row.Q(className: "shop-card-heading").EnableInClassList("shop-has-stat-icon", card.Icon != null);
                row.Q<Label>(className: "shop-level").text = card.Level + " / " + card.Cap;
                var segments = row.Q(className: "shop-bars");
                for (var i = 0; i < segments.childCount; i++) segments[i].EnableInClassList("shop-filled", i < card.Level);
                row.Q<Label>(className: "shop-bonus").text = card.Bonus + (card.NextBonus == null ? "" : " → " + card.NextBonus);
                row.Q<Label>(className: "shop-price").text = card.Level == card.Cap ? "Максимум" : card.Price.ToString("N0") + " ◈";
                row.Q<Button>().SetEnabled(card.CanBuy && !shop.ConfirmRefund);
                row.Q<Label>(className: "shop-reason").text = string.IsNullOrEmpty(card.Detail) ? " " : card.Detail;
            }
            list.scrollOffset = offset;
            _collection.Render(state);
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
