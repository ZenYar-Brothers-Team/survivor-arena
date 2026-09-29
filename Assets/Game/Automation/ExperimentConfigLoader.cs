using System;
using System.Collections.Generic;
using System.IO;
using Game.Content;
using Game.Meta;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Game.Automation
{
    /// <summary>Strictly validates a version-1 experiment before any run or output file is created.</summary>
    public sealed class ExperimentConfigLoader
    {
        internal static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Error,
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        private readonly MetaCatalog _catalog;
        private readonly ProfileCodec _codec;
        private readonly HashSet<string> _playableFields;
        private readonly string _experimentRoot;
        private readonly bool _allowExistingOutput;

        /// <param name="playableFields">Fields with complete production runtime bindings, not merely unlock definitions.</param>
        public ExperimentConfigLoader(MetaCatalog catalog, IEnumerable<string> playableFields, string experimentRoot,
            bool allowExistingOutput = false)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _codec = new ProfileCodec(catalog);
            _playableFields = new HashSet<string>(playableFields ?? throw new ArgumentNullException(nameof(playableFields)), StringComparer.Ordinal);
            _experimentRoot = Path.GetFullPath(experimentRoot ?? throw new ArgumentNullException(nameof(experimentRoot)));
            _allowExistingOutput = allowExistingOutput;
        }

        public ExperimentConfig LoadFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Experiment file required.", nameof(path));
            var fullPath = Path.GetFullPath(path);
            return Parse(File.ReadAllText(fullPath), Path.GetDirectoryName(fullPath));
        }

        /// <summary>baseDirectory resolves only the preset input; output remains confined to the configured experiment root.</summary>
        public ExperimentConfig Parse(string json, string baseDirectory)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Experiment JSON required.", nameof(json));
            var data = JsonConvert.DeserializeObject<ExperimentConfigData>(json, JsonSettings)
                ?? throw new ArgumentException("Experiment object required.");
            if (data.SchemaVersion != 1) throw new ArgumentException("Unsupported experiment schemaVersion.");
            if (string.IsNullOrWhiteSpace(data.ExperimentId) || data.ExperimentId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("Invalid experimentId.");
            if (data.Template != "fresh" && data.Template != "preset") throw new ArgumentException("Unknown template.");
            if (data.Chains.GetValueOrDefault() <= 0 || data.MaxRunsPerChain.GetValueOrDefault() <= 0)
                throw new ArgumentException("chains and maxRunsPerChain must be positive.");
            if (data.RunSpeed != 1 && data.RunSpeed != 2 && data.RunSpeed != 3 && data.RunSpeed != 5)
                throw new ArgumentException("runSpeed must be 1, 2, 3 or 5.");
            if (!data.StopAfterRouteClear.HasValue) throw new ArgumentException("stopAfterRouteClear required.");
            PositiveFinite(data.MaxExperimentWallSeconds, "maxExperimentWallSeconds");
            PositiveFinite(data.RunWallTimeoutSeconds, "runWallTimeoutSeconds");
            PositiveFinite(data.TransitionTimeoutSeconds, "transitionTimeoutSeconds");
            ValidatePolicies(data);

            var character = RequiredContentId(data.CharacterId, "characterId");
            if (!_catalog.Unlocks.TryGetValue(character, out var characterRule) || characterRule.Kind != "character")
                throw new ArgumentException("Unknown characterId: " + character);
            if (data.FieldRoute == null || data.FieldRoute.Count == 0) throw new ArgumentException("fieldRoute required.");
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var raw in data.FieldRoute)
            {
                var field = RequiredContentId(raw, "fieldRoute");
                if (!_catalog.Unlocks.TryGetValue(field, out var rule) || rule.Kind != "field" || !seen.Add(field))
                    throw new ArgumentException("Unknown or duplicate fieldRoute ID: " + field);
            }
            if (!_playableFields.Contains(data.FieldRoute[0])) throw new ArgumentException("First field has no production runtime binding.");

            if (string.IsNullOrWhiteSpace(data.OutputDirectory) || Path.IsPathRooted(data.OutputDirectory))
                throw new ArgumentException("outputDirectory must be a relative path inside experiment root.");
            var output = Path.GetFullPath(Path.Combine(_experimentRoot, data.OutputDirectory));
            if (!output.StartsWith(_experimentRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("outputDirectory escapes experiment root.");
            if (File.Exists(output) || Directory.Exists(output) && !_allowExistingOutput)
                throw new IOException("Experiment output already exists: " + output);

            string presetPath = null;
            ProfileData initial;
            if (data.Template == "fresh")
            {
                if (data.InitialProfilePath != null) throw new ArgumentException("fresh forbids initialProfilePath.");
                initial = _codec.Create();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(data.InitialProfilePath)) throw new ArgumentException("preset requires initialProfilePath.");
                presetPath = Path.GetFullPath(Path.Combine(baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory)), data.InitialProfilePath));
                initial = _codec.Decode(File.ReadAllText(presetPath));
            }
            if (!initial.Unlocked.Contains(character)) throw new ArgumentException("Starting character is locked: " + character);
            if (!initial.Unlocked.Contains(data.FieldRoute[0])) throw new ArgumentException("Starting field is locked: " + data.FieldRoute[0]);
            return new ExperimentConfig(data, output, presetPath, _codec.Encode(initial));
        }

        private void ValidatePolicies(ExperimentConfigData data)
        {
            var movement = data.MovementPolicy ?? throw new ArgumentException("movementPolicy required.");
            if (movement.Id != "safePickup" || movement.Version != 1) throw new ArgumentException("Unknown movement policy/version.");
            Range(movement.DecisionIntervalSeconds, 0.02f, 2f, "decisionIntervalSeconds");
            Range(movement.ObservationRadius, 1f, 50f, "observationRadius");
            Range(movement.PredictionSeconds, 0.05f, 3f, "predictionSeconds");
            Range(movement.ObstaclePadding, 0f, 2f, "obstaclePadding");
            Range(movement.StuckSeconds, 0.5f, 30f, "stuckSeconds");
            if (data.DraftPolicy?.Id != "randomLegal" || data.DraftPolicy.Version != 1)
                throw new ArgumentException("Unknown draft policy/version.");
            var purchase = data.PurchasePolicy ?? throw new ArgumentException("purchasePolicy required.");
            if (purchase.Id != "cheapestPersonalUpgrade" || purchase.Version != 1 || purchase.AllowedUpgradeIds == null ||
                purchase.MaxPurchasesPerIntermission.GetValueOrDefault(-1) < 0)
                throw new ArgumentException("Invalid purchase policy.");
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var raw in purchase.AllowedUpgradeIds)
            {
                var id = RequiredContentId(raw, "allowedUpgradeIds");
                if (!_catalog.Upgrades.TryGetValue(id, out var upgrade) || !upgrade.Personal || !seen.Add(id))
                    throw new ArgumentException("Unknown, non-personal or duplicate allowed upgrade: " + id);
            }
        }

        private static string RequiredContentId(string raw, string name)
        {
            if (string.IsNullOrWhiteSpace(raw)) throw new ArgumentException(name + " required.");
            return new ContentId(raw).ToString();
        }

        private static void PositiveFinite(float? value, string name)
        {
            if (!value.HasValue || float.IsNaN(value.Value) || float.IsInfinity(value.Value) || value.Value <= 0)
                throw new ArgumentException(name + " must be finite and positive.");
        }

        private static void Range(float? value, float minimum, float maximum, string name)
        {
            if (!value.HasValue || float.IsNaN(value.Value) || float.IsInfinity(value.Value) || value.Value < minimum || value.Value > maximum)
                throw new ArgumentException(name + " out of range.");
        }
    }
}
