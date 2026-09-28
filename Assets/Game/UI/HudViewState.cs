using System;
using Game.Run;
using UnityEngine;

namespace Game.UI
{
    public readonly struct HudViewState
    {
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
        public float ExperienceProgress01 { get; }
        public int Level { get; }
        public float ElapsedSeconds { get; }
        public float RunDurationSeconds { get; }
        public WaveViewState Wave { get; }
        public CharacterStatsViewState Stats { get; }
        public RunExperienceSnapshot ExperienceTotals { get; }
        public long BookCurrency { get; }
        public BossViewState Boss { get; }
        public int SpeedMultiplier { get; }
        public bool CanChangeSpeed { get; }
        public bool IsHealthLocked { get; }
        public string CharacterName { get; }
        public Sprite CharacterPortrait { get; }
        public float? BaselineMovementSpeed { get; }

        public HudViewState(
            float currentHealth,
            float maxHealth,
            float experienceProgress01,
            int level,
            float elapsedSeconds,
            WaveViewState wave,
            CharacterStatsViewState stats = null,
            RunExperienceSnapshot experienceTotals = null, long bookCurrency = 0, BossViewState boss = default,
            int speedMultiplier = 1, bool canChangeSpeed = false, float runDurationSeconds = 0f,
            bool isHealthLocked = false, string characterName = "", Sprite characterPortrait = null,
            float? baselineMovementSpeed = null)
        {
            CharacterName = characterName ?? string.Empty;
            CharacterPortrait = characterPortrait;
            if (baselineMovementSpeed.HasValue)
                Game.Content.NumericValidation.ValidatePositive(baselineMovementSpeed.Value, nameof(baselineMovementSpeed));
            BaselineMovementSpeed = baselineMovementSpeed;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            ExperienceProgress01 = experienceProgress01;
            Level = level;
            ElapsedSeconds = elapsedSeconds;
            RunDurationSeconds = runDurationSeconds;
            Wave = wave ?? throw new ArgumentNullException(nameof(wave));
            Stats = stats;
            ExperienceTotals = experienceTotals;
            BookCurrency = bookCurrency;
            Boss = boss;
            SpeedMultiplier = speedMultiplier;
            CanChangeSpeed = canChangeSpeed;
            IsHealthLocked = isHealthLocked;
        }
    }
}
