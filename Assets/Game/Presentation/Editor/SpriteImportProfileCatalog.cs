using System;
using System.Collections.Generic;
using System.IO;
using Game.Content.Json;
using Newtonsoft.Json;

namespace Game.Presentation.Editor
{
    public static class SpriteImportProfileCatalog
    {
        public const string Root = "Assets/Resources/Art/";

        public static SpriteImportProfile Resolve(string assetPath)
        {
            var entries = JsonConvert.DeserializeObject<SpriteImportProfile[]>(
                File.ReadAllText("Art/ImportProfiles.json"), JsonContentFile.Settings);
            var profiles = new Dictionary<string, SpriteImportProfile>(StringComparer.Ordinal);
            foreach (var entry in entries ?? throw new InvalidOperationException("Missing import profiles."))
            {
                if (entry == null) throw new InvalidOperationException("Null import profile.");
                entry.Validate();
                if (!profiles.TryAdd(entry.Key, entry)) throw new InvalidOperationException("Duplicate import profile key.");
            }
            var category = CategoryFor(assetPath);
            if (profiles.TryGetValue(assetPath, out var exact)) return exact;
            if (category == "body")
                throw new InvalidOperationException($"Body '{assetPath}' needs an explicit ground-contact pivot record.");
            return profiles.TryGetValue(category, out var profile) ? profile :
                throw new InvalidOperationException($"Missing import profile '{category}'.");
        }

        public static string CategoryFor(string path)
        {
            if (!path.StartsWith(Root, StringComparison.Ordinal) || !path.EndsWith(".png", StringComparison.Ordinal))
                throw new InvalidOperationException("Runtime art must be PNG under Assets/Resources/Art.");
            var file = Path.GetFileNameWithoutExtension(path);
            if (!System.Text.RegularExpressions.Regex.IsMatch(file, "^[a-z0-9]+(-[a-z0-9]+)+$"))
                throw new InvalidOperationException("Runtime art filenames require lowercase kebab-case.");
            if (path.StartsWith(Root + "UI/", StringComparison.Ordinal))
            {
                if (file.EndsWith("-portrait", StringComparison.Ordinal)) return "portrait";
                if (file.EndsWith("-icon", StringComparison.Ordinal)) return "icon";
                throw new InvalidOperationException("UI art requires portrait or icon role.");
            }
            foreach (var role in new[] { "body", "shadow", "projectile", "pickup", "telegraph", "impact", "mask", "weapon", "background", "tile", "prop" })
                if (file.EndsWith("-" + role, StringComparison.Ordinal)) return role;
            throw new InvalidOperationException("Unknown runtime visual role.");
        }
    }
}
