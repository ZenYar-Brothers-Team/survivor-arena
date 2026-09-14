using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.ActiveSkill
{
    public sealed class ActiveSkillProgressionDefinition : IContentDefinition
    {
        public const int MaxLevel = 6;

        private readonly ActiveSkillLevelDefinition[] _levels;

        public ContentId Id { get; }
        public string DisplayName { get; }
        public IReadOnlyList<ActiveSkillLevelDefinition> Levels => _levels;

        public ActiveSkillProgressionDefinition(
            ContentId id,
            string displayName,
            params ActiveSkillLevelDefinition[] levels)
        {
            if (!id.IsValid)
                throw new ArgumentException("Active skill progression requires a valid content id.", nameof(id));
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
            if (levels == null || levels.Length != MaxLevel)
                throw new ArgumentException($"Active skill progression requires exactly {MaxLevel} levels.", nameof(levels));
            for (var i = 0; i < levels.Length; i++)
            {
                if (levels[i] == null)
                    throw new ArgumentException("Skill levels cannot contain null.", nameof(levels));
            }

            Id = id;
            DisplayName = displayName;
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
