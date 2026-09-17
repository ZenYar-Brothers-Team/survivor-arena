using System;
using Game.Content;

namespace Game.Progression
{
    public readonly struct CharacterDraftWeight
    {
        public ContentId SkillId { get; }
        public float Weight { get; }

        public CharacterDraftWeight(ContentId skillId, float weight)
        {
            if (!skillId.IsValid)
                throw new ArgumentException("Character draft weight requires a valid skill id.", nameof(skillId));
            NumericValidation.ValidateNonNegative(weight, nameof(weight));

            SkillId = skillId;
            Weight = weight;
        }
    }
}
