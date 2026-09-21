using System;
using System.Collections.Generic;
using Game.Content;
using Game.Run;
using UnityEngine;

namespace Game.Progression
{
    [DisallowMultipleComponent]
    public sealed class LevelUpDraftRuntime : MonoBehaviour, IRunOutcomeContributor
    {
        [SerializeField]
        private PlayerExperienceRuntime experienceRuntime;

        [SerializeField]
        private RunController runController;

        private DraftPool _pool;
        private SetDraftCheckState _setChecks;
        private IDraftRandom _draftRandom;
        private int _offerCount;
        private readonly Queue<DraftRequest> _requests = new Queue<DraftRequest>();
        private readonly HashSet<Guid> _bookPickups = new HashSet<Guid>();
        private RunModel _owner;
        private int? _emptyBookCurrency;
        private bool _pumping;
        private bool _resolving;
        private int _acceptedBooks, _selections, _emptyRequests, _cancelled;
        private long _bookCurrency;
        private bool _initialized;

        public PlayerBuild Build { get; private set; }
        public CharacterDefinition Character { get; private set; }
        public DraftSession CurrentDraft { get; private set; }
        public bool IsDraftOpen => CurrentDraft != null && CurrentDraft.IsOpen;
        public int PendingDraftCount => _requests.Count;
        public DraftRequest CurrentRequest => _requests.Count == 0 ? null : _requests.Peek();
        public DraftRequest NextRequest
        {
            get
            {
                using var items = _requests.GetEnumerator();
                return items.MoveNext() && items.MoveNext() ? items.Current : null;
            }
        }
        public Guid Revision => IsDraftOpen ? CurrentDraft.Revision : Guid.Empty;
        public long BookCurrency => _bookCurrency;
        public string Key => "draft";
        public RunDraftSnapshot Totals => new RunDraftSnapshot(_acceptedBooks, _selections, _emptyRequests, _cancelled, _bookCurrency);
        public DraftRunControls Controls { get; private set; }
        public IReadOnlyList<SetDefinition> SetDefinitions { get; private set; } = Array.Empty<SetDefinition>();
        public PlayerSetRuntime Sets { get; private set; }
        public int RemainingRerolls => Controls?.RemainingRerolls ?? 0;
        public int RemainingBanishes => Controls?.RemainingBanishes ?? 0;

