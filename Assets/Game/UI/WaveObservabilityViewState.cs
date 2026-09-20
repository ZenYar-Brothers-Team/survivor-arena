using System;

namespace Game.UI
{
    public sealed class WaveObservabilityViewState
    {
        public string Summary { get; }

        public WaveObservabilityViewState(string summary)
        {
            Summary = summary ?? throw new ArgumentNullException(nameof(summary));
        }
    }
}
