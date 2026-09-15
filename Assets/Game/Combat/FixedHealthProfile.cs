using System;

namespace Game.Combat
{
    public sealed class FixedHealthProfile : IHealthProfile
    {
        public float MaxHealth { get; }
        public float IncomingDamageMultiplier => 1f;
        public float HealthRestorationMultiplier => 1f;
        public float HealthRegenerationPerSecond => 0f;

        public event Action Changed
        {
            add { }
            remove { }
        }

        public FixedHealthProfile(float maxHealth)
        {
            if (float.IsNaN(maxHealth) || float.IsInfinity(maxHealth) || maxHealth <= 0f)
                throw new ArgumentOutOfRangeException(nameof(maxHealth), "Max health must be finite and greater than zero.");

            MaxHealth = maxHealth;
        }
    }
}
