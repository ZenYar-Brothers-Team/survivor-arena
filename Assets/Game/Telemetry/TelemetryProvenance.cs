using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Game.Telemetry
{
    /// <summary>Copies source text at catalog creation; ordinal hashes include resolved overrides.</summary>
    public static class TelemetryProvenance
    {
        public static JObject Capture(IReadOnlyDictionary<string, string> sources, object resolved,
            string commit, bool? dirty, string platform, string buildType)
        {
            var files = new JArray();
            var sorted = new SortedDictionary<string, string>(StringComparer.Ordinal);
            var bytes = 0;
            foreach (var pair in sources)
            {
                bytes += Encoding.UTF8.GetByteCount(pair.Key) + Encoding.UTF8.GetByteCount(pair.Value);
                if (bytes > TelemetryLimits.ConfigBytes) throw new InvalidOperationException("Config snapshot exceeds 2 MiB.");
                sorted.Add(pair.Key, pair.Value);
            }
            foreach (var pair in sorted)
                files.Add(new JObject { ["resource"] = pair.Key, ["sha256"] = Hash(pair.Value), ["snapshot"] = pair.Value });
            var actual = JObject.FromObject(resolved, TelemetryJson.CreateSerializer());
            return new JObject
            {
                ["commit"] = commit ?? "unknown", ["dirty"] = dirty.HasValue ? new JValue(dirty.Value) : JValue.CreateNull(),
                ["platform"] = platform, ["buildType"] = buildType, ["contentKind"] = "fixture",
                ["files"] = files, ["resolved"] = actual,
                ["configHash"] = Hash(files.ToString(Formatting.None) + Canonical(actual)),
                ["rngUncovered"] = new JArray("UnityEngine.Random spawn positions; no deterministic replay"),
                ["persistentProfile"] = "unsupported"
            };
        }
        private static string Canonical(JToken token)
        {
            if (token is JObject obj)
            {
                var properties = new SortedDictionary<string, JToken>(StringComparer.Ordinal);
                foreach (var property in obj.Properties()) properties.Add(property.Name, property.Value);
                var text = new StringBuilder("{");
                foreach (var pair in properties) text.Append(JsonConvert.SerializeObject(pair.Key)).Append(':').Append(Canonical(pair.Value)).Append(',');
                return text.Append('}').ToString();
            }
            if (token is JArray array)
            {
                var text = new StringBuilder("[");
                foreach (var item in array) text.Append(Canonical(item)).Append(',');
                return text.Append(']').ToString();
            }
            return token.ToString(Formatting.None);
        }
        public static string Hash(string text)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLowerInvariant();
        }
    }
}
