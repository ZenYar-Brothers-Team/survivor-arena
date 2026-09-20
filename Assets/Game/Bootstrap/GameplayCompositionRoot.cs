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

namespace Game.Bootstrap
{
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class GameplayCompositionRoot : MonoBehaviour
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

        [SerializeField]
        private PlayerPassiveSetRuntime passiveRuntime;

        [SerializeField]
        private ContinuousFixtureEnemySpawner enemySpawner;

        [SerializeField]
        private GameplayUiRoot gameplayUiRoot;

        [SerializeField]
        private string startingCharacterId = "FIXTURE-CHARACTER-AGILE";

        [SerializeField, Min(1)]
        private int draftOfferCount = 3;

        [SerializeField]
        private int draftSeed = 12345;

        [SerializeField, Min(0)]
        private int fixtureInitialRerolls = 2;

        [SerializeField, Min(0)]
        private int fixtureInitialBanishes = 2;

        public FixtureRuntimeContentCatalog Catalog { get; private set; }
        public bool IsInitialized { get; private set; }

        private void Start()
        {
            try
            {
                Initialize();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Gameplay composition failed: {exception}", this);
                enabled = false;
            }
        }

        public void Initialize()
        {
            if (IsInitialized)
                throw new InvalidOperationException("Gameplay composition root is already initialized.");
            ValidateSceneReferences();
            if (draftOfferCount <= 0)
                throw new InvalidOperationException("Draft offer count must be greater than zero.");
            if (fixtureInitialRerolls < 0 || fixtureInitialBanishes < 0)
                throw new InvalidOperationException("Draft control counts cannot be negative.");

            Catalog = FixtureRuntimeContentCatalog.Create();
            if (!Catalog.Characters.TrySelect(new ContentId(startingCharacterId), out var selectedCharacter))
                throw new InvalidOperationException($"Character '{startingCharacterId}' is locked or missing.");

            // If a subsystem's Initialize() throws partway through, every subsystem
            // that already succeeded gets rolled back (in reverse order) via its
            // Shutdown() before the exception propagates, so a failed composition
            // never leaves some subsystems live-subscribed and others untouched.
            var initializedSubsystems = new List<Action>();
            try
            {
                player.Initialize(selectedCharacter.BaseStats, runController);
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

                experienceRuntime.Initialize(player, runController);
                initializedSubsystems.Add(experienceRuntime.Shutdown);

                draftRuntime.Initialize(
                    experienceRuntime,
                    runController,
                    Catalog.BuildEntries,
                    selectedCharacter,
                    Catalog.Registry,
                    draftOfferCount,
                    new SeededDraftRandom(draftSeed),
                    fixtureInitialRerolls,
                    fixtureInitialBanishes,
                    Catalog.Sets,
                    new FixtureSetExtraAbilityFactory());
                initializedSubsystems.Add(draftRuntime.Shutdown);

                activeSkillRuntime.Initialize(
                    player,
                    runController,
                    draftRuntime,
                    Catalog.ActiveSkills,
                    new SceneEnemyTargetProvider(),
                    new SceneActiveSkillEffectExecutor(runController));
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
                enemySpawner.Initialize(waveDirector, enemyVisuals);
                initializedSubsystems.Add(enemySpawner.Shutdown);

                gameplayUiRoot.Initialize(
                    player,
                    experienceRuntime,
                    draftRuntime,
                    runController,
                    playerPresentation,
                    Catalog.Characters.UnlockedCharacters,
                    enemySpawner);
                initializedSubsystems.Add(gameplayUiRoot.Shutdown);
            }
            catch
            {
                for (var i = initializedSubsystems.Count - 1; i >= 0; i--)
                    initializedSubsystems[i]();
                throw;
            }

            IsInitialized = true;
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
    }
}
