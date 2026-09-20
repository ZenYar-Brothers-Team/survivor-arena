using System;
using Game.Enemy;

namespace Game.UI
{
    public sealed class WaveViewState
    {
        public int PhaseNumber { get; }
        public int PhaseCount { get; }
        public string DisplayName { get; }
        public WavePhaseTag Tag { get; }

        public WaveViewState(int phaseNumber, int phaseCount, string displayName, WavePhaseTag tag)
        {
            if (phaseNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(phaseNumber));
            if (phaseCount < 0)
                throw new ArgumentOutOfRangeException(nameof(phaseCount));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            PhaseNumber = phaseNumber;
            PhaseCount = phaseCount;
            Tag = tag;
        }
    }
}
