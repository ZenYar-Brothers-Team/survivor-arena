using System;
using Game.Content;

namespace Game.Enemy
{
    // A timeline marker the director raises once when run time reaches it. The
    // director only announces it; boss spawning belongs to the boss framework.
    public sealed class WaveHookDefinition
    {
        public WaveHookKind Kind { get; }
        public float TimeSeconds { get; }

        public WaveHookDefinition(WaveHookKind kind, float timeSeconds)
        {
            if (!Enum.IsDefined(typeof(WaveHookKind), kind))
                throw new ArgumentOutOfRangeException(nameof(kind));
            NumericValidation.ValidateNonNegative(timeSeconds, nameof(timeSeconds));

            Kind = kind;
            TimeSeconds = timeSeconds;
        }
    }
}
