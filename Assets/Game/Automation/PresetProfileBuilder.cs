using System;
using System.Collections.Generic;
using Game.Content;
using Game.Meta;
using Newtonsoft.Json;

namespace Game.Automation
{
    /// <summary>Builds a codec-valid laboratory starting profile, without invented receipts or earned progress.</summary>
    public sealed class PresetProfileBuilder
    {
        private readonly MetaCatalog _catalog;
        private readonly ProfileCodec _codec;

        public PresetProfileBuilder(MetaCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _codec = new ProfileCodec(catalog);
        }

        public string Build(string declarationJson)
        {
            var request = JsonConvert.DeserializeObject<PresetProfileData>(declarationJson, ExperimentConfigLoader.JsonSettings)
                ?? throw new ArgumentException("Preset declaration object required.");
            if (!request.Currency.HasValue || request.Currency < 0 || !request.FirstRun.HasValue ||
                !request.UpgradesDisabled.HasValue || request.Upgrades == null || request.Unlocked == null || request.ClearedFields == null)
                throw new ArgumentException("Preset declaration requires currency, firstRun, upgradesDisabled, upgrades, unlocked and clearedFields.");
            var result = _codec.Create();
            result.Currency = request.Currency.Value;
            result.FirstRun = request.FirstRun.Value;
            result.UpgradesDisabled = request.UpgradesDisabled.Value;
            var requestedUnlocks = new HashSet<string>(StringComparer.Ordinal);
            foreach (var raw in request.Unlocked)
            {
                var id = new ContentId(raw).ToString();
                if (!_catalog.Unlocks.ContainsKey(id) || !requestedUnlocks.Add(id))
                    throw new ArgumentException("Unknown or duplicate preset unlock: " + id);
                result.Unlocked.Add(id);
            }
            foreach (var raw in request.ClearedFields)
            {
                var id = new ContentId(raw).ToString();
                if (!_catalog.Unlocks.TryGetValue(id, out var rule) || rule.Kind != "field" || !result.Unlocked.Contains(id) || !result.ClearedFields.Add(id))
                    throw new ArgumentException("Invalid or duplicate cleared field: " + id);
            }
            foreach (var pair in request.Upgrades)
            {
                var pieces = pair.Key.Split(':');
                if (pieces.Length != 2 || !_catalog.Upgrades.TryGetValue(pieces[0], out var upgrade) || !upgrade.Personal ||
                    !_catalog.Unlocks.TryGetValue(pieces[1], out var owner) || owner.Kind != "character" ||
                    !result.Unlocked.Contains(pieces[1]) || pair.Value < 0 || pair.Value > upgrade.Cap)
                    throw new ArgumentException("Invalid personal upgrade level: " + pair.Key);
                var key = upgrade.Key(pieces[1]);
                result.Upgrades.Add(key, pair.Value);
                long spending = 0;
                for (var level = 0; level < pair.Value; level++) spending = checked(spending + upgrade.Price(level));
                result.UpgradeSpending.Add(key, spending);
            }
            return _codec.Encode(result);
        }
    }
}
