using System;
using Game.Character;
using Game.Content;
using Game.Pooling;
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

        private bool _initialized;
        private GameObjectPool<ExperienceDropRuntime> _dropPool;

        public ExperienceProgression Progression { get; private set; }
        public float DropLifetimeSeconds => baseDropLifetimeSeconds + OwnerStats.XpDropLifetimeBonusSeconds;
        public float DisappearingExperienceRecovery => OwnerStats.DisappearingXpRecovery;
        public float PickedUpExperienceMultiplier => OwnerStats.PickedUpXpMultiplier;
        public float PickupRadius => OwnerStats.BaseStats.PickupRadius;

        // Owned here (rather than statically inside ExperienceDropFactory) so
        // pooled drops live and die with this player instance instead of being
        // shared process-wide — each test's own PlayerExperienceRuntime gets an
        // isolated pool, and production has exactly one long-lived player.
        public GameObjectPool<ExperienceDropRuntime> DropPool =>
            _dropPool ??= new GameObjectPool<ExperienceDropRuntime>(ExperienceDropFactory.CreateInstance, transform);

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
            if (_initialized)
                return;

            Debug.LogError("Player experience runtime must be initialized by the gameplay composition root.", this);
            enabled = false;
        }

        public void Initialize(PlayerCharacterRuntime characterOwner, RunController controller, params float[] thresholds)
        {
            if (_initialized)
                throw new InvalidOperationException("Player experience runtime is already initialized.");

            owner = characterOwner != null ? characterOwner : throw new ArgumentNullException(nameof(characterOwner));
            runController = controller != null ? controller : throw new ArgumentNullException(nameof(controller));

            // An explicit threshold override (tests, or content that wants to bypass
            // the scene-configured defaults) replaces the Progression Awake() already
            // built from the serialized field; otherwise that default stands as-is.
            // Progression can still be null here: edit-mode tests construct this
            // component via AddComponent without running Awake() first.
            if (thresholds != null && thresholds.Length > 0)
            {
                if (Progression != null)
                    Progression.LevelUp -= HandleLevelUp;
                Progression = new ExperienceProgression(thresholds);
                Progression.LevelUp += HandleLevelUp;
            }

            _initialized = true;
        }

        public float AddPickedUpExperience(float baseAmount)
        {
            NumericValidation.ValidateNonNegativeFinite(baseAmount, nameof(baseAmount));
            var awarded = baseAmount * PickedUpExperienceMultiplier;
            Progression.AddExperience(awarded);
            return awarded;
        }

        public float AddRecoveredExperience(float expiredAmount)
        {
            NumericValidation.ValidateNonNegativeFinite(expiredAmount, nameof(expiredAmount));
            var awarded = expiredAmount * DisappearingExperienceRecovery;
            Progression.AddExperience(awarded);
            return awarded;
        }

        // Initialize() only assigns its own fields and (optionally) rebuilds the
        // self-owned Progression instance — it never subscribes to another
        // object, so rolling back is just clearing the initialized flag.
        public void Shutdown()
        {
            _initialized = false;
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
    }
}
