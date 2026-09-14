using System;
using Game.Character;
using Game.Run;
using UnityEngine;

namespace Game.Progression
{
    [DisallowMultipleComponent]
    public sealed class PlayerExperienceRuntime : MonoBehaviour
    {
        [SerializeField]
        private PlayerCharacterRuntime owner;

        [SerializeField]
        private RunController runController;

        [SerializeField, Min(0.0001f)]
        private float baseDropLifetimeSeconds = 60f;

        [SerializeField]
        private float[] fixtureLevelThresholds = { 5f, 10f, 15f };

        public ExperienceProgression Progression { get; private set; }
        public float DropLifetimeSeconds => baseDropLifetimeSeconds + OwnerStats.XpDropLifetimeBonusSeconds;
        public float DisappearingExperienceRecovery => OwnerStats.DisappearingXpRecovery;
        public float PickedUpExperienceMultiplier => OwnerStats.PickedUpXpMultiplier;

        public event Action<int> LevelUp;

        private CharacterStats OwnerStats
        {
            get
            {
                if (owner == null || owner.Stats == null)
                    throw new InvalidOperationException("Experience runtime requires an initialized character owner.");
                return owner.Stats;
            }
        }

        private void Awake()
        {
            Progression = new ExperienceProgression(fixtureLevelThresholds);
            Progression.LevelUp += HandleLevelUp;
        }

        private void Start()
        {
            if (owner != null && runController != null && runController.Model != null)
                return;

            Debug.LogError("Player experience owner and run controller must be configured.", this);
            enabled = false;
        }

        public float AddPickedUpExperience(float baseAmount)
        {
            ValidateNonNegativeFinite(baseAmount, nameof(baseAmount));
            var awarded = baseAmount * PickedUpExperienceMultiplier;
            Progression.AddExperience(awarded);
            return awarded;
        }

        public float AddRecoveredExperience(float expiredAmount)
        {
            ValidateNonNegativeFinite(expiredAmount, nameof(expiredAmount));
            var awarded = expiredAmount * DisappearingExperienceRecovery;
            Progression.AddExperience(awarded);
            return awarded;
        }

        public void ConfigureForTests(
            PlayerCharacterRuntime characterOwner,
            RunController controller,
            params float[] thresholds)
        {
            owner = characterOwner;
            runController = controller;
            if (Progression != null)
                Progression.LevelUp -= HandleLevelUp;
            Progression = new ExperienceProgression(thresholds);
            Progression.LevelUp += HandleLevelUp;
        }

        private void HandleLevelUp(int newLevel)
        {
            if (runController != null && runController.Model != null)
                runController.Model.RequestPause(RunPauseReasons.LevelUpDraft);
            LevelUp?.Invoke(newLevel);
        }

        private void OnDestroy()
        {
            if (Progression != null)
                Progression.LevelUp -= HandleLevelUp;
        }

        private static void ValidateNonNegativeFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and non-negative.");
        }
    }
}
