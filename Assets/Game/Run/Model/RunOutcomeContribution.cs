using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Run
{
    /// <summary>Null means unavailable, including collections; an empty collection is a known empty build.</summary>
    public sealed class RunOutcomeContribution
    {
        public int? Kills { get; }
        public int? Level { get; }
        public float? Experience { get; }
        public IReadOnlyList<RunBuildEntrySnapshot> Build { get; }

        public RunOutcomeContribution(int? kills = null, int? level = null,
            float? experience = null, IEnumerable<RunBuildEntrySnapshot> build = null)
        {
            if (kills.HasValue) NumericValidation.ValidateNonNegative(kills.Value, nameof(kills));
            if (level.HasValue) NumericValidation.ValidateCount(level.Value, nameof(level));
            if (experience.HasValue) NumericValidation.ValidateNonNegative(experience.Value, nameof(experience));
            Kills = kills;
            Level = level;
            Experience = experience;
            if (build != null)
            {
                var copy = new List<RunBuildEntrySnapshot>(build);
                if (copy.Contains(null)) throw new ArgumentException("Build entries cannot be null.", nameof(build));
                Build = copy.AsReadOnly();
            }
        }
    }
}
