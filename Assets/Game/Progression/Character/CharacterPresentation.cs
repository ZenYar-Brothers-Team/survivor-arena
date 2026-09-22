using System;
using System.Collections.Generic;
using Game.Character;
using Game.Content;
using Game.Presentation;

namespace Game.Progression
{
    /// <summary>Authored, ordered highlights. Empty means no highlights, not automatic filtering.</summary>
    public sealed class CharacterPresentation
    {
        public string Role { get; }
        public ContentRef<CharacterComparisonBaseline> Baseline { get; }
        public ContentRef<SpriteDefinition> Crop { get; }
        public ContentRef<SpriteDefinition> Icon { get; }
        public IReadOnlyList<CharacterStatField> Highlights { get; }

        public CharacterPresentation(string role, ContentId baselineId, ContentId cropId, ContentId iconId,
            IReadOnlyList<CharacterStatField> highlights)
        {
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentException("Character role is required.", nameof(role));
            if (!baselineId.IsValid || !cropId.IsValid || !iconId.IsValid)
                throw new ArgumentException("Baseline, crop and icon references are required.");
            if (highlights == null) throw new ArgumentNullException(nameof(highlights));
            var copy = new CharacterStatField[highlights.Count];
            var unique = new HashSet<CharacterStatField>();
            for (var i = 0; i < copy.Length; i++)
            {
                if (!Enum.IsDefined(typeof(CharacterStatField), highlights[i]) || !unique.Add(highlights[i]))
                    throw new ArgumentException("Unknown or duplicate character highlight.", nameof(highlights));
                copy[i] = highlights[i];
            }
            Role = role;
            Baseline = new ContentRef<CharacterComparisonBaseline>(baselineId);
            Crop = new ContentRef<SpriteDefinition>(cropId);
            Icon = new ContentRef<SpriteDefinition>(iconId);
            Highlights = Array.AsReadOnly(copy);
        }
    }
}
