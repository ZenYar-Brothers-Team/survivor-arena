using Game.Meta;
using Game.Settings;
using Game.Movement;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Game.Traveler;
using System;
using System.Collections.Generic;
using Game.ActiveSkill;
using Game.Character;
using Game.Content;
using Game.Enemy;
using Game.Field;
using Game.Pickup;
using Game.Presentation;
using Game.Progression;
using Game.Run;
using Game.UI;
using UnityEngine;
using Game.Telemetry;
using Game.Audio;

namespace Game.Bootstrap
{
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class GameplayCompositionRoot : MonoBehaviour, ICharacterRunLauncher, IFieldRunLauncher, IProfileNavigation, IAppNavigation
    {
        [SerializeField]
        private RunController runController;

        [SerializeField]
        private PlayerCharacterRuntime player;

        [SerializeField]
        private SpritePresentationRuntime playerPresentation;

        [SerializeField]
        private PlayerExperienceRuntime experienceRuntime;

        [SerializeField]
        private LevelUpDraftRuntime draftRuntime;

        [SerializeField]
        private PlayerActiveSkillSetRuntime activeSkillRuntime;

        private SetEffectHost _setEffects;
        private CharacterSelectScreen _selectionScreen;
        private FieldSelectScreen _fieldScreen;
        private FieldRoster _fieldRoster;
        private ContentId _pendingCharacterId;
        public FieldSelectionSession FieldSelection { get; private set; }
        public ResolvedFieldConfiguration FieldConfiguration { get; private set; }
        /// <summary>Draft, wave and traveler seeds of the current run: fresh per run (DECISION-0057) unless
        /// <see cref="UseReferenceSeeds"/> pins the reference seeds from content JSON for
        /// comparison runs and deterministic smoke tests.</summary>
        public int DraftSeed { get; private set; }
        public int WaveSeed { get; private set; }
        /// <summary>Seed of this run's obstacle layout (DECISION-0068); 0 for fields without one.</summary>
        public int LayoutSeed { get; private set; }
        public int TravelerSeed { get; private set; }
        /// <summary>Potion drop-roll seed of the current run: fresh per run (DECISION-0074) unless reference seeds are pinned.</summary>
        public int PickupSeed { get; private set; }
        public bool UseReferenceSeeds { get; set; }
        public UnityEngine.UIElements.UIDocument FieldSelectionDocument => _fieldScreen?.Document;
        private Behaviour[] _waitingComponents;
        private bool[] _previousEnabled;
        public CharacterSelectionSession Selection { get; private set; }
        public UnityEngine.UIElements.UIDocument SelectionDocument => _selectionScreen?.Document;

        [SerializeField]
        private PlayerPassiveSetRuntime passiveRuntime;

        [SerializeField]
        private ContinuousFixtureEnemySpawner enemySpawner;

        [SerializeField]
        private GameplayUiRoot gameplayUiRoot;

        public RuntimeContentCatalog Catalog { get; private set; }
        /// <summary>Production save file; independent from the prototype fixture profile (DECISION-0050).</summary>
        public const string ProductionProfileFileName = "profile-meta-r1.json";
        public bool IsInitialized { get; private set; }
        public IPlaytestSession Playtest { get; private set; }
        public BossEncounterRuntime BossEncounters { get; private set; }
        public WorldPickupRuntime Pickups { get; private set; }
        public TravelerEncounterRuntime Travelers { get; private set; }

        public IProfileService Profile { get; private set; }
        private ProfileRunBinding _profileBinding;
        private MetaScreen _metaScreen;
        private MetaPresenter _metaPresenter;
        private bool _allowQuit;
        public UnityEngine.UIElements.UIDocument ProfileDocument => _metaScreen?.Document;
        public Task<bool> ProfileSaveTask => _profileBinding?.SaveTask ?? Task.FromResult(true);

        public event Action NavigationChanged;
        public bool AtMainMenu { get; private set; }
        public bool AtCharacterSelection => _selectionScreen != null;
        public bool AtManualPause => IsInitialized && runController.Model.IsPausedBy(RunPauseReasons.Manual);
        public bool CanPlay => Profile != null && Profile.CanStart;
        public string MovementBindings => player != null ? player.GetComponent<PlayerMover>()?.MovementBindings ?? "Unavailable" : "Unavailable";
        public ISettingsService Settings { get; private set; }
        public UnityEngine.UIElements.UIDocument ShellDocument => _shellScreen?.Document;
        private AppShellScreen _shellScreen;
        private AppShellPresenter _shellPresenter;
        private AudioRoutingRuntime _audio;
        private ProductionAudioCatalog _audioCatalog;
        private RunAudioRuntime _runAudio;
        private SettingsConfig _settingsConfig;
        private CameraShakeRuntime _shake;
        private CameraFollowTarget _cameraFollow;
        private GroundShadowRuntime _playerGroundShadow;
        private FieldEnvironmentArtRuntime _fieldEnvironmentArt;
        private NotificationQueue _notifications;
        private RunNotificationBinding _notificationsBinding;
        private readonly HashSet<string> _knownUnlocks = new HashSet<string>();
        public string Notification => _notifications?.Current ?? "";
        public void ConfigureSettings(ISettingsService settings)
        {
            if (Settings != null) throw new InvalidOperationException("Settings already configured.");
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }
        private async Task InitializeShellAsync()
        {
            _settingsConfig = SettingsConfig.Load();
            if (Settings == null) Settings = new SettingsService(_settingsConfig,
                new FileSettingsStore(Path.Combine(Application.persistentDataPath, "settings-v1.json")), new UnityVideoDevice(_settingsConfig));
            await Settings.LoadAsync();
            if (this == null || !isActiveAndEnabled) return;
            _notifications = new NotificationQueue(_settingsConfig.NotificationSeconds);
            _notifications.Changed += NotifyNavigation;
            foreach (var rule in Profile.Catalog.Unlocks.Values) if (Profile.IsUnlocked(rule.Id)) _knownUnlocks.Add(rule.Id);
            _audio = new AudioRoutingRuntime(transform, Settings, _settingsConfig);
            _audioCatalog = ProductionAudioCatalog.Load();
            _audio.PlayMusic(_audioCatalog.MenuMusic);
            _shellScreen = new AppShellScreen(transform);
            _shellPresenter = new AppShellPresenter(this, Settings, _audio, _shellScreen);
            Profile.Changed += ProfileChanged;
        }
        private string PermanentSummary(ContentId character)
        {
            if (Profile.UpgradesDisabled) return "Permanent bonuses: disabled in Meta progression";
            var modifier = Profile.Modifier(character.ToString());
            return $"Permanent bonuses: HP +{modifier.MaxHealthMultiplierBonus:P0}, damage +{modifier.ActiveSkillDamageMultiplierBonus:P0}";
        }
        private void ProfileChanged()
        {
            foreach (var rule in Profile.Catalog.Unlocks.Values)
                if (Profile.IsUnlocked(rule.Id) && _knownUnlocks.Add(rule.Id))
                    _notifications?.Push((rule.Kind == "character" ? "NEW CHARACTER UNLOCKED — " : rule.Kind == "field" ? "NEW FIELD UNLOCKED — " : "NEW CONTENT UNLOCKED — ") + rule.Name);
            NotifyNavigation();
        }
        private void NotifyNavigation() => NavigationChanged?.Invoke();
        private void PlayMenuCue(string id)
        {
            if (_audioCatalog != null && _audioCatalog.TryGet(id, out var cue, out var clips))
                _audio?.PlayMenuSfx(clips[0], cue.Gain.Value);
        }
        public void Play() { if (CanPlay && AtMainMenu) { PlayMenuCue("ui.confirm"); OpenCharacterSelection(); } }
        public void MainMenu()
        {
            if (!CanPlay) return;
            if (IsInitialized) Shutdown();
            else
            {
                _selectionScreen?.Dispose(); _selectionScreen = null;
                _fieldScreen?.Dispose(); _fieldScreen = null;
                Selection = null; FieldSelection = null;
            }
            _metaPresenter.ClearResult(); AtMainMenu = true;
            if (_audioCatalog != null) _audio?.PlayMusic(_audioCatalog.MenuMusic);
            NotifyNavigation();
        }
        public void Meta()
        {
            if (!AtMainMenu || !CanPlay) return;
            if (Catalog == null) Catalog = CreateCatalog();
            PlayMenuCue("ui.confirm"); AtMainMenu = false; _metaPresenter.OpenShop(); NotifyNavigation();
        }
        public void QuitRun() => QuitProfileRun();
        public void Exit() => Application.Quit();
        public bool DevelopmentTools => Application.isEditor || Debug.isDebugBuild;
        public async void UnlockAllForDevelopment()
        {
            if (!DevelopmentTools || !AtMainMenu || !CanPlay) return;
            try
            {
                // One summary notification instead of a queue of per-unlock announcements.
                foreach (var rule in Profile.Catalog.Unlocks.Values)
                    if (rule.Kind == "character" || rule.Kind == "field") _knownUnlocks.Add(rule.Id);
                if (await Profile.UnlockAllForDevelopmentAsync("character", "field") && this != null)
                    _notifications?.Push("DEV — all characters and fields unlocked");
            }
            catch (Exception exception) { Debug.LogError($"Development unlock failed: {exception}", this); }
        }
        public async void ResetProgressionForDevelopment()
        {
            if (!DevelopmentTools || !AtMainMenu || !CanPlay) return;
            try
            {
                if (!await Profile.ResetForDevelopmentAsync() || this == null) return;
                // Re-baseline so unlocks earned after the reset are announced again.
                _knownUnlocks.Clear();
                foreach (var rule in Profile.Catalog.Unlocks.Values) if (Profile.IsUnlocked(rule.Id)) _knownUnlocks.Add(rule.Id);
                _notifications?.Push("DEV — all progression reset (previous profile preserved)");
            }
            catch (Exception exception) { Debug.LogError($"Development reset failed: {exception}", this); }
        }

        public void ConfigureProfile(IProfileService profile)
        {
            if (Profile != null || IsInitialized) throw new InvalidOperationException("Profile already configured.");
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
        }
        private void EnsureProfileScreen()
        {
            if (Profile == null)
            {
                Profile = new ProfileService(MetaCatalog.Load(true), new MemoryProfileStore());
                // Explicit test/composition entry points use isolated memory, never a user's save.
                Profile.LoadAsync().GetAwaiter().GetResult();
            }
            if (_metaScreen != null) return;
            _metaScreen = new MetaScreen(transform);
            _metaPresenter = new MetaPresenter(Profile, _metaScreen, this, () => Catalog?.Registry);
        }
        private void SuspendForSelection()
        {
            if (_waitingComponents != null) return;
            _waitingComponents = new Behaviour[] { runController, player, playerPresentation, experienceRuntime,
                draftRuntime, activeSkillRuntime, passiveRuntime, enemySpawner, gameplayUiRoot };
            _previousEnabled = new bool[_waitingComponents.Length];
            for (var i = 0; i < _waitingComponents.Length; i++)
            { _previousEnabled[i] = _waitingComponents[i].enabled; _waitingComponents[i].enabled = false; }
        }
        private async void Start()
        {
            try
            {
                if (IsInitialized) return;
                PrepareStartupSurface();
                ValidateSceneReferences();
                SuspendForSelection();
                // Real play uses the production economy and a separate save (F1-08); tests configure a fixture profile.
                if (Profile == null) Profile = new ProfileService(MetaCatalog.Load(),
                    new FileProfileStore(Path.Combine(Application.persistentDataPath, ProductionProfileFileName)));
                EnsureProfileScreen();
                Application.wantsToQuit += WantsToQuit;
                await Profile.LoadAsync();
                if (this == null || !isActiveAndEnabled) return;
                await InitializeShellAsync();
                if (this != null && Profile.CanStart) MainMenu();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Gameplay composition failed: {exception}", this);
                enabled = false;
            }
        }

        private static void PrepareStartupSurface()
        {
            var camera = Camera.main;
            if (camera != null) camera.backgroundColor = new Color32(31, 25, 37, 255);
            var fixture = GameObject.Find("Obstacle_Fixture");
            var renderer = fixture != null ? fixture.GetComponent<SpriteRenderer>() : null;
            if (renderer != null) renderer.enabled = false;
        }

        public void OpenCharacterSelection(ICharacterAccessProvider access = null, IFieldAccessProvider fieldAccess = null)
        {
            if (IsInitialized) throw new InvalidOperationException("Shut down the current run before selecting another character.");
            ValidateSceneReferences();
            EnsureProfileScreen();
            if (!Profile.CanStart) throw new InvalidOperationException("Profile must be saved before selection.");
            AtMainMenu = false;
            _metaPresenter.ClearResult();
            Catalog = CreateCatalog();
            _fieldScreen?.Dispose();
            _fieldScreen = null;
            FieldSelection = null;
            _fieldRoster = new FieldRoster(Catalog.Fields.Roster.AllFields, fieldAccess ?? new ProfileAccessProvider(Profile));
            _selectionScreen?.Dispose();
            _selectionScreen = null;
            SuspendForSelection();
            var roster = new CharacterRoster(Catalog.Characters.AllCharacters, access ?? new ProfileAccessProvider(Profile));
            Selection = new CharacterSelectionSession(roster, Catalog.RunSetup.StartingCharacterId, this);
            _selectionScreen = new CharacterSelectScreen(transform, Selection, Catalog.Registry, PermanentSummary);
            NotifyNavigation();
        }

        public bool TryStartCharacter(ContentId id)
        {
            if (IsInitialized || _selectionScreen == null || Selection == null || !Selection.Roster.TrySelect(id, out var selectedCharacter)) return false;
            PlayMenuCue("ui.confirm");
            _pendingCharacterId = id;
            var previousField = FieldSelection?.SelectedId ?? Catalog.Fields.DefaultFieldId;
            FieldSelection = new FieldSelectionSession(_fieldRoster, previousField, this);
            _fieldScreen = new FieldSelectScreen(transform, FieldSelection, Catalog.Registry, selectedCharacter);
            _selectionScreen?.Dispose();
            _selectionScreen = null;
            NotifyNavigation();
            return true;
        }

        public void BackToCharacters()
        {
            if (IsInitialized || _fieldScreen == null) return;
            PlayMenuCue("ui.back");
            _fieldScreen.Dispose();
            _fieldScreen = null;
            Selection = new CharacterSelectionSession(Selection.Roster, _pendingCharacterId, this);
            _selectionScreen = new CharacterSelectScreen(transform, Selection, Catalog.Registry, PermanentSummary);
            NotifyNavigation();
        }

        public bool TryStartField(ContentId id)
        {
            if (IsInitialized || _fieldScreen == null || FieldSelection == null ||
                !FieldSelection.Roster.TrySelect(id, out _) || !Selection.Roster.TrySelect(_pendingCharacterId, out _)) return false;
            PlayMenuCue("ui.confirm");
            Initialize(_pendingCharacterId, Selection.Roster, id, FieldSelection.Roster);
            _fieldScreen.Dispose();
            _fieldScreen = null;
            RestoreWaitingComponents();
            runController.Model.Start();
            return true;
        }

        private void RestoreWaitingComponents()
        {
            if (_waitingComponents == null) return;
            for (var i = 0; i < _waitingComponents.Length; i++)
                if (_waitingComponents[i] != null) _waitingComponents[i].enabled = _previousEnabled[i];
            _waitingComponents = null;
            _previousEnabled = null;
        }

        // Explicit composition entry point retained for scene integration tests and future navigation.
        public void Initialize() { EnsureProfileScreen(); Initialize(CreateCatalog().RunSetup.StartingCharacterId); }

        /// <summary>
        /// Content follows the profile economy: the fixture profile (tests, prototype tools) composes fixture
        /// content; the production profile composes the FIELD-001 startup content only (F1-08, DECISION-0054).
        /// </summary>
        private RuntimeContentCatalog CreateCatalog() =>
            Profile != null && Profile.Catalog.IsFixture ? RuntimeContentCatalog.CreateFixture() : RuntimeContentCatalog.CreateProduction();

        public void Initialize(ContentId characterId, CharacterRoster roster = null, ContentId? fieldId = null, FieldRoster fields = null)
        {
            if (IsInitialized)
                throw new InvalidOperationException("Gameplay composition root is already initialized.");
            ValidateSceneReferences();
            EnsureProfileScreen();
            if (!Profile.CanStart) throw new InvalidOperationException("Profile must be saved before starting a run.");

            // Run parameters (starting character, draft settings, XP curve) are content,
            // validated by their domain types when the catalog loads.
            Catalog = CreateCatalog();
            var setup = Catalog.RunSetup;
            if (!(roster ?? new CharacterRoster(Catalog.Characters.AllCharacters, new ProfileAccessProvider(Profile))).TrySelect(characterId, out var selectedCharacter))
                throw new InvalidOperationException($"Character '{characterId}' is locked or missing.");
            if (!(fields ?? new FieldRoster(Catalog.Fields.Roster.AllFields, new ProfileAccessProvider(Profile))).TrySelect(fieldId ?? Catalog.Fields.DefaultFieldId, out var selectedField))
                throw new InvalidOperationException("Field is locked or missing.");
            var configuration = selectedField.Resolve(Catalog.Registry);
            if (configuration.Travelers != null && configuration.Travelers is not TravelerScheduleDefinition)
                throw new InvalidOperationException("Traveler schedule requires a concrete encounter payload.");
            var spawn = FieldEnvironmentBinding.Validate(configuration.Environment, gameObject.scene);

            // If a subsystem's Initialize() throws partway through, every subsystem
            // that already succeeded gets rolled back (in reverse order) via its
            // Shutdown() before the exception propagates, so a failed composition
            // never leaves some subsystems live-subscribed and others untouched.
            if (!runController.IsInitialized) runController.Initialize();
            if (Settings != null)
            {
                _settingsConfig ??= SettingsConfig.Load();
                player.GetComponent<PlayerMover>()?.ConfigureMouseMovement(() => Settings.Current.MouseMovement, Camera.main, _settingsConfig.MouseDeadzoneWorldUnits);
            }
            var initializedSubsystems = new List<Action>();
            try
            {
                runController.Model.ConfigureSelection(new RunSelectionSnapshot(characterId, selectedField.Id,
                    configuration.Environment.Id, configuration.Timeline.Id));
                initializedSubsystems.Add(runController.Shutdown);
                if (!Catalog.FieldEnvironmentPresentations.TryGetValue(configuration.Environment.Id, out var fieldPresentation))
                    throw new InvalidOperationException($"Environment '{configuration.Environment.Id}' requires presentation content.");
                _fieldEnvironmentArt = new FieldEnvironmentArtRuntime();
                // DECISION-0068: fields with a pattern layout get a fresh obstacle arrangement every run.
                var layout = fieldPresentation.ObstacleLayout;
                LayoutSeed = layout == null ? 0 : UseReferenceSeeds ? layout.ReferenceSeed : FreshRunSeed.Next();
                var arenaSideLength = FixtureArenaGeometryCatalog.Create().SideLength;
                _fieldEnvironmentArt.Initialize(fieldPresentation, Catalog.Registry, configuration.Environment,
                    gameObject.scene, arenaSideLength, layout == null ? (int?)null : LayoutSeed);
                initializedSubsystems.Add(() => { _fieldEnvironmentArt?.Dispose(); _fieldEnvironmentArt = null; });
                _cameraFollow = Camera.main.GetComponent<CameraFollowTarget>();
                if (_cameraFollow == null) throw new InvalidOperationException("Gameplay camera requires CameraFollowTarget.");
                _cameraFollow.ConfigureBounds(arenaSideLength);
                initializedSubsystems.Add(() => { _cameraFollow?.ClearBounds(); _cameraFollow = null; });
                var previousPosition = player.transform.position;
                var body = player.GetComponent<Rigidbody2D>();
                player.transform.position = spawn.position;
                if (body != null) { body.position = spawn.position; body.linearVelocity = Vector2.zero; body.angularVelocity = 0; }
                initializedSubsystems.Add(() => { player.transform.position = previousPosition; if (body != null) body.position = previousPosition; });
                player.Initialize(selectedCharacter.BaseStats.WithIncomingDamageScale(setup.HostileDamageMultiplier), runController, selectedCharacter.Id, Profile.Modifier(selectedCharacter.Id.ToString()));
                initializedSubsystems.Add(player.Shutdown);

                if (!selectedCharacter.Visual.Id.IsValid || !selectedCharacter.MotionProfile.Id.IsValid)
                    throw new InvalidOperationException(
                        $"Character '{selectedCharacter.Id}' requires visual and motion profile references.");
                var playerVisual = selectedCharacter.Visual.Resolve(Catalog.Registry);
                var playerCollider = player.GetComponent<CircleCollider2D>();
                if (playerCollider == null)
                    throw new InvalidOperationException("Fixture player requires a circular collider.");
                playerVisual.Contact?.Apply(playerCollider);
                var playerMotionProfile = selectedCharacter.MotionProfile.Resolve(Catalog.Registry);
                var playerBody = player.GetComponent<Rigidbody2D>();
                if (playerBody == null)
                    throw new InvalidOperationException("Player presentation requires a Rigidbody2D motion source.");
                playerPresentation.Initialize(
                    playerVisual,
                    playerMotionProfile,
                    player.Health,
                    playerBody,
                    runController);
                initializedSubsystems.Add(playerPresentation.Shutdown);
                if (_playerGroundShadow == null) _playerGroundShadow = player.gameObject.AddComponent<GroundShadowRuntime>();
                var playerRig = playerPresentation.GetComponent<SpritePresentationRig>();
                _playerGroundShadow.Initialize(Catalog.GroundShadowPresentation, playerVisual.Contact, 1f,
                    playerRig.BodyRenderer, playerRig.ShadowRenderer);
                initializedSubsystems.Add(_playerGroundShadow.Shutdown);

                var experienceVisual = Catalog.Pickups.ExperienceVisual.Resolve(Catalog.Registry);
                experienceVisual.RequireRole(SpriteRole.Pickup);
                experienceRuntime.Initialize(player, runController, setup.Experience, experienceVisual,
                    Catalog.Pickups.ExperienceVisualScale, Catalog.Pickups.DropScatterRadius,
                    UseReferenceSeeds ? Catalog.Pickups.DropScatterSeed : FreshRunSeed.Next());
                initializedSubsystems.Add(experienceRuntime.Shutdown);

                // DECISION-0058: player attacks choose enemies/points only on the visible screen.
                var targetViewport = new CameraTargetViewport(Camera.main);
                _setEffects = new SetEffectHost(player, runController, activeSkillRuntime, experienceRuntime.Progression,
                    Catalog.ActiveSkills.Concat(Catalog.SetAttackTemplates), Catalog.SkillWorldEffects, targetViewport,
                    Catalog.Registry);
                initializedSubsystems.Add(_setEffects.Dispose);
                draftRuntime.Initialize(
                    experienceRuntime,
                    runController,
                    Catalog.BuildEntries.Where(entry => Profile.IsUnlocked(entry.Id.ToString())),
                    selectedCharacter,
                    Catalog.Registry,
                    setup.Draft.OfferCount,
                    new SeededDraftRandom(DraftSeed = UseReferenceSeeds ? setup.Draft.Seed : FreshRunSeed.Next()),
                    checked(setup.Draft.InitialRerolls + Profile.ExtraRerolls(selectedCharacter.Id.ToString())),
                    checked(setup.Draft.InitialBanishes + Profile.ExtraBanishes(selectedCharacter.Id.ToString())),
                    Catalog.Sets,
                    new SetEffectAbilityFactory(_setEffects),
                    checked((int)Profile.Catalog.EmptyBookReward),
                    new FixtureSetDraftOfferProvider(setup.Draft.SetDraftChance), Profile.Catalog.BookUpgradeReward,
                    setup.Draft.BookUpgradeCount);
                initializedSubsystems.Add(draftRuntime.Shutdown);

                // The executor owns a scene GameObject (mine pool root); it is registered for
                // rollback before Initialize so a failed Initialize cannot leak it (Dispose is
                // idempotent, and Shutdown disposes it again on the success path).
                var effectExecutor = new SceneActiveSkillEffectExecutor(runController, contentRegistry: Catalog.Registry,
                    worldEffectProfiles: Catalog.SkillWorldEffects, viewport: targetViewport);
                initializedSubsystems.Add(effectExecutor.Dispose);
                activeSkillRuntime.Initialize(
                    player,
                    runController,
                    draftRuntime,
                    Catalog.ActiveSkills,
                    new SceneEnemyTargetProvider(viewport: targetViewport),
                    effectExecutor);
                initializedSubsystems.Add(activeSkillRuntime.Shutdown);

                passiveRuntime.Initialize(player, draftRuntime, Catalog.Passives);
                initializedSubsystems.Add(passiveRuntime.Shutdown);

                if (Pickups == null) Pickups = gameObject.AddComponent<WorldPickupRuntime>();
                var pickupVisuals = Catalog.Pickups.Definitions.ToDictionary(definition => definition.Id,
                    definition => definition.Visual.Resolve(Catalog.Registry));
                Pickups.Initialize(Catalog.Pickups, runController.Model, player,
                    new PlayerPickupRewardTarget(player, runController.Model, draftRuntime, _setEffects.PublishReward),
                    FixturePickupPlacement.ArenaBounds(configuration.Environment, gameObject.scene), selectedField.Id, pickupVisuals,
                    PickupSeed = UseReferenceSeeds ? Catalog.Pickups.Seed : FreshRunSeed.Next(),
                    UseReferenceSeeds ? Catalog.Pickups.DropScatterSeed : FreshRunSeed.Next());
                initializedSubsystems.Add(Pickups.Shutdown);

                var enemiesById = new Dictionary<ContentId, EnemyDefinition>(configuration.Enemies.Count);
                var enemyVisuals = new Dictionary<ContentId, Sprite>(configuration.Enemies.Count);
                var enemyMotions = new Dictionary<ContentId, SpriteMotionProfile>(configuration.Enemies.Count);
                var enemyContacts = new Dictionary<ContentId, SpriteContactProfile>(configuration.Enemies.Count);
                for (var i = 0; i < configuration.Enemies.Count; i++)
                {
                    var enemy = configuration.Enemies[i];
                    enemiesById.Add(enemy.Id, enemy);
                    var enemyBody = EnemyBodyVisual.Resolve(enemy, Catalog.Registry);
                    if (enemyBody.Sprite != null) enemyVisuals.Add(enemy.Id, enemyBody.Sprite);
                    if (enemyBody.Contact != null) enemyContacts.Add(enemy.Id, enemyBody.Contact);
                    if (enemyBody.Motion != null) enemyMotions.Add(enemy.Id, enemyBody.Motion);
                }
                var waveDirector = new WaveDirector(
                    configuration.Timeline,
                    enemiesById,
                    runController.Model.Duration,
                    WaveSeed = UseReferenceSeeds ? configuration.Timeline.Seed : FreshRunSeed.Next());
                enemySpawner.Initialize(waveDirector, enemyVisuals,
                    new EnemyRewardSink(new EnemyExperienceDropSink(experienceRuntime, runController), Pickups),
                    enemyMotions, enemyContacts, Catalog.EnemyDeathPresentation, Catalog.GroundShadowPresentation,
                    Catalog.Registry, Camera.main);
                initializedSubsystems.Add(enemySpawner.Shutdown);

                if (BossEncounters == null) BossEncounters = gameObject.AddComponent<BossEncounterRuntime>();
                BossEncounters.Initialize(waveDirector, runController, player.transform, configuration.Bosses,
                    new EnemyExperienceDropSink(experienceRuntime, runController), Catalog.EnemyDeathPresentation,
                    Catalog.GroundShadowPresentation, Catalog.Registry,
                    new EnemyRewardSink(new EnemyExperienceDropSink(experienceRuntime, runController), Pickups));
                initializedSubsystems.Add(BossEncounters.Shutdown);

                if (configuration.Travelers is TravelerScheduleDefinition travelerSchedule)
                {
                    if (Travelers == null) Travelers = gameObject.AddComponent<TravelerEncounterRuntime>();
                    initializedSubsystems.Add(Travelers.Shutdown);
                    var travelerPlacement = FixturePickupPlacement.Create(configuration.Environment, gameObject.scene,
                        player.GetComponent<Collider2D>(), Catalog.Pickups.PlacementSkin,
                        Catalog.Travelers.Definitions.Values.Max(item => item.Body.CollisionSize * .5f),
                        _fieldEnvironmentArt.ObstacleColliders);
                    Travelers.Initialize(travelerSchedule, Catalog.Travelers, runController, player.transform,
                        Camera.main, new TravelerPlacement(travelerPlacement), Pickups, Catalog.Pickups.Book,
                        new EnemyExperienceDropSink(experienceRuntime, runController), Catalog.EnemyDeathPresentation,
                        Catalog.GroundShadowPresentation, Catalog.Registry,
                        TravelerSeed = UseReferenceSeeds ? travelerSchedule.Seed : FreshRunSeed.Next());
                }
                Playtest = PlaytestComposition.Create(Catalog, DraftSeed, runController.Model, player, experienceRuntime,
                    draftRuntime, enemySpawner, activeSkillRuntime, Pickups, Travelers);
                if (Playtest is PlaytestSession session) initializedSubsystems.Add(session.Dispose);

                gameplayUiRoot.Initialize(
                    player,
                    experienceRuntime,
                    draftRuntime,
                    runController,
                    playerPresentation,
                    Catalog.Registry,
                    (roster ?? new CharacterRoster(Catalog.Characters.AllCharacters, new ProfileAccessProvider(Profile))).UnlockedCharacters,
                    enemySpawner,
                    Playtest,
                    BossEncounters, Pickups, Travelers,
                    Catalog.BuildEntries); // development "unlock all" draws from the whole catalog, never the save
                initializedSubsystems.Add(gameplayUiRoot.Shutdown);
                _shellScreen?.AttachPauseActions(gameplayUiRoot.PauseFooter);
                initializedSubsystems.Add(() => _shellScreen?.AttachPauseActions(null));
                _profileBinding = new ProfileRunBinding(runController.Model, Profile);
                initializedSubsystems.Add(_profileBinding.Dispose);
                _notifications?.Clear();
                _notificationsBinding = new RunNotificationBinding(runController.Model, experienceRuntime, draftRuntime,
                    BossEncounters, Travelers, Catalog.Sets, _notifications);
                initializedSubsystems.Add(_notificationsBinding.Dispose);
                if (_audioCatalog != null && _audio != null)
                {
                    _runAudio = new RunAudioRuntime(transform, _audioCatalog, Settings, _audio,
                        runController.Model, player.Health, experienceRuntime, draftRuntime,
                        activeSkillRuntime, Pickups, enemySpawner, BossEncounters,
                        selectedField.Id.ToString());
                    initializedSubsystems.Add(() => { _runAudio?.Dispose(); _runAudio = null; });
                }
                runController.Model.Completed += ShowProfileResult;
            }
            catch
            {
                for (var i = initializedSubsystems.Count - 1; i >= 0; i--)
                    initializedSubsystems[i]();
                throw;
            }

            IsInitialized = true;
            FieldConfiguration = configuration;
            player.ShuttingDown += Shutdown;
            AtMainMenu = false;
            runController.Model.StateChanged += ShellRunStateChanged;
            runController.Model.PauseChanged += ShellPauseChanged;
            _audio?.Bind(runController.Model);
            if (Settings != null)
            {
                _shake = gameObject.AddComponent<CameraShakeRuntime>();
                _shake.Initialize(Camera.main, player.Health, runController.Model, Settings, _settingsConfig);
            }
            gameplayUiRoot.BindHealthAnchor(Camera.main);
            NotifyNavigation();
        }

        private void ValidateSceneReferences()
        {
            if (runController == null || player == null || playerPresentation == null ||
                experienceRuntime == null || draftRuntime == null ||
                activeSkillRuntime == null || passiveRuntime == null || enemySpawner == null || gameplayUiRoot == null)
            {
                throw new InvalidOperationException("Gameplay composition root has missing scene references.");
            }
        }

        private void ShellRunStateChanged(RunState state) => NotifyNavigation();
        private void ShellPauseChanged(string reason, bool paused) => NotifyNavigation();
        private void LateUpdate()
        {
            if (Playtest is PlaytestSession session) session.Tick();
            Settings?.Tick(Time.realtimeSinceStartupAsDouble);
            _notifications?.Tick(Time.unscaledDeltaTime, IsInitialized && runController.Model.State == RunState.Paused);
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                if (_shellPresenter?.SettingsOpen == true) { _shellPresenter.Back(); return; }
                if (AtCharacterSelection) { MainMenu(); return; }
                if (gameplayUiRoot == null || !gameplayUiRoot.ConsumePauseShortcut(false)) ToggleManualPauseFromShortcut();
                return;
            }
            if (Keyboard.current?.spaceKey.wasPressedThisFrame == true || Mouse.current?.rightButton.wasPressedThisFrame == true)
                if (gameplayUiRoot == null || !gameplayUiRoot.ConsumePauseShortcut(Keyboard.current?.spaceKey.wasPressedThisFrame == true))
                    ToggleManualPauseFromShortcut();
        }

