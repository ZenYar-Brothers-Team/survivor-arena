using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Game.Content;
using Game.Content.Json;
namespace Game.Meta
{
    /// <summary>Approved DECISION-0037 economy with the DECISION-0050 unlock mapping; fixture access is a separately loaded catalog.</summary>
    public sealed class MetaCatalog
    {
        public long RewardPerLevel { get; }
        public long EmptyBookReward { get; }
        public float FieldClearSeconds { get; }
        public IReadOnlyDictionary<string, MetaUpgrade> Upgrades { get; }
        public IReadOnlyDictionary<string, MetaUnlock> Unlocks { get; }
        /// <summary>True for the fixture economy used by prototype content and tests.</summary>
        public bool IsFixture { get; }
        public MetaCatalog(MetaCatalogData data, bool isFixture = false)
        {
            IsFixture = isFixture;
            if (data == null) throw new ArgumentNullException(nameof(data));
            RewardPerLevel = data.RewardPerLevel ?? throw new ArgumentException("rewardPerLevel required.");
            EmptyBookReward = data.EmptyBookReward ?? throw new ArgumentException("emptyBookReward required.");
            FieldClearSeconds = data.FieldClearSeconds ?? throw new ArgumentException("fieldClearSeconds required.");
            NumericValidation.ValidateNonNegative(RewardPerLevel, nameof(RewardPerLevel));
            NumericValidation.ValidateNonNegative(EmptyBookReward, nameof(EmptyBookReward));
            if (EmptyBookReward > int.MaxValue) throw new ArgumentException("Book reward exceeds draft representation.");
            NumericValidation.ValidatePositive(FieldClearSeconds, nameof(FieldClearSeconds));
            var upgrades = new Dictionary<string, MetaUpgrade>(StringComparer.Ordinal);
            foreach (var item in data.Upgrades ?? throw new ArgumentException("upgrades required."))
            { var upgrade = new MetaUpgrade(item ?? throw new ArgumentException("Null upgrade.")); upgrades.Add(upgrade.Id, upgrade); }
            var unlocks = new Dictionary<string, MetaUnlock>(StringComparer.Ordinal);
            foreach (var item in data.Unlocks ?? throw new ArgumentException("unlocks required."))
            { var rule = new MetaUnlock(item ?? throw new ArgumentException("Null unlock.")); unlocks.Add(rule.Id, rule); }
            foreach (var rule in unlocks.Values)
            {
                if (rule.RequiredId == null) continue;
                if (!unlocks.TryGetValue(rule.RequiredId, out var required) || required.Kind != "field") throw new ArgumentException("Unlock requires a known field: " + rule.Id);
                var seen = new HashSet<string> { rule.Id };
                var next = rule.RequiredId;
                while (next != null)
                { if (!seen.Add(next)) throw new ArgumentException("Cyclic unlock."); next = unlocks[next].RequiredId; }
            }
            Upgrades = new ReadOnlyDictionary<string, MetaUpgrade>(upgrades);
            Unlocks = new ReadOnlyDictionary<string, MetaUnlock>(unlocks);
        }
        public static MetaCatalog Load(bool fixture = false) => new MetaCatalog(JsonContentFile.Load<MetaCatalogData>(
            fixture ? "Content/Meta/FixtureMetaEconomy" : "Content/Meta/MetaEconomy"), fixture);
    }
}