        public event Action<IReadOnlyList<DraftOption>> DraftOpened;
        public event Action<BuildSelectionResult> SelectionApplied;
        public event Action<DraftResolution> RequestResolved;
        public event Action Changed;

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
            ISetExtraAbilityFactory setAbilityFactory = null,
            int? emptyBookCurrency = null,
            ISetDraftOfferProvider setOffers = null)
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
                setAbilityFactory, emptyBookCurrency, setOffers);
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
            ISetExtraAbilityFactory setAbilityFactory = null,
            int? emptyBookCurrency = null,
            ISetDraftOfferProvider setOffers = null)
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
                setAbilityFactory, emptyBookCurrency, setOffers);
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
            ISetExtraAbilityFactory setAbilityFactory, int? emptyBookCurrency, ISetDraftOfferProvider setOffers)
        {
            if (_initialized)
                throw new InvalidOperationException("Level-up draft runtime is already initialized.");
            NumericValidation.ValidateRange(offerCount, 1, 3, nameof(offerCount));

            experienceRuntime = experience != null ? experience : throw new ArgumentNullException(nameof(experience));
            runController = controller != null ? controller : throw new ArgumentNullException(nameof(controller));
            if (emptyBookCurrency.HasValue) NumericValidation.ValidateCount(emptyBookCurrency.Value, nameof(emptyBookCurrency));
            _owner = controller.Model ?? throw new InvalidOperationException("Run must be initialized before draft.");
            _pool = new DraftPool(definitions, character, setOffers);
            _emptyBookCurrency = emptyBookCurrency;
            _draftRandom = draftRandom ?? new SeededDraftRandom(0);
            _offerCount = offerCount;
            Build = new PlayerBuild(startingActive);
            Character = character;
            Controls = new DraftRunControls(initialRerolls, initialBanishes);
            var setList = setDefinitions == null
                ? new List<SetDefinition>()
                : new List<SetDefinition>(setDefinitions);
            SetDefinitions = setList.AsReadOnly();
            Sets = new PlayerSetRuntime(setList, setAbilityFactory);
            try { _owner.RegisterOutcomeContributor(this); }
            catch { Sets.Dispose(); Sets = null; throw; }
            experienceRuntime.LevelsEarned += HandleLevelsEarned;
            _owner.Completed += HandleCompleted;
            _initialized = true;
        }

        // Compatibility entry points for direct model callers. UI always supplies its captured revision.
        public bool Select(ContentId id) => Select(id, Revision);
        public bool Select(ContentId id, Guid revision)
        {
            if (!CanAct(revision) || !CurrentDraft.TrySelect(id, out var result)) return false;
            _resolving = true;
            var request = _requests.Dequeue();
            CurrentDraft = null;
            _selections++;
            try
            {
                Sets.Synchronize(Build);
                SelectionApplied?.Invoke(result);
                RequestResolved?.Invoke(new DraftResolution(request, DraftResolutionKind.Selected, id));
            }
            finally
            {
                _resolving = false;
                OpenNextDraft();
            }
            return true;
        }

        public bool Reroll() => Reroll(Revision);
        public bool Reroll(Guid revision)
        {
            if (!CanAct(revision) || !Controls.TryConsumeReroll()) return false;
            _setChecks = new SetDraftCheckState();
            var options = _pool.CreateRerolledOptions(Build, _offerCount, _draftRandom,
                Controls.BanishedIds, CurrentDraft.Options, _setChecks);
            if (options.Count == 0) ResolveEmpty();
            else ReplaceCurrentDraft(options);
            return true;
        }

        public bool Banish(ContentId id) => Banish(id, Revision);
        public bool Banish(ContentId id, Guid revision)
        {
            if (!CanAct(revision) || !IsCurrentOption(id) || !Controls.TryBanish(id)) return false;
            var options = _pool.CreateOptions(Build, _offerCount, _draftRandom, Controls.BanishedIds, _setChecks);
            if (options.Count == 0) ResolveEmpty();
            else ReplaceCurrentDraft(options);
            return true;
        }

        /// <summary>
        /// Complete an already accepted Book pickup, including callbacks during a draft pause.
        /// World-pickup adapters must gate collection on Running. Source run/life IDs are mandatory.
        /// DECISION-0020: empty at pickup awards currency now; later queue/banish exhaustion does not.
        /// </summary>
        public bool RequestBook(Guid pickupId, Guid sourceRunId, ContentId sourceContentId)
        {
            if (!CanProcess || sourceRunId != _owner.RunId || !_emptyBookCurrency.HasValue ||
                pickupId == Guid.Empty || !sourceContentId.IsValid || _bookPickups.Contains(pickupId)) return false;
            var request = DraftRequest.ForBook(sourceRunId, pickupId, sourceContentId);
            var empty = !_pool.HasEligibleOptions(Build, Controls.BanishedIds);
            var total = empty ? checked(_bookCurrency + _emptyBookCurrency.Value) : _bookCurrency;
            _bookPickups.Add(pickupId);
            _acceptedBooks++;
            _bookCurrency = total;
            if (empty)
            {
                _emptyRequests++;
                RequestResolved?.Invoke(new DraftResolution(request, DraftResolutionKind.BookCurrency,
                    currencyAmount: _emptyBookCurrency.Value));
                Changed?.Invoke();
            }
            else
            {
                _requests.Enqueue(request);
                OpenNextDraft();
            }
            return true;
        }

        public void ResetControlsForNewRun()
        {
            if (!_initialized) throw new InvalidOperationException("Draft must be initialized before reset.");
            if (PendingDraftCount > 0) throw new InvalidOperationException("A draft is pending.");
            Controls.Reset();
        }

        private bool CanProcess => _initialized && _owner != null && _owner.Outcome == null &&
            (_owner.State == RunState.Running || _owner.State == RunState.Paused);
        private bool CanAct(Guid revision) => CanProcess && !_resolving && IsDraftOpen &&
            revision != Guid.Empty && revision == CurrentDraft.Revision;

        private void HandleLevelsEarned(int firstLevel, int lastLevel)
        {
            if (!CanProcess) return;
            // Enqueue the complete XP award before any DraftOpened callback can enqueue a Book.
            for (var level = firstLevel; ; level++)
            {
                _requests.Enqueue(DraftRequest.ForLevel(_owner.RunId, level));
                if (level == lastLevel) break;
            }
            OpenNextDraft();
        }

        private void OpenNextDraft()
        {
            if (!CanProcess || _pumping || _resolving) return;
            _pumping = true;
            try
            {
                if (_requests.Count > 0) _owner.RequestPause(RunPauseReasons.LevelUpDraft);
                while (CanProcess && _requests.Count > 0 && !IsDraftOpen)
                {
                    _setChecks = new SetDraftCheckState();
                    var options = _pool.CreateOptions(Build, _offerCount, _draftRandom, Controls.BanishedIds, _setChecks);
                    if (options.Count > 0) ReplaceCurrentDraft(options);
                    else
                    {
                        var request = _requests.Dequeue();
                        _emptyRequests++;
                        RequestResolved?.Invoke(new DraftResolution(request, DraftResolutionKind.Empty));
                    }
                }
                if (CanProcess && _requests.Count == 0) _owner.ReleasePause(RunPauseReasons.LevelUpDraft);
            }
            finally { _pumping = false; Changed?.Invoke(); }
        }

        private void ReplaceCurrentDraft(IReadOnlyList<DraftOption> options)
        {
            CurrentDraft?.Cancel();
            CurrentDraft = new DraftSession(Build, options);
            DraftOpened?.Invoke(CurrentDraft.Options);
            Changed?.Invoke();
        }

        private void ResolveEmpty()
        {
            CurrentDraft.Cancel();
            CurrentDraft = null;
            var request = _requests.Dequeue();
            _emptyRequests++;
            RequestResolved?.Invoke(new DraftResolution(request, DraftResolutionKind.Empty));
            OpenNextDraft();
        }

        private bool IsCurrentOption(ContentId id)
        {
            foreach (var option in CurrentDraft.Options)
                if (option.Definition.Id == id) return true;
            return false;
        }

        public RunOutcomeContribution Capture()
        {
            var entries = new List<RunBuildEntrySnapshot>();
            foreach (var entry in Build.Entries)
                entries.Add(new RunBuildEntrySnapshot(entry.Definition.Id.ToString(), entry.Level));
            entries.Sort((a, b) => string.CompareOrdinal(a.ContentId, b.ContentId));
            // Outcome capture occurs before Completed. All outstanding choices are cancelled at terminal.
            var totals = new RunDraftSnapshot(_acceptedBooks, _selections, _emptyRequests,
                _cancelled + _requests.Count, _bookCurrency);
            return new RunOutcomeContribution(build: entries, draftTotals: totals);
        }

        private void HandleCompleted(RunOutcome _) => CancelRequests();

        private void CancelRequests()
        {
            CurrentDraft?.Cancel();
            CurrentDraft = null;
            var cancelled = _requests.ToArray();
            _requests.Clear();
            _cancelled += cancelled.Length;
            foreach (var request in cancelled)
                RequestResolved?.Invoke(new DraftResolution(request, DraftResolutionKind.Cancelled));
            Changed?.Invoke();
        }

        public void Shutdown()
        {
            if (experienceRuntime != null) experienceRuntime.LevelsEarned -= HandleLevelsEarned;
            if (_owner != null)
            {
                _owner.Completed -= HandleCompleted;
                _owner.UnregisterOutcomeContributor(this);
            }
            _initialized = false;
            CancelRequests();
            _owner?.ReleasePause(RunPauseReasons.LevelUpDraft);
            _owner = null;
            Sets?.Dispose();
            Sets = null;
            SetDefinitions = Array.Empty<SetDefinition>();
            Character = null;
            Build = null;
            Controls = null;
            _pool = null;
            _setChecks = null;
            _draftRandom = null;
            _bookPickups.Clear();
            _acceptedBooks = _selections = _emptyRequests = _cancelled = 0;
            _bookCurrency = 0;
            _emptyBookCurrency = null;
            _pumping = _resolving = false;
            DraftOpened = null;
            SelectionApplied = null;
            RequestResolved = null;
            Changed = null;
        }

        private void OnDestroy() => Shutdown();
    }
}
