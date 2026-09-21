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

namespace Game.Bootstrap
{
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class GameplayCompositionRoot : MonoBehaviour, ICharacterRunLauncher, IFieldRunLauncher
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

        public FixtureRuntimeContentCatalog Catalog { get; private set; }
        public bool IsInitialized { get; private set; }
        public IPlaytestSession Playtest { get; private set; }
        public BossEncounterRuntime BossEncounters { get; private set; }
        public WorldPickupRuntime Pickups { get; private set; }

        private void Start()
        {
            try
            {
                if (!IsInitialized) OpenCharacterSelection();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Gameplay composition failed: {exception}", this);
                enabled = false;
            }
        }

        public void OpenCharacterSelection(ICharacterAccessProvider access = null, IFieldAccessProvider fieldAccess = null)
        {
            if (IsInitialized) throw new InvalidOperationException("Shut down the current run before selecting another character.");
            ValidateSceneReferences();
            Catalog = FixtureRuntimeContentCatalog.Create();
            _fieldScreen?.Dispose();
            _fieldScreen = null;
            FieldSelection = null;
            _fieldRoster = fieldAccess == null ? Catalog.Fields.Roster : new FieldRoster(Catalog.Fields.Roster.AllFields, fieldAccess);
            _selectionScreen?.Dispose();
            _selectionScreen = null;
            if (_waitingComponents == null)
            {
                _waitingComponents = new Behaviour[] { runController, player, playerPresentation, experienceRuntime,
                    draftRuntime, activeSkillRuntime, passiveRuntime, enemySpawner, gameplayUiRoot };
                _previousEnabled = new bool[_waitingComponents.Length];
                for (var i = 0; i < _waitingComponents.Length; i++)
                {
                    _previousEnabled[i] = _waitingComponents[i].enabled;
                    _waitingComponents[i].enabled = false;
                }
            }
            var roster = access == null ? Catalog.Characters : new CharacterRoster(Catalog.Characters.AllCharacters, access);
            Selection = new CharacterSelectionSession(roster, Catalog.RunSetup.StartingCharacterId, this);
            _selectionScreen = new CharacterSelectScreen(transform, Selection, Catalog.Registry);
        }

        public bool TryStartCharacter(ContentId id)
        {
            if (IsInitialized || _selectionScreen == null || Selection == null || !Selection.Roster.TrySelect(id, out _)) return false;
            _pendingCharacterId = id;
            var previousField = FieldSelection?.SelectedId ?? Catalog.Fields.DefaultFieldId;
            FieldSelection = new FieldSelectionSession(_fieldRoster, previousField, this);
            _fieldScreen = new FieldSelectScreen(transform, FieldSelection);
            _selectionScreen?.Dispose();
            _selectionScreen = null;
            return true;
        }

        public void BackToCharacters()
        {
            if (IsInitialized || _fieldScreen == null) return;
            _fieldScreen.Dispose();
            _fieldScreen = null;
            Selection = new CharacterSelectionSession(Selection.Roster, _pendingCharacterId, this);
            _selectionScreen = new CharacterSelectScreen(transform, Selection, Catalog.Registry);
        }

