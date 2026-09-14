using System;
using System.Collections.Generic;
using Game.Content;
using Game.Progression;

namespace Game.ActiveSkill
{
    public sealed class ActiveSkillProgressionDefinition : BuildEntryDefinition
    {
        private readonly ActiveSkillLevelDefinition[] _levels;

        public IReadOnlyList<ActiveSkillLevelDefinition> Levels => _levels;

        public ActiveSkillProgressionDefinition(
            ContentId id,
            string displayName,
            params ActiveSkillLevelDefinition[] levels)
            : base(id, BuildEntryKind.ActiveSkill, displayName)
        {
            if (levels == null || levels.Length != MaxLevel)
                throw new ArgumentException($"Active skill progression requires exactly {MaxLevel} levels.", nameof(levels));
            for (var i = 0; i < levels.Length; i++)
            {
                if (levels[i] == null)
                    throw new ArgumentException("Skill levels cannot contain null.", nameof(levels));
            }

            _levels = (ActiveSkillLevelDefinition[])levels.Clone();
        }

        public ActiveSkillLevelDefinition GetLevel(int level)
        {
            if (level < 1 || level > MaxLevel)
                throw new ArgumentOutOfRangeException(nameof(level));
            return _levels[level - 1];
        }
    }
}
