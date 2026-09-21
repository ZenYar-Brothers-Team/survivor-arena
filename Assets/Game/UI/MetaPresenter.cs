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
        private bool _shop;
        private bool _disposed;
        private string _character;
        private RunOutcome _result;
        public MetaPresenter(IProfileService profile, IMetaView view, IProfileNavigation navigation)
        {
            _profile = profile; _view = view; _navigation = navigation;
            profile.Changed += Refresh;
            view.ShopRequested += Shop; view.CloseRequested += Close; view.RetryRequested += Retry;
            view.SelectionRequested += Selection; view.QuitRequested += Quit; view.SaveRequested += Save;
            view.ResetRequested += Reset; view.CharacterRequested += Character; view.PurchaseRequested += Purchase;
            Refresh();
        }
        public void ShowResult(RunOutcome outcome) { _result = outcome; _shop = false; Refresh(); }
        public void ClearResult() { _result = null; _shop = false; Refresh(); }
        private void Shop() { if (_profile.CanStart) { _shop = true; Refresh(); } }
        private void Close() { _shop = false; if (_result == null && _profile.CanStart) _navigation.ReturnToProfileSelection(); Refresh(); }
        private void Retry() { if (_profile.CanStart && _result != null) _navigation.RetryProfileRun(); }
        private void Selection() { if (_profile.CanStart) _navigation.ReturnToProfileSelection(); }
        private void Quit() { if (_profile.RunActive) _navigation.QuitProfileRun(); }
        private async void Save() { if (_profile.State == ProfileState.LoadError) await _profile.LoadAsync(); else await _profile.RetrySaveAsync(); if (!_disposed && _result == null && _profile.CanStart) _navigation.ReturnToProfileSelection(); }
        private async void Reset() { await _profile.ResetAsync(); if (!_disposed && _profile.CanStart) _navigation.ReturnToProfileSelection(); }
        private void Character(string id) { if (_profile.IsUnlocked(id)) { _character = id; Refresh(); } }
        private async void Purchase(MetaCardViewState card) { await _profile.PurchaseAsync(card.Id, card.Level, card.Character); }
        private void Refresh()
        {
            if (_disposed) return;
            var characters = _profile.Catalog.Unlocks.Values.Where(r => r.Kind == "character" && _profile.IsUnlocked(r.Id)).Select(r => r.Id).ToList();
            if (!characters.Contains(_character)) _character = characters.FirstOrDefault();
            var cards = new List<MetaCardViewState>();
            if (_shop)
            {
                foreach (var upgrade in _profile.Catalog.Upgrades.Values)
                {
                    var owner = upgrade.Personal ? _character : null;
                    var level = _profile.Level(upgrade.Id, owner);
                    var reason = _profile.PurchaseLockReason(upgrade.Id, owner);
                    var icon = upgrade.Stat == "health" ? "HP" : "DMG";
                    cards.Add(new MetaCardViewState(upgrade.Id, owner, level,
                        icon + " · " + upgrade.Name + " · " + level + "/" + upgrade.Cap,
                        "+" + (upgrade.Bonus * 100).ToString("0") + "% per level · " +
                        (level >= upgrade.Cap ? "MAX" : upgrade.Price(level) + " currency") + "\n" + (reason ?? "Buy"), reason == null));
                }
                foreach (var rule in _profile.Catalog.Unlocks.Values.Where(r => r.Condition != "initial"))
                {
                    var reason = _profile.PurchaseLockReason(rule.Id);
                    cards.Add(new MetaCardViewState(rule.Id, null, 0, rule.Name,
                        _profile.IsUnlocked(rule.Id) ? "Unlocked" : rule.Description + (rule.Price > 0 ? " · " + rule.Price + " currency" : "") + "\n" + reason,
                        rule.Price > 0 && reason == null));
                }
            }
            var summary = "Currency: " + _profile.Currency;
            if (_result != null && !_shop)
            {
                _result.Contributions.TryGetValue("experience", out var xp);
                _result.Contributions.TryGetValue("ordinary-enemy-kills", out var enemies);
                var receipt = _profile.LastReceipt;
                summary = _result.Reason + " · " + TimeSpan.FromSeconds(_result.ElapsedSeconds).ToString(@"mm\:ss") +
                    " · Level " + (xp?.Level?.ToString() ?? "unavailable") + " � Kills " + (enemies?.Kills?.ToString() ?? "unavailable") + "\n" + summary;
                if (receipt != null && receipt.RunId == _result.RunId.ToString())
                    summary += "\nLevel reward: " + receipt.LevelReward + " · Books: " + receipt.BookReward + " · Total: " + receipt.Total +
                        (receipt.NewUnlocks.Count > 0 ? "\nNew unlocks: " + string.Join(", ", receipt.NewUnlocks) : "");
            }
            var error = _profile.State == ProfileState.LoadError;
            _view.Render(new MetaViewState(_shop || _result != null || error || _profile.State == ProfileState.Loading || _profile.State == ProfileState.NotLoaded,
                error ? "Profile unavailable" : _shop ? "Meta progression" : _result != null ? "Run results" : "Loading profile",
                summary, _profile.Message ?? (_profile.State == ProfileState.Saving ? "Saving…" : ""),
                _profile.CanStart, _profile.CanStart, _result != null && !_shop, error, _profile.CanReset, _profile.RunActive,
                cards, characters, _character));
        }
        public void Dispose()
        {
            _disposed = true; _profile.Changed -= Refresh;
            _view.ShopRequested -= Shop; _view.CloseRequested -= Close; _view.RetryRequested -= Retry;
            _view.SelectionRequested -= Selection; _view.QuitRequested -= Quit; _view.SaveRequested -= Save;
            _view.ResetRequested -= Reset; _view.CharacterRequested -= Character; _view.PurchaseRequested -= Purchase;
        }
    }
}
