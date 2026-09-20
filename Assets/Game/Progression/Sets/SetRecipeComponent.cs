using System;
using Game.Content;

namespace Game.Progression
{
    public sealed class SetRecipeComponent
    {
        public ContentId Id { get; }
        public BuildEntryKind Kind { get; }
        public int MinimumLevel { get; }

        public SetRecipeComponent(ContentId id, BuildEntryKind kind, int minimumLevel)
        {
            if (!id.IsValid)
                throw new ArgumentException("Set recipe component requires a valid content id.", nameof(id));
            if (kind != BuildEntryKind.ActiveSkill && kind != BuildEntryKind.PassiveItem)
                throw new ArgumentOutOfRangeException(nameof(kind), "Set recipes only support active skills and passive items.");
            if (minimumLevel < 1 || minimumLevel > BuildEntryDefinition.MaxLevel)
                throw new ArgumentOutOfRangeException(nameof(minimumLevel));

            Id = id;
            Kind = kind;
            MinimumLevel = minimumLevel;
        }

        public bool IsFulfilled(PlayerBuild build)
        {
            if (build == null)
                throw new ArgumentNullException(nameof(build));
            return build.TryGetEntry(Id, out var entry) &&
                   entry.Definition.Kind == Kind &&
                   entry.Level >= MinimumLevel;
        }
    }
}
