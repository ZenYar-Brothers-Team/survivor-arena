using System;
using Game.Content;

namespace Game.Run
{
    /// <summary>A content identity and level at session completion, independent of progression types.</summary>
    public sealed class RunBuildEntrySnapshot
    {
        public string ContentId { get; }
        public int Level { get; }

        public RunBuildEntrySnapshot(string contentId, int level)
        {
            if (string.IsNullOrWhiteSpace(contentId))
                throw new ArgumentException("Content identity is required.", nameof(contentId));
            NumericValidation.ValidateCount(level, nameof(level));
            ContentId = contentId;
            Level = level;
        }
    }
}
