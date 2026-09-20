using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Progression
{
    // XP curve and drop lifetime for a run. Content, not code: it comes from
    // Resources/Content/Run/*.json (see FixtureRunSetupCatalog and AGENTS.md).
    public sealed class ExperienceSettings
    {
        private readonly float[] _levelThresholds;

        public IReadOnlyList<float> LevelThresholds => _levelThresholds;
        public float BaseDropLifetimeSeconds { get; }

        public ExperienceSettings(float baseDropLifetimeSeconds, params float[] levelThresholds)
        {
            if (levelThresholds == null || levelThresholds.Length == 0)
                throw new ArgumentException("At least one level threshold is required.", nameof(levelThresholds));

            NumericValidation.ValidatePositive(baseDropLifetimeSeconds, nameof(baseDropLifetimeSeconds));
            _levelThresholds = (float[])levelThresholds.Clone();
            for (var i = 0; i < _levelThresholds.Length; i++)
                NumericValidation.ValidatePositive(
                    _levelThresholds[i],
                    nameof(levelThresholds),
                    "Thresholds must be greater than zero.");
            BaseDropLifetimeSeconds = baseDropLifetimeSeconds;
        }

        // ExperienceProgression keeps its own copy, so callers can hand it a fresh array.
        public float[] CopyThresholds() => (float[])_levelThresholds.Clone();
    }
}
