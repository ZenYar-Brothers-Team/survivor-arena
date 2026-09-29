using System;
using System.Threading.Tasks;
using Game.Meta;

namespace Game.Automation
{
    /// <summary>Only normal profile purchases; re-reads level, price, lock and currency after every save.</summary>
    public sealed class AutomationPurchasePolicy
    {
        private readonly PurchasePolicyData _settings;
        public AutomationPurchasePolicy(PurchasePolicyData settings)
        { _settings = settings ?? throw new ArgumentNullException(nameof(settings)); }

        public async Task<PurchasePolicyResult> ExecuteAsync(IProfileService profile, string character)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            var result = new PurchasePolicyResult();
            for (var purchase = 0; purchase < _settings.MaxPurchasesPerIntermission.Value; purchase++)
            {
                string chosen = null;
                long bestPrice = long.MaxValue;
                result.Refusals.Clear();
                foreach (var id in _settings.AllowedUpgradeIds)
                {
                    if (!profile.Catalog.Upgrades.TryGetValue(id, out var upgrade) || !upgrade.Personal)
                        throw new InvalidOperationException("Purchase policy contains an invalid personal upgrade: " + id);
                    var reason = profile.PurchaseLockReason(id, character);
                    if (reason != null) { result.Refusals[id] = reason; continue; }
                    var price = upgrade.Price(profile.Level(id, character));
                    if (price < bestPrice || price == bestPrice && StringComparer.Ordinal.Compare(id, chosen) < 0)
                    { bestPrice = price; chosen = id; }
                }
                if (chosen == null) break;
                var previous = profile.Level(chosen, character);
                var currencyBefore = profile.Currency;
                if (!await profile.PurchaseAsync(chosen, previous, character).ConfigureAwait(true))
                {
                    result.Error = "Purchase rejected or save failed: " + chosen + "; " + profile.Message;
                    break;
                }
                if (profile.Level(chosen, character) != previous + 1 || profile.Currency != currencyBefore - bestPrice)
                    throw new InvalidOperationException("Purchase state does not match saved price/level: " + chosen);
                result.Purchases.Add(new PurchaseRecord(chosen, previous, bestPrice, profile.Currency));
            }
            return result;
        }
    }
}
