using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Character;
using Game.Run;
namespace Game.Meta
{
    /// <summary>Main-thread coordinator. Only the store performs background IO; publishes committed state.</summary>
    public sealed class ProfileService : IProfileService
    {
        private readonly IProfileStore _store;
        private readonly ProfileCodec _codec;
        private ProfileData _data;
        private ProfileData _pending;
        private MetaRunReceipt _pendingReceipt;
        private MetaRunReceipt _lastReceipt;
        private bool _resetAllowed;
        public event Action Changed;
        public ProfileState State { get; private set; }
        public string Message { get; private set; }
        public MetaCatalog Catalog { get; }
        public long Currency => _data?.Currency ?? 0;
        public bool RunActive { get; private set; }
        public bool CanReset => State == ProfileState.LoadError && _resetAllowed;
        public bool CanStart => State == ProfileState.Ready && !RunActive;
        public MetaRunReceipt LastReceipt => _lastReceipt == null ? null : CopyReceipt(_lastReceipt);
        public ProfileService(MetaCatalog catalog, IProfileStore store, IEnumerable<IProfileMigration> migrations = null)
        { Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog)); _store = store ?? throw new ArgumentNullException(nameof(store)); _codec = new ProfileCodec(catalog, migrations); }
        private void Publish(ProfileState state, string message = null) { State = state; Message = message; Changed?.Invoke(); }
        public async Task LoadAsync()
        {
            if (State != ProfileState.NotLoaded && State != ProfileState.LoadError) return;
            Publish(ProfileState.Loading);
            _resetAllowed = false;
            try
            {
                var main = await _store.ReadAsync(false);
                if (main == null)
                {
                    var backup = await _store.ReadAsync(true);
                    if (backup == null) { var fresh = _codec.Create(); await _store.WriteAsync(_codec.Encode(fresh)); _data = fresh; Publish(ProfileState.Ready); return; }
                    _data = _codec.Decode(backup); await _store.WriteAsync(_codec.Encode(_data));
                    Publish(ProfileState.Ready, "Recovered backup; the latest transaction may be missing."); return;
                }
                try { _data = _codec.Decode(main); }
                catch (ProfileVersionException) { throw; }
                catch (Exception)
                {
                    var backup = await _store.ReadAsync(true);
                    if (backup == null) { _resetAllowed = true; throw new InvalidOperationException("Profile is damaged; no backup is available."); }
                    try { _data = _codec.Decode(backup); }
                    catch (ProfileVersionException) { throw; }
                    catch { _resetAllowed = true; throw; }
                    // Preserve corrupt input and the valid backup before replacing either.
                    await _store.PreserveAndResetAsync(_codec.Encode(_data));
                    Publish(ProfileState.Ready, "Recovered backup; the latest transaction may be missing."); return;
                }
                var canonical = _codec.Encode(_data);
                if ((int?)Newtonsoft.Json.Linq.JObject.Parse(main)["schemaVersion"] != ProfileCodec.CurrentVersion)
                    await _store.WriteAsync(canonical);
                Publish(ProfileState.Ready);
            }
            catch (Exception error) { _data = null; Publish(ProfileState.LoadError, error.Message); }
        }
        public async Task ResetAsync()
        {
            if (State != ProfileState.LoadError || !_resetAllowed) return;
            Publish(ProfileState.Saving);
            try { var fresh = _codec.Create(); await _store.PreserveAndResetAsync(_codec.Encode(fresh)); _data = fresh; _resetAllowed = false; Publish(ProfileState.Ready); }
            catch (Exception error) { Publish(ProfileState.LoadError, error.Message); }
        }
        public bool IsUnlocked(string id) => id != null && _data != null && _data.Unlocked.Contains(id);
        public int Level(string upgrade, string character = null)
        {
            if (_data == null || !Catalog.Upgrades.TryGetValue(upgrade, out var item) || item.Personal && character == null) return 0;
            return _data.Upgrades.TryGetValue(item.Key(character), out var level) ? level : 0;
        }
        private bool Condition(MetaUnlock rule, ProfileData data) => rule.Condition == "initial" ||
            rule.Condition == "firstRun" && data.FirstRun || rule.Condition == "fieldClear" && data.ClearedFields.Contains(rule.RequiredId) ||
            rule.Condition == "access" && data.Unlocked.Contains(rule.RequiredId);
        public string PurchaseLockReason(string id, string character = null)
        {
            if (State != ProfileState.Ready) return "Save the profile first";
            if (RunActive) return "Available between runs";
            long price;
            if (Catalog.Upgrades.TryGetValue(id, out var upgrade))
            {
                if (upgrade.Personal && (character == null || !Catalog.Unlocks.TryGetValue(character, out var owner) || owner.Kind != "character" || !IsUnlocked(character))) return "Choose an unlocked character";
                var level = Level(id, character);
                if (level >= upgrade.Cap) return "Maximum level";
                price = upgrade.Price(level);
            }
            else if (Catalog.Unlocks.TryGetValue(id, out var rule))
            {
                if (IsUnlocked(id)) return "Already unlocked";
                if (!Condition(rule, _data)) return rule.Description;
                if (rule.Price == 0) return "Unlocked by achievement";
                price = rule.Price;
            }
            else return "Unknown content";
            return Currency < price ? "Not enough currency" : null;
        }
        public async Task<bool> PurchaseAsync(string id, int expectedLevel, string character = null)
        {
            if (PurchaseLockReason(id, character) != null) return false;
            var next = _codec.Copy(_data);
            if (Catalog.Upgrades.TryGetValue(id, out var upgrade))
            {
                var current = Level(id, character);
                if (current != expectedLevel) return false;
                next.Currency = checked(next.Currency - upgrade.Price(current));
                next.Upgrades[upgrade.Key(character)] = current + 1;
            }
            else
            {
                if (expectedLevel != 0) return false;
                var rule = Catalog.Unlocks[id]; next.Currency = checked(next.Currency - rule.Price); next.Unlocked.Add(id);
            }
            ResolveUnlocks(next);
            Publish(ProfileState.Saving);
            try { await _store.WriteAsync(_codec.Encode(next)); _data = next; Publish(ProfileState.Ready, "Purchase saved"); return true; }
            catch (Exception error) { Publish(ProfileState.Ready, "Purchase not saved: " + error.Message); return false; }
        }
        private List<string> ResolveUnlocks(ProfileData data)
        {
            var unlocked = new List<string>(); bool changed;
            do
            {
                changed = false;
                foreach (var rule in Catalog.Unlocks.Values)
                    if (rule.Price == 0 && Condition(rule, data) && data.Unlocked.Add(rule.Id)) { unlocked.Add(rule.Id); changed = true; }
            } while (changed);
            return unlocked;
        }
        public CharacterStatModifier Modifier(string character)
        {
            float hp = 0, damage = 0;
            foreach (var upgrade in Catalog.Upgrades.Values)
            {
                var bonus = upgrade.Bonus * Level(upgrade.Id, character);
                if (upgrade.Stat == "health") hp += bonus; else damage += bonus;
            }
            return new CharacterStatModifier(maxHealthMultiplierBonus: hp, activeSkillDamageMultiplierBonus: damage);
        }
        public void SetRunActive(bool active)
        {
            if (active && !CanStart) throw new InvalidOperationException("Profile is not ready for a new run.");
            RunActive = active; Changed?.Invoke();
        }
        public async Task<bool> ApplyAsync(RunOutcome outcome, bool started)
        {
            if (outcome == null) throw new ArgumentNullException(nameof(outcome));
            if (!started) return true;
            if (_data != null && _data.Runs.TryGetValue(outcome.RunId.ToString(), out var existing))
            { _lastReceipt = existing; Changed?.Invoke(); return true; }
            if (State != ProfileState.Ready) return false;
            try
            {
                if (outcome.Selection == null || !Catalog.Unlocks.ContainsKey(outcome.Selection.FieldId.ToString()) ||
                    !outcome.Contributions.TryGetValue("experience", out var xp) || !xp.Level.HasValue ||
                    !outcome.Contributions.TryGetValue("draft", out var draft) || draft.DraftTotals == null)
                    throw new InvalidOperationException("Incomplete result; reward cannot be saved.");
                var next = _codec.Copy(_data);
                var receipt = new MetaRunReceipt { RunId = outcome.RunId.ToString(), LevelReward = checked(Catalog.RewardPerLevel * xp.Level.Value),
                    BookReward = draft.DraftTotals.BookCurrency, NewUnlocks = new List<string>() };
                next.Currency = checked(next.Currency + receipt.Total);
                next.FirstRun = true;
                // Victory is authoritative; shortened fixture runs never clear a 15-minute field.
                if (outcome.Reason == RunCompletionReason.Victory && outcome.ElapsedSeconds >= Catalog.FieldClearSeconds)
                    next.ClearedFields.Add(outcome.Selection.FieldId.ToString());
                receipt.NewUnlocks = ResolveUnlocks(next);
                next.Runs.Add(receipt.RunId, receipt);
                _pending = next; _pendingReceipt = receipt;
                return await SavePendingAsync();
            }
            catch (Exception error) { Publish(ProfileState.PendingResult, error.Message); return false; }
        }
        private async Task<bool> SavePendingAsync()
        {
            Publish(ProfileState.Saving);
            try
            {
                await _store.WriteAsync(_codec.Encode(_pending)); _data = _pending; _lastReceipt = _pendingReceipt;
                _pending = null; _pendingReceipt = null; Publish(ProfileState.Ready); return true;
            }
            catch (Exception error) { Publish(ProfileState.PendingResult, "Result not saved: " + error.Message); return false; }
        }
        public Task<bool> RetrySaveAsync() => State == ProfileState.PendingResult && _pending != null ? SavePendingAsync() : Task.FromResult(false);
        private static MetaRunReceipt CopyReceipt(MetaRunReceipt value) => new MetaRunReceipt { RunId = value.RunId, LevelReward = value.LevelReward,
            BookReward = value.BookReward, NewUnlocks = new List<string>(value.NewUnlocks) };
    }
}
