using System;
using Game.Content;

namespace Game.Progression
{
    /// <summary>Draft weight of one active skill or passive item (DECISION-0089); 0 excludes the entry.</summary>
    public readonly struct CharacterDraftWeight
    {
        public ContentId EntryId { get; }
        public float Weight { get; }

        public CharacterDraftWeight(ContentId entryId, float weight)
        {
            if (!entryId.IsValid)
                throw new ArgumentException("Character draft weight requires a valid entry id.", nameof(entryId));
            NumericValidation.ValidateNonNegative(weight, nameof(weight));

            EntryId = entryId;
            Weight = weight;
        }
    }
}
