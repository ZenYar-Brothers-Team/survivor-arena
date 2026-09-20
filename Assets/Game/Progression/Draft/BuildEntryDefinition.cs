using System;
using Game.Content;

namespace Game.Progression
{
    public class BuildEntryDefinition : IContentDefinition
    {
        public const int MaxLevel = 6;

        public ContentId Id { get; }
        public BuildEntryKind Kind { get; }
        public string DisplayName { get; }
        public virtual int LevelCap => MaxLevel;
        public virtual float DraftChance => 1f;

        public virtual DraftOptionPreview CreateDraftPreview(int currentLevel, int nextLevel) =>
            new DraftOptionPreview(currentLevel, nextLevel);

        public BuildEntryDefinition(ContentId id, BuildEntryKind kind, string displayName)
        {
            if (!id.IsValid)
                throw new ArgumentException("Build entry requires a valid content id.", nameof(id));
            if (!Enum.IsDefined(typeof(BuildEntryKind), kind))
                throw new ArgumentOutOfRangeException(nameof(kind));
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Build entry display name cannot be empty.", nameof(displayName));

            Id = id;
            Kind = kind;
            DisplayName = displayName;
        }
    }
}
