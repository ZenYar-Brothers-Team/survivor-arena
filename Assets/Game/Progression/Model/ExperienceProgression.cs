using System;
using Game.Content;

namespace Game.Progression
{
    public sealed class ExperienceProgression
    {
        private readonly float[] _thresholds;

        public int Level { get; private set; } = 1;
        public float CurrentExperience { get; private set; }
        public float RequiredExperience => GetThreshold(Level);
        public float Progress01 => RequiredExperience <= 0f ? 0f : CurrentExperience / RequiredExperience;

        public event Action<float, float> ExperienceChanged;
        public event Action<int> LevelUp;
        public event Action<int, int> LevelsEarned;

        public ExperienceProgression(params float[] thresholds)
        {
            if (thresholds == null || thresholds.Length == 0)
                throw new ArgumentException("At least one level threshold is required.", nameof(thresholds));

            _thresholds = (float[])thresholds.Clone();
            for (var i = 0; i < _thresholds.Length; i++)
                NumericValidation.ValidatePositive(_thresholds[i], nameof(thresholds), "Thresholds must be greater than zero.");
        }

        public int AddExperience(float amount) => ApplyAward(CalculateAward(amount));

        internal ExperienceAdvance CalculateAward(float amount)
        {
            NumericValidation.ValidateNonNegative(amount, nameof(amount));
            double remainder = (double)CurrentExperience + amount;
            var level = Level;
            while (level < _thresholds.Length && remainder >= GetThreshold(level))
            {
                remainder -= GetThreshold(level);
                level++;
            }
            // The final configured threshold repeats. Division avoids a subtraction loop
            // which can stop making progress for very large floating-point awards.
            var threshold = GetThreshold(level);
            var repeated = Math.Floor(remainder / threshold);
            if (repeated > int.MaxValue - level) throw new ArgumentOutOfRangeException(nameof(amount), "XP award exceeds the supported level range.");
            remainder -= repeated * threshold;
            level += (int)repeated;
            var experience = (float)remainder;
            if (experience >= threshold)
            {
                if (level == int.MaxValue) throw new ArgumentOutOfRangeException(nameof(amount));
                experience = 0f;
                level++;
            }
            return new ExperienceAdvance(Level, level, CurrentExperience, experience, amount > 0f);
        }

        internal int ApplyAward(ExperienceAdvance advance)
        {
            if (!advance.HasAward) return 0;
            if (advance.PreviousLevel != Level || advance.PreviousExperience != CurrentExperience)
                throw new InvalidOperationException("XP advance no longer matches progression state.");
            // Commit the entire award before callbacks can pause or finish the run.
            Level = advance.Level;
            CurrentExperience = advance.Experience;
            var gained = advance.Level - advance.PreviousLevel;
            if (gained > 0) LevelsEarned?.Invoke(advance.PreviousLevel + 1, advance.Level);
            for (var i = 0; i < gained; i++) LevelUp?.Invoke(advance.PreviousLevel + i + 1);
            ExperienceChanged?.Invoke(advance.PreviousExperience, advance.Experience);
            return gained;
        }

        private float GetThreshold(int level)
        {
            var index = Math.Min(level - 1, _thresholds.Length - 1);
            return _thresholds[index];
        }
    }
}
