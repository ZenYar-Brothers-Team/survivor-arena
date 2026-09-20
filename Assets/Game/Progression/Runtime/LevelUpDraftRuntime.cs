using System;
using System.Collections.Generic;
using Game.Content;
using Game.Run;
using UnityEngine;

namespace Game.Progression
{
    [DisallowMultipleComponent]
    public sealed class LevelUpDraftRuntime : MonoBehaviour
    {
        [SerializeField]
        private PlayerExperienceRuntime experienceRuntime;

        [SerializeField]
        private RunController runController;

        private DraftPool _pool;
        private IDraftRandom _draftRandom;
        private int _offerCount;
        private int _pendingDrafts;
        private bool _initialized;

        public PlayerBuild Build { get; private set; }
        public CharacterDefinition Character { get; private set; }
        public DraftSession CurrentDraft { get; private set; }
        public bool IsDraftOpen => CurrentDraft != null && CurrentDraft.IsOpen;
        public int PendingDraftCount => _pendingDrafts;
        public DraftRunControls Controls { get; private set; }
        public IReadOnlyList<SetDefinition> SetDefinitions { get; private set; } = Array.Empty<SetDefinition>();
        public PlayerSetRuntime Sets { get; private set; }
        public int RemainingRerolls => Controls?.RemainingRerolls ?? 0;
        public int RemainingBanishes => Controls?.RemainingBanishes ?? 0;

        public event Action<IReadOnlyList<DraftOption>> DraftOpened;
        public event Action<BuildSelectionResult> SelectionApplied;

        private void Update()
        {
            if (!_initialized || Sets == null || runController.Model == null)
                return;
            Sets.Tick(Time.deltaTime, runController.Model.State == RunState.Running);
        }

        private void Start()
        {
            if (_initialized)
                return;

            Debug.LogError("Level-up draft runtime must be initialized by the gameplay composition root.", this);
            enabled = false;
        }

        public void Initialize(
            PlayerExperienceRuntime experience,
            RunController controller,
            IEnumerable<BuildEntryDefinition> definitions,
            BuildEntryDefinition startingActive,
            int offerCount,
            IDraftRandom draftRandom = null,
            int initialRerolls = 0,
            int initialBanishes = 0,
            IEnumerable<SetDefinition> setDefinitions = null,
            ISetExtraAbilityFactory setAbilityFactory = null)
        {
            InitializeCore(
                experience,
                controller,
                definitions,
                startingActive,
                null,
                offerCount,
                draftRandom,
                initialRerolls,
                initialBanishes,
                setDefinitions,
                setAbilityFactory);
        }

        public void Initialize(
            PlayerExperienceRuntime experience,
            RunController controller,
            IEnumerable<BuildEntryDefinition> definitions,
            CharacterDefinition character,
            ContentRegistry registry,
            int offerCount,
            IDraftRandom draftRandom = null,
            int initialRerolls = 0,
            int initialBanishes = 0,
            IEnumerable<SetDefinition> setDefinitions = null,
            ISetExtraAbilityFactory setAbilityFactory = null)
        {
            if (character == null)
                throw new ArgumentNullException(nameof(character));
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            var startingActive = character.ResolveStartingActiveSkill(registry);

            InitializeCore(
                experience,
                controller,
                definitions,
                startingActive,
                character,
                offerCount,
                draftRandom,
                initialRerolls,
                initialBanishes,
                setDefinitions,
                setAbilityFactory);
        }

        private void InitializeCore(
            PlayerExperienceRuntime experience,
            RunController controller,
            IEnumerable<BuildEntryDefinition> definitions,
            BuildEntryDefinition startingActive,
            CharacterDefinition character,
            int offerCount,
            IDraftRandom draftRandom,
            int initialRerolls,
            int initialBanishes,
            IEnumerable<SetDefinition> setDefinitions,
            ISetExtraAbilityFactory setAbilityFactory)
        {
            if (_initialized)
                throw new InvalidOperationException("Level-up draft runtime is already initialized.");
            if (offerCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(offerCount));

            experienceRuntime = experience != null ? experience : throw new ArgumentNullException(nameof(experience));
            runController = controller != null ? controller : throw new ArgumentNullException(nameof(controller));
            _pool = new DraftPool(definitions, character);
            _draftRandom = draftRandom ?? new SeededDraftRandom(0);
            _offerCount = offerCount;
            Build = new PlayerBuild(startingActive);
            Character = character;
            Controls = new DraftRunControls(initialRerolls, initialBanishes);
            var setList = setDefinitions == null
                ? new List<SetDefinition>()
                : new List<SetDefinition>(setDefinitions);
            SetDefinitions = setList;
            Sets = new PlayerSetRuntime(setList, setAbilityFactory);
            experienceRuntime.LevelUp += HandleLevelUp;
            _initialized = true;
        }

