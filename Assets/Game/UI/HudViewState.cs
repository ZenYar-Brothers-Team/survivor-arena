using System;
using Game.Run;

namespace Game.UI
{
    public readonly struct HudViewState
    {
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
        public float ExperienceProgress01 { get; }
        public int Level { get; }
        public float ElapsedSeconds { get; }
        public WaveViewState Wave { get; }
        public CharacterStatsViewState Stats { get; }
        public RunExperienceSnapshot ExperienceTotals { get; }
        public long BookCurrency { get; }
        public BossViewState Boss { get; }

        public HudViewState(
            float currentHealth,
            float maxHealth,
            float experienceProgress01,
            int level,
            float elapsedSeconds,
            WaveViewState wave,
            CharacterStatsViewState stats = null,
            RunExperienceSnapshot experienceTotals = null, long bookCurrency = 0, BossViewState boss = default)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            ExperienceProgress01 = experienceProgress01;
            Level = level;
            ElapsedSeconds = elapsedSeconds;
            Wave = wave ?? throw new ArgumentNullException(nameof(wave));
            Stats = stats;
            ExperienceTotals = experienceTotals;
            BookCurrency = bookCurrency;
            Boss = boss;
        }
    }
}