        public bool TryStartField(ContentId id)
        {
            if (IsInitialized || _fieldScreen == null || FieldSelection == null ||
                !FieldSelection.Roster.TrySelect(id, out _) || !Selection.Roster.TrySelect(_pendingCharacterId, out _)) return false;
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
        public void Initialize() => Initialize(FixtureRuntimeContentCatalog.Create().RunSetup.StartingCharacterId);

        public void Initialize(ContentId characterId, CharacterRoster roster = null, ContentId? fieldId = null, FieldRoster fields = null)
        {
            if (IsInitialized)
                throw new InvalidOperationException("Gameplay composition root is already initialized.");
            ValidateSceneReferences();

            // Run parameters (starting character, draft settings, XP curve) are content,
            // validated by their domain types when the catalog loads.
            Catalog = FixtureRuntimeContentCatalog.Create();
            var setup = Catalog.RunSetup;
            if (!(roster ?? Catalog.Characters).TrySelect(characterId, out var selectedCharacter))
                throw new InvalidOperationException($"Character '{characterId}' is locked or missing.");
            if (!(fields ?? Catalog.Fields.Roster).TrySelect(fieldId ?? Catalog.Fields.DefaultFieldId, out var selectedField))
                throw new InvalidOperationException("Field is locked or missing.");
            var configuration = selectedField.Resolve(Catalog.Registry);
            if (configuration.Travelers != null)
                throw new InvalidOperationException("Traveler schedule requires an IP-29 runtime consumer.");
            var spawn = FieldEnvironmentBinding.Validate(configuration.Environment, gameObject.scene);

            // If a subsystem's Initialize() throws partway through, every subsystem
            // that already succeeded gets rolled back (in reverse order) via its
            // Shutdown() before the exception propagates, so a failed composition
            // never leaves some subsystems live-subscribed and others untouched.
            if (!runController.IsInitialized) runController.Initialize();
            var initializedSubsystems = new List<Action>();
            try
            {
                runController.Model.ConfigureSelection(new RunSelectionSnapshot(characterId, selectedField.Id,
                    configuration.Environment.Id, configuration.Timeline.Id));
                initializedSubsystems.Add(runController.Shutdown);
                var previousPosition = player.transform.position;
                var body = player.GetComponent<Rigidbody2D>();
                player.transform.position = spawn.position;
                if (body != null) { body.position = spawn.position; body.linearVelocity = Vector2.zero; body.angularVelocity = 0; }
                initializedSubsystems.Add(() => { player.transform.position = previousPosition; if (body != null) body.position = previousPosition; });
                player.Initialize(selectedCharacter.BaseStats, runController, selectedCharacter.Id);
                initializedSubsystems.Add(player.Shutdown);

                if (!selectedCharacter.Visual.Id.IsValid || !selectedCharacter.MotionProfile.Id.IsValid)
                    throw new InvalidOperationException(
                        $"Character '{selectedCharacter.Id}' requires visual and motion profile references.");
                var playerVisual = selectedCharacter.Visual.Resolve(Catalog.Registry);
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

                experienceRuntime.Initialize(player, runController, setup.Experience);
                initializedSubsystems.Add(experienceRuntime.Shutdown);

                _setEffects = new SetEffectHost(player, runController, activeSkillRuntime, experienceRuntime.Progression, Catalog.ActiveSkills);
                initializedSubsystems.Add(_setEffects.Dispose);
                draftRuntime.Initialize(
                    experienceRuntime,
                    runController,
                    Catalog.BuildEntries,
                    selectedCharacter,
                    Catalog.Registry,
                    setup.Draft.OfferCount,
                    new SeededDraftRandom(setup.Draft.Seed),
                    setup.Draft.InitialRerolls,
                    setup.Draft.InitialBanishes,
                    Catalog.Sets,
                    new SetEffectAbilityFactory(_setEffects),
                    setup.Draft.EmptyBookCurrency,
                    new FixtureSetDraftOfferProvider(setup.Draft.SetDraftChance));
                initializedSubsystems.Add(draftRuntime.Shutdown);

                // The executor owns a scene GameObject (mine pool root); it is registered for
                // rollback before Initialize so a failed Initialize cannot leak it (Dispose is
                // idempotent, and Shutdown disposes it again on the success path).
                var effectExecutor = new SceneActiveSkillEffectExecutor(runController);
                initializedSubsystems.Add(effectExecutor.Dispose);
                activeSkillRuntime.Initialize(
                    player,
                    runController,
                    draftRuntime,
                    Catalog.ActiveSkills,
                    new SceneEnemyTargetProvider(),
                    effectExecutor);
                initializedSubsystems.Add(activeSkillRuntime.Shutdown);

                passiveRuntime.Initialize(player, draftRuntime, Catalog.Passives);
                initializedSubsystems.Add(passiveRuntime.Shutdown);

                if (Pickups == null) Pickups = gameObject.AddComponent<WorldPickupRuntime>();
                var placement = FixturePickupPlacement.Create(configuration.Environment, gameObject.scene,
                    player.GetComponent<Collider2D>(), Catalog.Pickups.PlacementSkin);
                Pickups.Initialize(Catalog.Pickups, runController.Model, player,
                    new PlayerPickupRewardTarget(player, runController.Model, draftRuntime, _setEffects.PublishReward), placement, selectedField.Id);
                initializedSubsystems.Add(Pickups.Shutdown);

                var enemiesById = new Dictionary<ContentId, EnemyDefinition>(configuration.Enemies.Count);
                var enemyVisuals = new Dictionary<ContentId, Sprite>(configuration.Enemies.Count);
                for (var i = 0; i < configuration.Enemies.Count; i++)
                {
                    var enemy = configuration.Enemies[i];
                    enemiesById.Add(enemy.Id, enemy);
                    if (enemy.Visual.TryResolve(Catalog.Registry, out var enemySprite))
                        enemyVisuals.Add(enemy.Id, enemySprite.Sprite);
                }
                var waveDirector = new WaveDirector(
                    configuration.Timeline,
                    enemiesById,
                    runController.Model.Duration);
                enemySpawner.Initialize(waveDirector, enemyVisuals,
                    new EnemyRewardSink(new EnemyExperienceDropSink(experienceRuntime, runController), Pickups));
                initializedSubsystems.Add(enemySpawner.Shutdown);

                if (BossEncounters == null) BossEncounters = gameObject.AddComponent<BossEncounterRuntime>();
                BossEncounters.Initialize(waveDirector, runController, player.transform, configuration.Bosses,
                    new EnemyExperienceDropSink(experienceRuntime, runController));
                initializedSubsystems.Add(BossEncounters.Shutdown);

                Playtest = PlaytestComposition.Create(Catalog, runController.Model, player, experienceRuntime,
                    draftRuntime, enemySpawner, activeSkillRuntime, Pickups);
                if (Playtest is PlaytestSession session) initializedSubsystems.Add(session.Dispose);

                gameplayUiRoot.Initialize(
                    player,
                    experienceRuntime,
                    draftRuntime,
                    runController,
                    playerPresentation,
                    (roster ?? Catalog.Characters).UnlockedCharacters,
                    enemySpawner,
                    Playtest,
                    BossEncounters, Pickups);
                initializedSubsystems.Add(gameplayUiRoot.Shutdown);
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

        private void LateUpdate() { if (Playtest is PlaytestSession session) session.Tick(); }

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
            // Capture required Results while every contributor is still alive, then diagnostics.
            runController.Shutdown();
            gameplayUiRoot.Shutdown();
            if (Playtest is PlaytestSession session) session.Dispose();
            BossEncounters?.Shutdown();
            enemySpawner.Shutdown();
            Pickups?.Shutdown();
            passiveRuntime.Shutdown();
            activeSkillRuntime.Shutdown();
            draftRuntime.Shutdown();
            _setEffects?.Dispose();
            _setEffects = null;
            experienceRuntime.Shutdown();
            playerPresentation.Shutdown();
            player.Shutdown();
            FieldConfiguration = null;
        }

        private void OnDisable() => Shutdown();
        private void OnDestroy() => Shutdown();
    }
}
