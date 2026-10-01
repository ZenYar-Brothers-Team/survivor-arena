using System;
using System.Collections.Generic;
using System.Linq;
using Game.Meta;
using Game.Run;
namespace Game.UI
{
    public sealed class MetaPresenter : IDisposable
    {
        private readonly IProfileService _profile;
        private readonly IMetaView _view;
        private readonly IProfileNavigation _navigation;
        private readonly Func<Game.Content.ContentRegistry> _registry;
        private bool _shop;
        private bool _disposed;
        private string _character;
        private RunOutcome _result;
        private string _refundCharacter;
        private long _refundInvestment;
        public MetaPresenter(IProfileService profile, IMetaView view, IProfileNavigation navigation, Func<Game.Content.ContentRegistry> registry = null)
        {
            _profile = profile; _view = view; _navigation = navigation;
            _registry = registry;
            profile.Changed += Refresh;
            view.ShopRequested += Shop; view.CloseRequested += Close; view.RetryRequested += Retry;
            view.SelectionRequested += Selection; view.QuitRequested += Quit; view.SaveRequested += Save;
            view.ResetRequested += Reset; view.CharacterRequested += Character; view.PurchaseRequested += Purchase;
            view.UpgradesDisabledRequested += UpgradesDisabled;
            view.RefundRequested += RequestRefund; view.RefundConfirmed += ConfirmRefund; view.RefundCancelled += CancelRefund;
            Refresh();
        }
        public void ShowResult(RunOutcome outcome) { _result = outcome; _shop = false; _refundCharacter = null; Refresh(); }
        public void ClearResult() { _result = null; _shop = false; _refundCharacter = null; Refresh(); }
        public void OpenShop() => Shop();
        private void Shop() { if (_profile.CanStart) { _shop = true; Refresh(); } }
        private void Close() { if (!_profile.CanStart) return; _shop = false; _refundCharacter = null; if (_result == null) _navigation.ReturnToProfileSelection(); Refresh(); }
        private void Retry() { if (_profile.CanStart && _result != null) _navigation.RetryProfileRun(); }
        private void Selection() { if (_profile.CanStart) _navigation.ReturnToProfileSelection(); }
        private void Quit() { if (_profile.RunActive) _navigation.QuitProfileRun(); }
        private async void Save() { if (_profile.State == ProfileState.LoadError) await _profile.LoadAsync(); else await _profile.RetrySaveAsync(); if (!_disposed && _result == null && _profile.CanStart) _navigation.ReturnToProfileSelection(); }
        private async void Reset() { await _profile.ResetAsync(); if (!_disposed && _profile.CanStart) _navigation.ReturnToProfileSelection(); }
        private void Character(string id) { if (_refundCharacter == null && _profile.CanStart && _profile.Catalog.Unlocks.TryGetValue(id, out var rule) && rule.Kind == "character" && _profile.IsUnlocked(id)) { _character = id; Refresh(); } }
        private async void Purchase(MetaCardViewState card)
        {
            if (!_shop || _refundCharacter != null || (card.Character != null && card.Character != _character)) return;
            await _profile.PurchaseAsync(card.Id, card.Level, card.Character);
        }
        private void RequestRefund()
        {
            if (!_shop || _profile.RefundLockReason(_character) != null) return;
            _refundCharacter = _character; _refundInvestment = _profile.Invested(_character); Refresh();
        }
        private void CancelRefund() { _refundCharacter = null; Refresh(); }
        private async void ConfirmRefund()
        {
            if (_refundCharacter == null) return;
            var owner = _refundCharacter; var spent = _refundInvestment; _refundCharacter = null;
            await _profile.RefundAsync(owner, spent); Refresh();
        }
        // A refused change still re-renders so the view toggle snaps back to the profile value.
        private async void UpgradesDisabled(bool disabled) { if (!await _profile.SetUpgradesDisabledAsync(disabled)) Refresh(); }
        private void Refresh()
        {
            if (_disposed) return;
            var characters = _profile.Catalog.Unlocks.Values.Where(r => r.Kind == "character" && _profile.IsUnlocked(r.Id)).Select(r => r.Id).ToList();
            if (!characters.Contains(_character)) _character = characters.FirstOrDefault();
            var cards = new List<MetaCardViewState>();
            if (_shop)
            {
                foreach (var upgrade in _profile.Catalog.Upgrades.Values.OrderBy(u => MetaShopProjection.UpgradeOrder(u.Stat)).ThenBy(u => u.Id, StringComparer.Ordinal))
                {
                    var owner = upgrade.Personal ? _character : null;
                    var level = _profile.Level(upgrade.Id, owner);
                    var reason = _profile.PurchaseLockReason(upgrade.Id, owner);
                    var displayReason = reason;
                    if (_profile.State == ProfileState.Saving && reason == "Save the profile first")
                        displayReason = level >= upgrade.Cap ? "Maximum level" :
                            _profile.Currency < upgrade.Price(level) ? "Not enough currency" : null;
                    cards.Add(new MetaCardViewState(upgrade.Id, owner, level,
                        upgrade.Name, MetaShopProjection.Reason(displayReason) ?? (_profile.UpgradesDisabled ? "Не действует" : ""),
                        reason == null, upgrade.Cap, level < upgrade.Cap ? upgrade.Price(level) : 0,
                        MetaShopProjection.Bonus(upgrade, level), level < upgrade.Cap ? MetaShopProjection.Bonus(upgrade, level + 1) : null,
                        icon: MetaShopProjection.UpgradeIcon(upgrade.Id, _registry?.Invoke())));
                }
                foreach (var rule in _profile.Catalog.Unlocks.Values)
                {
                    if (rule.Condition == "dev") continue; // development-only fields are not collection content
                    var reason = _profile.PurchaseLockReason(rule.Id);
                    var content = RunResultsProjection.Content(rule.Id, _profile.Catalog, _registry?.Invoke());
                    var hidden = rule.Kind == "character" && !_profile.IsUnlocked(rule.Id);
                    var group = rule.Kind == "character" ? "Персонажи" : rule.Kind == "field" ? "Карты" :
                        rule.RequiredId != null && _profile.Catalog.Unlocks.TryGetValue(rule.RequiredId, out var field) ? "Карта · " + field.Name : "Доступно с начала";
                    var hiddenField = rule.Kind == "field" && !_profile.IsUnlocked(rule.Id);
                    cards.Add(new MetaCardViewState(rule.Id, null, 0, hidden || hiddenField ? "?" : content.Name,
                        _profile.IsUnlocked(rule.Id) ? "✓ Открыто" : (reason == null ? "Можно открыть" : MetaShopProjection.Condition(rule, _profile.Catalog, _profile.UnlockProgress(rule.Id))) +
                        (reason == "Not enough currency" ? " · Не хватает монет" : ""),
                        rule.Price > 0 && reason == null, price: _profile.IsUnlocked(rule.Id) ? 0 : rule.Price,
                        group: group, icon: content.Icon, hiddenCharacter: hidden, kind: rule.Kind,
                        owned: _profile.IsUnlocked(rule.Id), hiddenField: hiddenField));
                }
            }
            var summary = "Currency: " + _profile.Currency;
            if (_shop && _profile.UpgradesDisabled) summary += "\nPermanent upgrades are disabled: runs start without meta bonuses.";
            var error = _profile.State == ProfileState.LoadError;
            _view.Render(new MetaViewState(_shop || _result != null || error || _profile.State == ProfileState.Loading || _profile.State == ProfileState.NotLoaded,
                error ? "Profile unavailable" : _shop ? "Meta progression" : _result != null ? "Run results" : "Loading profile",
                summary, _profile.Message ?? (_profile.State == ProfileState.Saving ? "Saving…" : ""),
                _profile.CanStart, _profile.CanStart, _result != null && !_shop, error, _profile.CanReset, _profile.RunActive,
                cards, characters, _character, _shop, _profile.UpgradesDisabled, _shop && _profile.UpgradesToggleLockReason == null,
                _result != null && !_shop ? RunResultsProjection.Create(_result, _profile, _registry?.Invoke()) : null,
                _shop ? new MetaShopViewState(_profile.Currency, _profile.Invested(_character), _profile.Catalog.RefundFee,
                    _profile.RefundLockReason(_character), _refundCharacter != null && _profile.RefundLockReason(_refundCharacter) == null,
                    characters.Select(id => RunResultsProjection.Content(id, _profile.Catalog, _registry?.Invoke()))) : null));
        }
        public void Dispose()
        {
            _disposed = true; _profile.Changed -= Refresh;
            _view.ShopRequested -= Shop; _view.CloseRequested -= Close; _view.RetryRequested -= Retry;
            _view.SelectionRequested -= Selection; _view.QuitRequested -= Quit; _view.SaveRequested -= Save;
            _view.ResetRequested -= Reset; _view.CharacterRequested -= Character; _view.PurchaseRequested -= Purchase;
            _view.UpgradesDisabledRequested -= UpgradesDisabled;
            _view.RefundRequested -= RequestRefund; _view.RefundConfirmed -= ConfirmRefund; _view.RefundCancelled -= CancelRefund;
        }
    }
}
