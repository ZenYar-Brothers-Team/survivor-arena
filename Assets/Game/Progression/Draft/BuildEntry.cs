using System;

namespace Game.Progression
{
    public sealed class BuildEntry
    {
        public BuildEntryDefinition Definition { get; }
        public int Level { get; private set; } = 1;
        public bool IsMaxLevel => Level >= Definition.LevelCap;

        internal BuildEntry(BuildEntryDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        internal void Upgrade()
        {
            if (IsMaxLevel)
                throw new InvalidOperationException("A max-level build entry cannot be upgraded.");
            Level++;
        }
    }
}