        public bool Select(ContentId id)
        {
            if (!IsDraftOpen || !CurrentDraft.TrySelect(id, out var result))
                return false;

            _pendingDrafts--;
            Sets.Synchronize(Build);
            SelectionApplied?.Invoke(result);

            if (_pendingDrafts > 0)
                OpenNextDraft();
            else if (runController.Model != null)
                runController.Model.ReleasePause(RunPauseReasons.LevelUpDraft);
            return true;
        }

        public bool Reroll()
        {
            if (!IsDraftOpen || !Controls.TryConsumeReroll())
                return false;

            var options = _pool.CreateRerolledOptions(
                Build,
                _offerCount,
                _draftRandom,
                Controls.BanishedIds,
                CurrentDraft.Options);
            if (options.Count == 0)
            {
                ResolveDraftWithoutSelection();
                return true;
            }
            ReplaceCurrentDraft(options);
            return true;
        }

        public bool Banish(ContentId id)
        {
            if (!IsDraftOpen || !IsCurrentOption(id) || !Controls.TryBanish(id))
                return false;

            OpenNextDraft();
            return true;
        }

        public void ResetControlsForNewRun()
        {
            if (!_initialized)
                throw new InvalidOperationException("Level-up draft runtime must be initialized before reset.");
            if (IsDraftOpen || _pendingDrafts > 0)
                throw new InvalidOperationException("Draft controls cannot be reset while a level-up draft is pending.");

            Controls.Reset();
        }

        private void HandleLevelUp(int _)
        {
            _pendingDrafts++;
            if (!IsDraftOpen)
                OpenNextDraft();
        }

        private void OpenNextDraft()
        {
            while (_pendingDrafts > 0)
            {
                var options = _pool.CreateOptions(Build, _offerCount, _draftRandom, Controls.BanishedIds);
                if (options.Count > 0)
                {
                    ReplaceCurrentDraft(options);
                    return;
                }

                // The level has already been awarded. If the build has no eligible
                // acquisition or upgrade left, consume only the pending draft and
                // continue without opening an empty selection window.
                CurrentDraft = null;
                _pendingDrafts--;
            }

            if (runController.Model != null)
                runController.Model.ReleasePause(RunPauseReasons.LevelUpDraft);
        }

        private void ReplaceCurrentDraft(IReadOnlyList<DraftOption> options)
        {
            CurrentDraft = new DraftSession(Build, options);
            DraftOpened?.Invoke(options);
        }

        private void ResolveDraftWithoutSelection()
        {
            CurrentDraft = null;
            _pendingDrafts--;
            OpenNextDraft();
        }

        private bool IsCurrentOption(ContentId id)
        {
            for (var i = 0; i < CurrentDraft.Options.Count; i++)
            {
                if (CurrentDraft.Options[i].Definition.Id == id)
                    return true;
            }
            return false;
        }

        public void Shutdown()
        {
            if (!_initialized)
                return;

            if (experienceRuntime != null)
                experienceRuntime.LevelUp -= HandleLevelUp;
            Sets?.Dispose();
            Sets = null;
            SetDefinitions = Array.Empty<SetDefinition>();
            Character = null;
            // Drop everything Initialize() built so a later Initialize() starts clean
            // (no stale pending drafts or open session from the previous life). Consumers
            // already treat a null Build/Controls as "not initialized".
            CurrentDraft = null;
            _pendingDrafts = 0;
            Build = null;
            Controls = null;
            _pool = null;
            _draftRandom = null;
            _initialized = false;
        }

        private void OnDestroy()
        {
            Shutdown();
        }
    }
}
