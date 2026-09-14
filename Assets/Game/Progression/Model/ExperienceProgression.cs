using System;

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

        public ExperienceProgression(params float[] thresholds)
        {
            if (thresholds == null || thresholds.Length == 0)
                throw new ArgumentException("At least one level threshold is required.", nameof(thresholds));

            _thresholds = (float[])thresholds.Clone();
            for (var i = 0; i < _thresholds.Length; i++)
                ValidatePositiveFinite(_thresholds[i], nameof(thresholds));
        }

        public int AddExperience(float amount)
        {
            ValidateNonNegativeFinite(amount, nameof(amount));
            if (amount == 0f)
                return 0;

            var previousExperience = CurrentExperience;
            CurrentExperience += amount;
            var levelsGained = 0;

            while (CurrentExperience >= RequiredExperience)
            {
                CurrentExperience -= RequiredExperience;
                Level++;
                levelsGained++;
                LevelUp?.Invoke(Level);
            }

            ExperienceChanged?.Invoke(previousExperience, CurrentExperience);
            return levelsGained;
        }

        private float GetThreshold(int level)
        {
            var index = Math.Min(level - 1, _thresholds.Length - 1);
            return _thresholds[index];
        }

        private static void ValidatePositiveFinite(float value, string parameterName)
        {
            ValidateNonNegativeFinite(value, parameterName);
            if (value <= 0f)
                throw new ArgumentOutOfRangeException(parameterName, "Thresholds must be greater than zero.");
        }

        private static void ValidateNonNegativeFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and non-negative.");
        }
    }
}