        private void ToggleManualPauseFromShortcut()
        {
            if (!IsInitialized || _shellPresenter?.SettingsOpen == true) return;
            if (runController.Model.State == RunState.Running || AtManualPause)
                runController.TogglePause();
        }

        /// <summary>Captures the run, then unwinds consumers before their producers. Idempotent.</summary>
        public void Shutdown()
        {
            _selectionScreen?.Dispose();
            _selectionScreen = null;
            _fieldScreen?.Dispose();
            _fieldScreen = null;
            Selection = null;
            FieldSelection = null;
            // A cancelled selection must not enable components whose Start expects a composed run.
            // Keep the original enabled-state snapshot for the next selection/launch attempt.
            if (!IsInitialized) { runController?.Shutdown(); return; }
            IsInitialized = false;
            player.ShuttingDown -= Shutdown;
            runController.Model.StateChanged -= ShellRunStateChanged;
            runController.Model.PauseChanged -= ShellPauseChanged;
            _audio?.Bind(null);
            _runAudio?.Dispose(); _runAudio = null;
            if (_shake != null) { _shake.Shutdown(); Destroy(_shake); _shake = null; }
            // Capture required Results while every contributor is still alive, then diagnostics.
            runController.Shutdown();
            runController.Model.Completed -= ShowProfileResult;
            _profileBinding?.Dispose();
            _notificationsBinding?.Dispose(); _notificationsBinding = null;
            _shellScreen?.AttachPauseActions(null);
            gameplayUiRoot.Shutdown();
            if (Playtest is PlaytestSession session) session.Dispose();
            BossEncounters?.Shutdown();
            enemySpawner.Shutdown();
            Travelers?.Shutdown();
            Pickups?.Shutdown();
            passiveRuntime.Shutdown();
            activeSkillRuntime.Shutdown();
            draftRuntime.Shutdown();
            _setEffects?.Dispose();
            _setEffects = null;
            experienceRuntime.Shutdown();
            playerPresentation.Shutdown();
            _playerGroundShadow?.Shutdown();
            _fieldEnvironmentArt?.Dispose();
            _fieldEnvironmentArt = null;
            _cameraFollow = null;
            player.Shutdown();
            FieldConfiguration = null;
        }

