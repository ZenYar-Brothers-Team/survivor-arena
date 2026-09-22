using System;
using System.Collections.Generic;
using Game.Diagnostics;
using Game.Character;
using Game.Content;
using Game.Pooling;
using Game.Run;
using UnityEngine;

namespace Game.Progression
{
    [DisallowMultipleComponent]
    public sealed class PlayerExperienceRuntime : MonoBehaviour, IRunOutcomeContributor
    {
        [SerializeField]
        private PlayerCharacterRuntime owner;

        [SerializeField]
        private RunController runController;

        private bool _initialized;
        private float _baseDropLifetimeSeconds;
        private GameObjectPool<ExperienceDropRuntime> _dropPool;
        private Transform _dropPoolRoot;
        private RunModel _outcomeOwner;
        private readonly HashSet<ExperienceDropRuntime> _activeDrops = new HashSet<ExperienceDropRuntime>();
        public bool IsInitialized => _initialized;
        public string Key => "experience";
        public float CollectedBase { get; private set; }
        public float CollectedAwarded { get; private set; }
        public float ExpiredBase { get; private set; }
        public float RecoveredAwarded { get; private set; }
        public float InterventionBase { get; private set; }
        public float InterventionAwarded { get; private set; }
        public float TotalAwarded => CollectedAwarded + RecoveredAwarded + InterventionAwarded;
        public RunExperienceSnapshot Totals => new RunExperienceSnapshot(CollectedBase, CollectedAwarded, ExpiredBase,
            RecoveredAwarded, InterventionBase, InterventionAwarded);
        public event Action<ExperienceAwardEvent> ExperienceResolved;

        public ExperienceProgression Progression { get; private set; }
        public float DropLifetimeSeconds => _baseDropLifetimeSeconds + OwnerStats.XpDropLifetimeBonusSeconds;
        public float DisappearingExperienceRecovery => OwnerStats.DisappearingXpRecovery;
        public float PickedUpExperienceMultiplier => OwnerStats.PickedUpXpMultiplier;
        public float PickupRadius => OwnerStats.PickupRadius;

        // Owned here (rather than statically inside ExperienceDropFactory) so
        // pooled drops live and die with this player instance instead of being
        // shared process-wide — each test's own PlayerExperienceRuntime gets an
        // isolated pool, and production has exactly one long-lived player.
        public GameObjectPool<ExperienceDropRuntime> DropPool
        {
            get
            {
                if (!_initialized) throw new InvalidOperationException("Experience runtime is not initialized.");
                if (_dropPool == null)
                {
                    _dropPoolRoot = new GameObject("XP Drop Pool").transform;
                    _dropPoolRoot.SetParent(transform, false);
                    _dropPool = new GameObjectPool<ExperienceDropRuntime>(ExperienceDropFactory.CreateInstance, _dropPoolRoot);
                }
                return _dropPool;
            }
        }

        public double DroppedBase { get; private set; }
        public double GroundBase { get; private set; }
        internal void RegisterDrop(ExperienceDropRuntime drop)
        {
            if (!_activeDrops.Add(drop)) return;
            DroppedBase += drop.Amount;
            GroundBase += drop.Amount;
        }
        internal void UnregisterDrop(ExperienceDropRuntime drop)
        {
            if (_activeDrops.Remove(drop)) GroundBase = Math.Max(0, GroundBase - drop.Amount);
        }

        public event Action<int> LevelUp;
        public event Action<int, int> LevelsEarned;

        private CharacterStats OwnerStats
        {
            get
            {
                if (owner == null || owner.Stats == null)
                    throw new InvalidOperationException("Experience runtime requires an initialized character owner.");
                return owner.Stats;
            }
        }

        private void Start()
        {
            if (_initialized)
                return;

            Debug.LogError("Player experience runtime must be initialized by the gameplay composition root.", this);
            enabled = false;
        }

        // The XP curve and base drop lifetime are content (Resources/Content/Run/*.json), handed
        // in by the caller — the composition root in production — never serialized on this component.
        public void Initialize(PlayerCharacterRuntime characterOwner, RunController controller, ExperienceSettings settings)
        {
            if (_initialized)
                throw new InvalidOperationException("Player experience runtime is already initialized.");

            owner = characterOwner != null ? characterOwner : throw new ArgumentNullException(nameof(characterOwner));
            runController = controller != null ? controller : throw new ArgumentNullException(nameof(controller));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            // A previous life (Shutdown() then Initialize() again) still owns a subscribed
            // Progression; release it before replacing it.
            if (Progression != null)
            {
                Progression.LevelUp -= HandleLevelUp;
                Progression.LevelsEarned -= HandleLevelsEarned;
            }
            Progression = new ExperienceProgression(settings.CopyThresholds());
            Progression.LevelUp += HandleLevelUp;
            Progression.LevelsEarned += HandleLevelsEarned;
            _baseDropLifetimeSeconds = settings.BaseDropLifetimeSeconds;

            var model = controller.Model ?? throw new InvalidOperationException("Run must be initialized before XP.");
            model.RegisterOutcomeContributor(this);
            _outcomeOwner = model;
            CollectedBase = CollectedAwarded = ExpiredBase = RecoveredAwarded = InterventionBase = InterventionAwarded = 0f;
            DroppedBase = GroundBase = 0;
            _initialized = true;
        }

