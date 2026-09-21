using System;
using System.Collections.Generic;
using Game.ActiveSkill;
using Game.Character;
using Game.Content;
using Game.Enemy;
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
    public sealed class GameplayCompositionRoot : MonoBehaviour, ICharacterRunLauncher
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

        public void OpenCharacterSelection(ICharacterAccessProvider access = null)
        {
            if (IsInitialized) throw new InvalidOperationException("Shut down the current run before selecting another character.");
            ValidateSceneReferences();
            Catalog = FixtureRuntimeContentCatalog.Create();
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
            if (IsInitialized || Selection == null || !Selection.Roster.TrySelect(id, out _)) return false;
            Initialize(id, Selection.Roster);
            _selectionScreen?.Dispose();
            _selectionScreen = null;
            for (var i = 0; i < _waitingComponents.Length; i++)
                _waitingComponents[i].enabled = _previousEnabled[i];
            _waitingComponents = null;
            _previousEnabled = null;
            runController.Model.Start();
            return true;
        }

        // Explicit composition entry point retained for scene integration tests and future navigation.
        public void Initialize() => Initialize(FixtureRuntimeContentCatalog.Create().RunSetup.StartingCharacterId);

        public void Initialize(ContentId characterId, CharacterRoster roster = null)
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

            // If a subsystem's Initialize() throws partway through, every subsystem
            // that already succeeded gets rolled back (in reverse order) via its
            // Shutdown() before the exception propagates, so a failed composition
            // never leaves some subsystems live-subscribed and others untouched.
            if (!runController.IsInitialized) runController.Initialize();
            var initializedSubsystems = new List<Action>();
            try
            {
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

                var enemiesById = new Dictionary<ContentId, EnemyDefinition>(Catalog.Enemies.Count);
                var enemyVisuals = new Dictionary<ContentId, Sprite>(Catalog.Enemies.Count);
                for (var i = 0; i < Catalog.Enemies.Count; i++)
                {
                    var enemy = Catalog.Enemies[i];
                    enemiesById.Add(enemy.Id, enemy);
                    if (enemy.Visual.TryResolve(Catalog.Registry, out var enemySprite))
                        enemyVisuals.Add(enemy.Id, enemySprite.Sprite);
                }
                var waveDirector = new WaveDirector(
                    Catalog.WaveTimeline,
                    enemiesById,
                    runController.Model.Duration);
                enemySpawner.Initialize(waveDirector, enemyVisuals,
                    new EnemyExperienceDropSink(experienceRuntime, runController));
                initializedSubsystems.Add(enemySpawner.Shutdown);

                if (BossEncounters == null) BossEncounters = gameObject.AddComponent<BossEncounterRuntime>();
                BossEncounters.Initialize(waveDirector, runController, player.transform, Catalog.Bosses,
                    new EnemyExperienceDropSink(experienceRuntime, runController));
                initializedSubsystems.Add(BossEncounters.Shutdown);

                Playtest = PlaytestComposition.Create(Catalog, runController.Model, player, experienceRuntime,
                    draftRuntime, enemySpawner, activeSkillRuntime);
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
                    BossEncounters);
                initializedSubsystems.Add(gameplayUiRoot.Shutdown);
            }
            catch
            {
                for (var i = initializedSubsystems.Count - 1; i >= 0; i--)
                    initializedSubsystems[i]();
                throw;
            }

            IsInitialized = true;
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
            Selection = null;
            if (!IsInitialized) return;
            IsInitialized = false;
            player.ShuttingDown -= Shutdown;
            // Capture required Results while every contributor is still alive, then diagnostics.
            runController.Shutdown();
            gameplayUiRoot.Shutdown();
            if (Playtest is PlaytestSession session) session.Dispose();
            BossEncounters?.Shutdown();
            enemySpawner.Shutdown();
            passiveRuntime.Shutdown();
            activeSkillRuntime.Shutdown();
            draftRuntime.Shutdown();
            _setEffects?.Dispose();
            _setEffects = null;
            experienceRuntime.Shutdown();
            playerPresentation.Shutdown();
            player.Shutdown();
        }

        private void OnDisable() => Shutdown();
        private void OnDestroy() => Shutdown();
    }
}
