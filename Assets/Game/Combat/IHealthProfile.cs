using System;

namespace Game.Combat
{
    public interface IHealthProfile
    {
        float MaxHealth { get; }
        float IncomingDamageMultiplier { get; }
        float HealthRestorationMultiplier { get; }
        float HealthRegenerationPerSecond { get; }

        event Action Changed;
    }
}