        private void ShowProfileResult(RunOutcome result) { _metaPresenter?.ShowResult(result); NotifyNavigation(); }
        public void QuitProfileRun() { if (IsInitialized) runController.Model.Stop(); }
        public void RetryProfileRun()
        {
            if (!Profile.CanStart || runController.Model.Outcome?.Selection == null) return;
            var selection = runController.Model.Outcome.Selection;
            Shutdown();
            _metaPresenter.ClearResult();
            Initialize(selection.CharacterId, null, selection.FieldId);
            RestoreWaitingComponents();
            runController.Model.Start();
        }
        public void ReturnToProfileSelection()
        {
            if (!Profile.CanStart) return;
            MainMenu();
        }
        private bool _quitRequested;
        private bool WantsToQuit()
        {
            if (_allowQuit || Profile == null) return true;
            if (!_quitRequested)
            {
                _quitRequested = true;
                if (Profile.RunActive) runController.Model.Stop();
                FinishQuitAsync();
            }
            return false;
        }
        private async void FinishQuitAsync()
        {
            if (Settings != null) await Settings.CloseAsync();
            if (Profile.State == ProfileState.Saving)
            {
                var completed = new TaskCompletionSource<bool>();
                void Changed() { if (Profile.State != ProfileState.Saving) completed.TrySetResult(true); }
                Profile.Changed += Changed;
                try { Changed(); await completed.Task; }
                finally { Profile.Changed -= Changed; }
            }
            // Closing is still allowed after a failed best-effort save (DECISION-0037).
            _allowQuit = true;
            Application.Quit();
        }
        private void OnDisable() => Shutdown();
        private void OnDestroy()
        {
            Application.wantsToQuit -= WantsToQuit;
            Shutdown();
            if (Profile != null) Profile.Changed -= ProfileChanged;
            if (_notifications != null) _notifications.Changed -= NotifyNavigation;
            _shellPresenter?.Dispose(); _shellScreen?.Dispose(); _audio?.Dispose();
            _metaPresenter?.Dispose(); _metaScreen?.Dispose();
        }
    }
}
