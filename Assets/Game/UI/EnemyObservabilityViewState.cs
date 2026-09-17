using System;

namespace Game.UI
{
    public sealed class EnemyObservabilityViewState
    {
        public string Summary { get; }

        public EnemyObservabilityViewState(string summary)
        {
            Summary = summary ?? throw new ArgumentNullException(nameof(summary));
        }
    }
}
