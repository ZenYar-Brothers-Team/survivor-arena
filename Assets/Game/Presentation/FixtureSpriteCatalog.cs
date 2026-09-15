using System.Collections.Generic;
using Game.Content;
using Game.Movement;

namespace Game.Presentation
{
    public static class FixtureSpriteCatalog
    {
        public static IReadOnlyList<SpriteDefinition> CreateFor(IEnumerable<ContentId> visualIds)
        {
            var definitions = new List<SpriteDefinition>();
            var seen = new HashSet<ContentId>();
            foreach (var id in visualIds)
            {
                if (!id.IsValid || !seen.Add(id))
                    continue;

                definitions.Add(new SpriteDefinition(id, PlaceholderSprite.Shared));
            }

            return definitions;
        }
    }
}
