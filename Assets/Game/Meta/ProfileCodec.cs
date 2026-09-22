using System;
using System.Collections.Generic;
using Game.Content;
using Game.Content.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace Game.Meta
{
    public sealed class ProfileCodec
    {
        public const int CurrentVersion = 1;
        private readonly Dictionary<int, IProfileMigration> _migrations = new Dictionary<int, IProfileMigration>();
        private readonly MetaCatalog _catalog;
        public ProfileCodec(MetaCatalog catalog, IEnumerable<IProfileMigration> migrations = null)
        { _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog)); if (migrations != null) foreach (var item in migrations) _migrations.Add(item.FromVersion, item); }
        public ProfileData Create()
        {
            var data = new ProfileData { SchemaVersion = CurrentVersion, Upgrades = new Dictionary<string, int>(),
                Unlocked = new HashSet<string>(), ClearedFields = new HashSet<string>(), Runs = new Dictionary<string, MetaRunReceipt>() };
            foreach (var rule in _catalog.Unlocks.Values) if (rule.Condition == "initial") data.Unlocked.Add(rule.Id);
            return data;
        }
        public ProfileData Decode(string json)
        {
            var version = (int?)JObject.Parse(json)["schemaVersion"] ?? throw new ArgumentException("schemaVersion missing.");
            if (version > CurrentVersion) throw new ProfileVersionException("Profile is from a newer game version.");
            while (version < CurrentVersion)
            {
                if (!_migrations.TryGetValue(version, out var step)) throw new ProfileVersionException("Unsupported old profile version.");
                json = step.Migrate(json);
                var next = (int?)JObject.Parse(json)["schemaVersion"];
                if (next != version + 1) throw new ProfileVersionException("Invalid migration step.");
                version++;
            }
            var document = JObject.Parse(json);
            foreach (var field in new[] { "schemaVersion", "currency", "firstRun", "upgrades", "unlocked", "clearedFields", "runs" })
                if (document[field] == null || document[field].Type == JTokenType.Null) throw new ArgumentException("Missing profile field: " + field);
            var data = JsonConvert.DeserializeObject<ProfileData>(json, Settings);
            Validate(data); return data;
        }
        private static JsonSerializerSettings Settings => new JsonSerializerSettings
        { MissingMemberHandling = MissingMemberHandling.Error, ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver { NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy { ProcessDictionaryKeys = false } } };
        public string Encode(ProfileData data) { Validate(data); return JsonConvert.SerializeObject(data, Settings); }
        public ProfileData Copy(ProfileData data) => Decode(Encode(data));
        public void Validate(ProfileData data)
        {
            if (data == null || data.SchemaVersion != CurrentVersion || data.Upgrades == null || data.Unlocked == null || data.ClearedFields == null || data.Runs == null) throw new ArgumentException("Incomplete profile.");
            NumericValidation.ValidateNonNegative(data.Currency, nameof(data.Currency));
            foreach (var id in data.Unlocked) if (!_catalog.Unlocks.ContainsKey(id)) throw new ArgumentException("Unknown unlocked ID: " + id);
            foreach (var id in data.ClearedFields) if (!_catalog.Unlocks.TryGetValue(id, out var field) || field.Kind != "field") throw new ArgumentException("Unknown cleared field.");
            foreach (var pair in data.Upgrades)
            {
                var keys = pair.Key.Split(':');
                if (!_catalog.Upgrades.TryGetValue(keys[0], out var upgrade) || pair.Value < 0 || pair.Value > upgrade.Cap) throw new ArgumentException("Invalid upgrade level.");
                if (upgrade.Personal ? keys.Length != 2 || !_catalog.Unlocks.TryGetValue(keys[1], out var owner) || owner.Kind != "character" || !data.Unlocked.Contains(keys[1]) : keys.Length != 1) throw new ArgumentException("Invalid upgrade owner.");
            }
            foreach (var pair in data.Runs)
            {
                var receipt = pair.Value;
                if (!Guid.TryParse(pair.Key, out _) || receipt == null || receipt.RunId != pair.Key || receipt.NewUnlocks == null) throw new ArgumentException("Invalid run receipt.");
                NumericValidation.ValidateNonNegative(receipt.LevelReward, nameof(receipt.LevelReward));
                NumericValidation.ValidateNonNegative(receipt.BookReward, nameof(receipt.BookReward));
                _ = receipt.Total;
                foreach (var id in receipt.NewUnlocks) if (!_catalog.Unlocks.ContainsKey(id)) throw new ArgumentException("Unknown receipt unlock.");
            }
            foreach (var rule in _catalog.Unlocks.Values) if (rule.Condition == "initial" && !data.Unlocked.Contains(rule.Id)) throw new ArgumentException("Initial unlock missing.");
        }
    }
}
