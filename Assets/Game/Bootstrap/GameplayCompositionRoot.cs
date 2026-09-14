using System;
using Game.ActiveSkill;
using Game.Character;
using Game.Content;
using Game.Enemy;
using Game.Progression;
using Game.Run;
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
        private string startingActiveId = "FIXTURE-SKILL-BOLT";

        [SerializeField, Min(1)]
        private int draftOfferCount = 3;

        [SerializeField]
        private int draftSeed = 12345;

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

            Catalog = FixtureRuntimeContentCatalog.Create();
            var startingActive = Catalog.Registry.Get<ActiveSkillProgressionDefinition>(new ContentId(startingActiveId));

            draftRuntime.Initialize(
                experienceRuntime,
                runController,
                Catalog.BuildEntries,
                startingActive,
                draftOfferCount,
                new SeededDraftRandom(draftSeed));
            activeSkillRuntime.Initialize(
                player,
                runController,
                draftRuntime,
                Catalog.ActiveSkills,
                new SceneEnemyTargetProvider(),
                new SceneActiveSkillEffectExecutor(runController));
            passiveRuntime.Initialize(player, draftRuntime, Catalog.Passives);
            enemySpawner.Initialize(Catalog.Enemies[0]);
            IsInitialized = true;
        }

        private void ValidateSceneReferences()
        {
            if (runController == null || player == null || experienceRuntime == null || draftRuntime == null ||
                activeSkillRuntime == null || passiveRuntime == null || enemySpawner == null)
            {
                throw new InvalidOperationException("Gameplay composition root has missing scene references.");
            }
        }
    }
}
