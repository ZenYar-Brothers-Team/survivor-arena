using System;
using Game.Character;
using Game.Content;

namespace Game.Progression
{
    /// <summary>DECISION-0026: independent presentation data; never applied as player stats.</summary>
    public sealed class CharacterComparisonBaseline : IContentDefinition
    {
        public ContentId Id { get; }
        public CharacterBaseStats Stats { get; }
        public CharacterComparisonBaseline(ContentId id, CharacterBaseStats stats)
        {
            if (!id.IsValid) throw new ArgumentException("Baseline requires an id.", nameof(id));
            stats.Validate();
            Id = id;
            Stats = stats;
        }
    }
}
