using System;
using System.Collections.Generic;
using Game.Content;
using Game.Content.Json;
using Game.Movement;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    public static class FixtureSpriteCatalog
    {
        private const string ResourcePath = "Content/Presentation/FixtureSprites";

        public static IReadOnlyList<SpriteDefinition> CreateFor(IEnumerable<ContentId> visualIds)
        {
            var definitions = new List<SpriteDefinition>();
            var seen = new HashSet<ContentId>();
            var configured = LoadConfiguredSprites();
            foreach (var id in visualIds)
            {
                if (!id.IsValid || !seen.Add(id))
                    continue;

                if (!configured.ContainsKey(id) && !id.ToString().StartsWith("FIXTURE-", StringComparison.Ordinal))
                    throw new InvalidOperationException($"Production visual '{id}' cannot use fixture fallback.");

                definitions.Add(configured.TryGetValue(id, out var sprite)
                    ? sprite
                    : new SpriteDefinition(id, PlaceholderSprite.Shared));
            }

            return definitions;
        }

        private static IReadOnlyDictionary<ContentId, SpriteDefinition> LoadConfiguredSprites()
        {
            var data = JsonContentFile.Load<SpriteDefinitionData[]>(ResourcePath);
            var definitions = new Dictionary<ContentId, SpriteDefinition>();
            for (var i = 0; i < data.Length; i++)
            {
                var entry = data[i] ?? throw new InvalidOperationException(
                    "Fixture sprite data cannot contain null entries.");
                if (string.IsNullOrWhiteSpace(entry.ResourcePath))
                    throw new InvalidOperationException($"Fixture sprite '{entry.Id}' requires a resource path.");
                if (!entry.Role.HasValue || entry.Role == SpriteRole.Unspecified)
                    throw new InvalidOperationException($"Fixture sprite '{entry.Id}' requires an explicit role.");

                ContentId id = entry.Id;
                var sprite = Resources.Load<Sprite>(entry.ResourcePath);
                if (sprite == null)
                    throw new InvalidOperationException(
                        $"Fixture sprite '{id}' is missing at Resources/{entry.ResourcePath}.");
                if (!definitions.TryAdd(id, new SpriteDefinition(id, sprite, entry.Role.Value)))
                    throw new InvalidOperationException($"Duplicate fixture sprite id '{id}'.");
            }

            return definitions;
        }
    }
}
