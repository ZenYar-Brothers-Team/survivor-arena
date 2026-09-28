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
        private int _bookUpgradeCurrency;
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

        /// <summary>Display name of any build entry this run can offer; null when unknown or not initialized.</summary>
        public string FindDisplayName(ContentId id) =>
            _pool != null && _pool.TryGetDefinition(id, out var definition) ? definition.DisplayName : null;
        /// <summary>True when this run's draft pool contains the id (meta-unlocked or added by a development command).</summary>
        public bool IsInDraftPool(ContentId id) => _pool != null && _pool.TryGetDefinition(id, out _);

        /// <summary>DECISION-0073: false when the set recipe can no longer be fulfilled in this run.</summary>
        public bool CanStillFulfillSet(SetDefinition set)
        {
            if (set == null) throw new ArgumentNullException(nameof(set));
            if (_pool == null || Build == null) return false;
            return set.CanStillBeFulfilled(Build, id => _pool.CanOffer(id) && !Controls.IsBanished(id));
        }

        public PlayerSetRuntime Sets { get; private set; }
        public int RemainingRerolls => Controls?.RemainingRerolls ?? 0;
        public int RemainingBanishes => Controls?.RemainingBanishes ?? 0;

        public event Action<IReadOnlyList<DraftOption>> DraftOpened;
        public event Action<BuildSelectionResult> SelectionApplied;
        public event Action<DraftResolution> RequestResolved;
        public event Action Changed;
        public event Action<DraftRequest> RequestQueued;
        public event Action<DraftControlAttempt> ControlAttempted;

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
            ISetDraftOfferProvider setOffers = null, int bookUpgradeCurrency = 0)
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
                setAbilityFactory, emptyBookCurrency, setOffers, bookUpgradeCurrency);
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
            ISetDraftOfferProvider setOffers = null, int bookUpgradeCurrency = 0)
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
                setAbilityFactory, emptyBookCurrency, setOffers, bookUpgradeCurrency);
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
            ISetExtraAbilityFactory setAbilityFactory, int? emptyBookCurrency, ISetDraftOfferProvider setOffers, int bookUpgradeCurrency)
        {
            if (_initialized)
                throw new InvalidOperationException("Level-up draft runtime is already initialized.");
            NumericValidation.ValidateRange(offerCount, 1, 3, nameof(offerCount));

            experienceRuntime = experience != null ? experience : throw new ArgumentNullException(nameof(experience));
            runController = controller != null ? controller : throw new ArgumentNullException(nameof(controller));
            if (emptyBookCurrency.HasValue) NumericValidation.ValidateNonNegative(emptyBookCurrency.Value, nameof(emptyBookCurrency));
            _owner = controller.Model ?? throw new InvalidOperationException("Run must be initialized before draft.");
            _pool = new DraftPool(definitions, character, setOffers ??
                new FixtureSetDraftOfferProvider(FixtureRunSetupCatalog.Create().Draft.SetDraftChance));
            _emptyBookCurrency = emptyBookCurrency;
            NumericValidation.ValidateNonNegative(bookUpgradeCurrency, nameof(bookUpgradeCurrency));
            _bookUpgradeCurrency = bookUpgradeCurrency;
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

        /// <summary>Development command: adds rerolls for this run only.</summary>
        public void GrantDevelopmentRerolls(int count)
        {
            if (!_initialized) throw new InvalidOperationException("Level-up draft runtime is not initialized.");
            Controls.GrantRerolls(count);
            Changed?.Invoke();
        }

        /// <summary>
        /// Development command: adds entries (e.g. profile-locked skills/passives/sets) to this run's draft pool.
        /// The profile save is not touched; set recipes still apply.
        /// </summary>
        public int AddDevelopmentDraftEntries(IEnumerable<BuildEntryDefinition> definitions)
        {
            if (!_initialized) throw new InvalidOperationException("Level-up draft runtime is not initialized.");
            var added = _pool.AddDefinitions(definitions);
            if (added > 0) Changed?.Invoke();
            return added;
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
            // DECISION-0090: only an applied Book choice earns the extra reward, once.
            if (request.Origin == DraftOrigin.Book) _bookCurrency = checked(_bookCurrency + _bookUpgradeCurrency);
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
            var requestId = CurrentRequest?.Id ?? Guid.Empty;
            if (!CanAct(revision) || !Controls.TryConsumeReroll())
            { ControlAttempted?.Invoke(new DraftControlAttempt("reroll", requestId, revision, null, false)); return false; }
            _setChecks = new SetDraftCheckState();
            var options = _pool.CreateRerolledOptions(Build, _offerCount, _draftRandom,
                Controls.BanishedIds, CurrentDraft.Options, _setChecks);
            if (options.Count == 0) ResolveEmpty();
            else ReplaceCurrentDraft(options);
            ControlAttempted?.Invoke(new DraftControlAttempt("reroll", requestId, revision, null, true));
            return true;
        }

        public bool Banish(ContentId id) => Banish(id, Revision);
        public bool Banish(ContentId id, Guid revision)
        {
            var requestId = CurrentRequest?.Id ?? Guid.Empty;
            if (!CanAct(revision) || !IsCurrentOption(id) || !Controls.TryBanish(id))
            { ControlAttempted?.Invoke(new DraftControlAttempt("banish", requestId, revision, id, false)); return false; }
            var options = _pool.CreateOptions(Build, _offerCount, _draftRandom, Controls.BanishedIds, _setChecks);
            if (options.Count == 0) ResolveEmpty();
            else ReplaceCurrentDraft(options);
            ControlAttempted?.Invoke(new DraftControlAttempt("banish", requestId, revision, id, true));
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
                RequestQueued?.Invoke(request);
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
                var request = DraftRequest.ForLevel(_owner.RunId, level);
                _requests.Enqueue(request);
                RequestQueued?.Invoke(request);
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
            var sets = new List<RunBuildEntrySnapshot>();
            foreach (var entry in Build.Entries)
            {
                entries.Add(new RunBuildEntrySnapshot(entry.Definition.Id.ToString(), entry.Level));
                if (entry.Definition.Kind == BuildEntryKind.Set)
                    sets.Add(new RunBuildEntrySnapshot(entry.Definition.Id.ToString(), entry.Level));
            }
            entries.Sort((a, b) => string.CompareOrdinal(a.ContentId, b.ContentId));
            // Outcome capture occurs before Completed. All outstanding choices are cancelled at terminal.
            var totals = new RunDraftSnapshot(_acceptedBooks, _selections, _emptyRequests,
                _cancelled + _requests.Count, _bookCurrency);
            sets.Sort((a, b) => string.CompareOrdinal(a.ContentId, b.ContentId));
            return new RunOutcomeContribution(build: entries, draftTotals: totals, sets: sets);
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
            _bookUpgradeCurrency = 0;
            _pumping = _resolving = false;
            DraftOpened = null;
            SelectionApplied = null;
            RequestResolved = null;
            Changed = null;
            RequestQueued = null;
            ControlAttempted = null;
        }

        private void OnDestroy() => Shutdown();
    }
}
