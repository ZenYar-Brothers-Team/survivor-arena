using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.ActiveSkill.Json;
using Game.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Game.ActiveSkill
{
    /// <summary>Authoring-only L1..L6 fold. Domain constructors still validate each resolved definition.</summary>
    public static class ActiveSkillLevelResolver
    {
        private static readonly Regex PathSyntax = new Regex(@"^[a-zA-Z][a-zA-Z0-9]*(\[\d+\])?(\.[a-zA-Z][a-zA-Z0-9]*(\[\d+\])?)*$");

        public static ActiveSkillLevelData[] Resolve(JObject baseLevel, ActiveSkillLevelChangeData[] changes,
            JsonSerializerSettings settings)
        {
            if (baseLevel == null) throw new ArgumentNullException(nameof(baseLevel));
            if (changes == null || changes.Length != ActiveSkillProgressionDefinition.MaxLevel)
                throw new ArgumentException("Exactly six level changes are required.", nameof(changes));
            var baseline = (JObject)baseLevel.DeepClone();
            var bonuses = new Dictionary<string, double>(StringComparer.Ordinal);
            var levels = new ActiveSkillLevelData[changes.Length];
            var serializer = JsonSerializer.Create(settings);
            for (var level = 0; level < changes.Length; level++)
            {
                var change = changes[level] ?? throw new ArgumentException("Level changes cannot contain null.");
                if (change.Overrides != null)
                    foreach (var item in change.Overrides)
                    {
                        Find(baseline, item.Key).Replace(item.Value?.DeepClone() ?? JValue.CreateNull());
                        // An explicit absolute replacement supersedes previous bonuses at this path/subtree.
                        var obsolete = new List<string>();
                        foreach (var key in bonuses.Keys)
                            if (key == item.Key || key.StartsWith(item.Key + ".", StringComparison.Ordinal) ||
                                key.StartsWith(item.Key + "[", StringComparison.Ordinal)) obsolete.Add(key);
                        foreach (var key in obsolete) bonuses.Remove(key);
                    }
                if (change.Bonuses != null)
                    foreach (var item in change.Bonuses)
                    {
                        NumericValidation.ValidateFinite(item.Value, item.Key);
                        var token = Find(baseline, item.Key);
                        if (token.Type != JTokenType.Float && token.Type != JTokenType.Integer)
                            throw new ArgumentException($"Bonus '{item.Key}' must address a numeric parameter.");
                        if (item.Key.EndsWith("Count", StringComparison.Ordinal) || item.Key.EndsWith("Seed", StringComparison.Ordinal))
                            throw new ArgumentException($"'{item.Key}' requires an absolute replacement.");
                        bonuses.TryGetValue(item.Key, out var previous);
                        bonuses[item.Key] = previous + item.Value;
                    }
                var resolved = (JObject)baseline.DeepClone();
                foreach (var item in bonuses)
                {
                    var value = Find(resolved, item.Key);
                    // Frequency bonuses are already additive ratios, not a percentage of a zero baseline.
                    var result = item.Key == "actionSpeedBonus"
                        ? value.Value<double>() + item.Value
                        : value.Value<double>() * (1d + item.Value);
                    NumericValidation.ValidateFinite((float)result, item.Key);
                    value.Replace(new JValue(result));
                }
                levels[level] = resolved.ToObject<ActiveSkillLevelData>(serializer);
            }
            return levels;
        }

        private static JToken Find(JObject root, string path)
        {
            if (path == null || !PathSyntax.IsMatch(path)) throw new ArgumentException("A concrete parameter path is required.", nameof(path));
            return root.SelectToken(path, errorWhenNoMatch: true);
        }
    }
}
