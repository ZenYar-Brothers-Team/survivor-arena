using System;

namespace Game.UI
{
    public readonly struct HudViewState
    {
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
        public float ExperienceProgress01 { get; }
        public int Level { get; }
        public float RemainingSeconds { get; }
        public WaveViewState Wave { get; }

        public HudViewState(
            float currentHealth,
            float maxHealth,
            float experienceProgress01,
            int level,
            float remainingSeconds,
            WaveViewState wave)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            ExperienceProgress01 = experienceProgress01;
            Level = level;
            RemainingSeconds = remainingSeconds;
            Wave = wave ?? throw new ArgumentNullException(nameof(wave));
        }
    }
}
