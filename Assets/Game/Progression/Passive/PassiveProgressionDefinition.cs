using System;
using System.Collections.Generic;
using Game.Character;
using Game.Content;

namespace Game.Progression
{
    public sealed class PassiveProgressionDefinition : BuildEntryDefinition
    {
        private readonly CharacterStatModifier[] _levels;

        public IReadOnlyList<CharacterStatModifier> Levels => _levels;

        public PassiveProgressionDefinition(
            ContentId id,
            string displayName,
            params CharacterStatModifier[] levels)
            : base(id, BuildEntryKind.PassiveItem, displayName)
        {
            if (levels == null || levels.Length != MaxLevel)
                throw new ArgumentException($"Passive progression requires exactly {MaxLevel} levels.", nameof(levels));

            _levels = (CharacterStatModifier[])levels.Clone();
        }

        public CharacterStatModifier GetLevel(int level)
        {
            if (level < 1 || level > MaxLevel)
                throw new ArgumentOutOfRangeException(nameof(level));
            return _levels[level - 1];
        }
    }
}
