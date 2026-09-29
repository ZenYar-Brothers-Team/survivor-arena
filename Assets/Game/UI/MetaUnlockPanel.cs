using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
namespace Game.UI
{
    internal sealed class MetaUnlockPanel
    {
        private readonly VisualElement _root;
        private readonly Action<MetaCardViewState> _buy;
        private readonly DropdownField _stateFilter;
        private readonly ScrollView _list;
        private MetaCardViewState[] _cards = Array.Empty<MetaCardViewState>();
        private string _kind = "all";
        private bool _blocked;
        public MetaUnlockPanel(VisualElement root, Action<MetaCardViewState> buy)
        {
            _root = root; _buy = buy;
            _list = root.Q<ScrollView>(GameplayUiElementIds.MetaUnlockList);
            _stateFilter = root.Q<DropdownField>(GameplayUiElementIds.MetaUnlockState);
            _stateFilter.choices = new System.Collections.Generic.List<string> { "Все состояния", "Не открыто", "Можно купить", "Открыто" };
            _stateFilter.SetValueWithoutNotify(_stateFilter.choices[0]);
            _stateFilter.RegisterValueChangedCallback(_ => Rebuild(true));
            var filters = root.Q(GameplayUiElementIds.MetaUnlockFilters);
            var kinds = new[] { "all", "character", "field", "ability", "set" };
            var titles = new[] { "Все", "Персонажи", "Карты", "Умения", "Сеты" };
            for (var i = 0; i < kinds.Length; i++)
            {
                var kind = kinds[i];
                var button = new Button(() => { _kind = kind; Rebuild(true); })
                    { name = GameplayUiElementIds.MetaUnlockType(kind), text = titles[i] };
                button.AddToClassList("shop-type-filter"); filters.Insert(i, button);
            }
        }
        public void Render(MetaViewState state)
        {
            _cards = state.Cards.Where(c => c.Cap == 0).ToArray();
            _blocked = state.Shop.ConfirmRefund;
            Rebuild(false);
        }
        private void Rebuild(bool reset)
        {
            var focus = (_root.panel?.focusController?.focusedElement as VisualElement)?.name;
            var offset = reset ? Vector2.zero : _list.scrollOffset;
            var selectedState = Math.Max(0, _stateFilter.index);
            var category = _cards.Where(c => MetaShopProjection.MatchesUnlock(c, _kind, 0)).ToArray();
            _stateFilter.choices = new System.Collections.Generic.List<string>
            {
                "Все состояния · " + category.Length,
                "Не открыто · " + category.Count(c => !c.Owned),
                "Можно купить · " + category.Count(c => c.CanBuy),
                "Открыто · " + category.Count(c => c.Owned)
            };
            _stateFilter.SetValueWithoutNotify(_stateFilter.choices[selectedState]);
            var visible = _cards.Where(c => MetaShopProjection.MatchesUnlock(c, _kind, _stateFilter.index)).ToArray();
            _list.Clear();
            foreach (var button in _root.Q(GameplayUiElementIds.MetaUnlockFilters).Children().OfType<Button>())
                button.EnableInClassList("shop-selected", button.name == GameplayUiElementIds.MetaUnlockType(_kind));
            foreach (var group in visible.GroupBy(c => c.Group))
            {
                _list.Add(Label(group.Key + "  ·  " + group.Count(c => c.Owned) + " / " + group.Count(), "shop-group-title"));
                var grid = new VisualElement(); grid.AddToClassList("shop-unlock-grid");
                foreach (var card in group)
                {
                    var row = new VisualElement { name = GameplayUiElementIds.MetaCard(card.Id) };
                    row.AddToClassList("shop-unlock"); row.EnableInClassList("shop-unlock-available", card.CanBuy);
                    var art = new VisualElement(); art.AddToClassList("shop-unlock-art");
                    var icon = new Image { sprite = card.Icon }; icon.AddToClassList("shop-unlock-icon");
                    if (card.HiddenCharacter) icon.tintColor = Color.black;
                    if (card.Icon != null) art.Add(icon);
                    else if (card.Kind == "field") art.Add(Label(card.Id.Substring(card.Id.LastIndexOf('-') + 1).TrimStart('0'), "shop-map-number"));
                    row.Add(art);
                    var copy = new VisualElement(); copy.AddToClassList("shop-unlock-copy");
                    copy.Add(Label(MetaShopProjection.Kind(card.Kind), "shop-unlock-kind"));
                    copy.Add(Label(card.Text, "shop-card-title"));
                    var detail = Label(card.Detail, "shop-reason"); detail.EnableInClassList("shop-owned", card.Owned); copy.Add(detail);
                    if (card.Price > 0)
                    {
                        var actions = new VisualElement(); actions.AddToClassList("shop-unlock-actions");
                        actions.Add(Label(card.Price.ToString("N0") + " золота", "shop-price"));
                        var buy = new Button(() => _buy(card)) { text = "Открыть", name = GameplayUiElementIds.MetaBuy(card.Id) };
                        buy.AddToClassList("shop-primary"); buy.SetEnabled(card.CanBuy && !_blocked); actions.Add(buy); copy.Add(actions);
                    }
                    row.Add(copy); grid.Add(row);
                }
                _list.Add(grid);
            }
            if (visible.Length == 0) _list.Add(Label("Под этот фильтр пока ничего не подходит.", "shop-unlock-empty"));
            _root.Q<Label>(GameplayUiElementIds.MetaUnlockTotal).text = "В категории открыто " + category.Count(c => c.Owned) + " / " + category.Length + " · показано " + visible.Length;
            _list.scrollOffset = offset;
            if (!string.IsNullOrEmpty(focus) && focus.StartsWith("meta-buy-"))
            {
                var target = _root.Q<Button>(focus);
                if (target != null && target.enabledInHierarchy) target.Focus(); else _list.Focus();
            }
        }
        private static Label Label(string text, string css) { var label = new Label(text); label.AddToClassList(css); return label; }
    }
}