        public float AddPickedUpExperience(float baseAmount, ExperienceDropIdentity? drop = null) =>
            Award(baseAmount, ExperienceEventKind.Collected, drop);

        public float AddRecoveredExperience(float expiredAmount, ExperienceDropIdentity? drop = null) =>
            Award(expiredAmount, ExperienceEventKind.Expired, drop);

        public float AddInterventionExperience(float baseAmount) =>
            Award(baseAmount, ExperienceEventKind.DevelopmentIntervention, null);

        private float Award(float amount, ExperienceEventKind kind, ExperienceDropIdentity? drop)
        {
            NumericValidation.ValidateNonNegative(amount, nameof(amount));
            if (!_initialized || _outcomeOwner.Outcome != null) return 0f;
            if (drop.HasValue && drop.Value.RunId != _outcomeOwner.RunId) return 0f;
            var awarded = amount * (kind == ExperienceEventKind.Expired ? DisappearingExperienceRecovery : PickedUpExperienceMultiplier);
            NumericValidation.ValidateNonNegative(awarded, nameof(awarded));
            NumericValidation.ValidateNonNegative(Progression.CurrentExperience + awarded, nameof(awarded));
            NumericValidation.ValidateNonNegative(TotalAwarded + awarded, nameof(awarded));
            var advance = Progression.CalculateAward(awarded);
            var snapshot = new ExperienceAwardEvent(_outcomeOwner.RunId, drop, kind, amount, awarded);
            var notify = ExperienceResolved;
            switch (kind)
            {
                case ExperienceEventKind.Collected:
                    NumericValidation.ValidateNonNegative(CollectedBase + amount, nameof(amount));
                    CollectedBase += amount;
                    CollectedAwarded += awarded;
                    break;
                case ExperienceEventKind.Expired:
                    NumericValidation.ValidateNonNegative(ExpiredBase + amount, nameof(amount));
                    ExpiredBase += amount;
                    RecoveredAwarded += awarded;
                    break;
                case ExperienceEventKind.DevelopmentIntervention:
                    NumericValidation.ValidateNonNegative(InterventionBase + amount, nameof(amount));
                    InterventionBase += amount;
                    InterventionAwarded += awarded;
                    break;
            }
            Progression.ApplyAward(advance);
            notify?.Invoke(snapshot);
            return awarded;
        }

        public RunOutcomeContribution Capture() => new RunOutcomeContribution(
            level: Progression.Level, experience: Progression.CurrentExperience,
            experienceTotals: Totals);

        public void Shutdown()
        {
            _outcomeOwner?.UnregisterOutcomeContributor(this);
            _outcomeOwner = null;
            if (Progression != null)
            {
                Progression.LevelUp -= HandleLevelUp;
                Progression.LevelsEarned -= HandleLevelsEarned;
            }
            _initialized = false;
            using var guard = PerfGuard.Measure("PlayerExperienceRuntime.ShutdownDrops", 2f);
            foreach (var drop in new List<ExperienceDropRuntime>(_activeDrops))
                if (drop != null) drop.Shutdown();
            _activeDrops.Clear();
            if (_dropPoolRoot != null)
            {
                if (Application.isPlaying) Destroy(_dropPoolRoot.gameObject);
                else DestroyImmediate(_dropPoolRoot.gameObject);
            }
            _dropPoolRoot = null;
            _dropPool = null;
            ExperienceResolved = null;
            LevelUp = null;
            LevelsEarned = null;
        }

        private void HandleLevelUp(int newLevel)
        {
            if (!_initialized || _outcomeOwner == null || _outcomeOwner.Outcome != null) return;
            LevelUp?.Invoke(newLevel);
        }

        private void HandleLevelsEarned(int firstLevel, int lastLevel)
        {
            if (!_initialized || _outcomeOwner == null || _outcomeOwner.Outcome != null) return;
            _outcomeOwner.RequestPause(RunPauseReasons.LevelUpDraft);
            LevelsEarned?.Invoke(firstLevel, lastLevel);
        }

        private void OnDestroy()
        {
            Shutdown();
        }
    }
}
