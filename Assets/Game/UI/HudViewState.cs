namespace Game.UI
{
    public readonly struct HudViewState
    {
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
        public float ExperienceProgress01 { get; }
        public int Level { get; }
        public float RemainingSeconds { get; }

        public HudViewState(float currentHealth, float maxHealth, float experienceProgress01, int level, float remainingSeconds)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            ExperienceProgress01 = experienceProgress01;
            Level = level;
            RemainingSeconds = remainingSeconds;
        }
    }
}
