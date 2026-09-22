using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Progression
{
    public sealed class DraftOptionPreview
    {
        public int CurrentLevel { get; }
        public int NextLevel { get; }
        public IReadOnlyList<DraftValueChange> Values { get; }

        public DraftOptionPreview(int currentLevel, int nextLevel, IEnumerable<DraftValueChange> values = null)
        {
            NumericValidation.ValidateNonNegative(currentLevel, nameof(currentLevel));
            NumericValidation.ValidateCount(nextLevel, nameof(nextLevel));
            CurrentLevel = currentLevel;
            NextLevel = nextLevel;
            Values = Array.AsReadOnly(values == null ? Array.Empty<DraftValueChange>() : new List<DraftValueChange>(values).ToArray());
        }
    }
}
