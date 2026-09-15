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
        private string startingActiveId = "FIXTURE-SKILL-BOLT";

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
            var startingActive = Catalog.Registry.Get<ActiveSkillProgressionDefinition>(new ContentId(startingActiveId));

            // If a subsystem's Initialize() throws partway through, every subsystem
            // that already succeeded gets rolled back (in reverse order) via its
            // Shutdown() before the exception propagates, so a failed composition
            // never leaves some subsystems live-subscribed and others untouched.
            var initializedSubsystems = new List<Action>();
            try
            {
                player.Initialize(Catalog.DefaultCharacterBaseStats, runController);
                initializedSubsystems.Add(player.Shutdown);

                experienceRuntime.Initialize(player, runController);
                initializedSubsystems.Add(experienceRuntime.Shutdown);

                draftRuntime.Initialize(
                    experienceRuntime,
                    runController,
                    Catalog.BuildEntries,
                    startingActive,
                    draftOfferCount,
                    new SeededDraftRandom(draftSeed),
                    fixtureInitialRerolls,
                    fixtureInitialBanishes);
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

                var fixtureEnemy = Catalog.Enemies[0];
                var enemyVisual = fixtureEnemy.Visual.TryResolve(Catalog.Registry, out var enemySprite)
                    ? enemySprite.Sprite
                    : null;
                enemySpawner.Initialize(fixtureEnemy, enemyVisual);
                initializedSubsystems.Add(enemySpawner.Shutdown);

                gameplayUiRoot.Initialize(player, experienceRuntime, draftRuntime, runController);
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
            if (runController == null || player == null || experienceRuntime == null || draftRuntime == null ||
                activeSkillRuntime == null || passiveRuntime == null || enemySpawner == null || gameplayUiRoot == null)
            {
                throw new InvalidOperationException("Gameplay composition root has missing scene references.");
            }
        }
    }
}
