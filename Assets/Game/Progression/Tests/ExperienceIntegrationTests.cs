using Game.Character;
using Game.Enemy;
using Game.Run;
using NUnit.Framework;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Progression.Tests
{
    public class ExperienceIntegrationTests
    {
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [Test]
        public void EnemyDeath_SpawnsPhysicalExperienceAtDeathPosition()
        {
            var runObject = new GameObject("RunController");
            var runController = runObject.AddComponent<RunController>();
            InvokeAwake(runController);
            runController.Model.Start();
            var player = new GameObject("Player");
            player.AddComponent<CircleCollider2D>();
            var character = player.AddComponent<PlayerCharacterRuntime>();
            character.Initialize(new CharacterBaseStats(100f, 3f), runController);
            var experience = player.AddComponent<PlayerExperienceRuntime>();
            experience.Initialize(character, runController, 100f);
            var definition = new EnemyDefinition("FIXTURE-ENEMY", 1f, 1f, 0f, 0f, 1f, 3f);
            var deathPosition = new Vector2(2f, 4f);
            ExperienceDropRuntime drop = null;

            try
            {
                var enemy = EnemyFactory.Spawn(definition, deathPosition, player.transform, runController);
                enemy.TakeDamage(1f);
                drop = Object.FindAnyObjectByType<ExperienceDropRuntime>();

                Assert.IsNotNull(drop);
                Assert.AreEqual(deathPosition, (Vector2)drop.transform.position);
                Assert.AreEqual(3f, drop.Amount);
                Assert.AreEqual(60f, drop.Lifetime);
            }
            finally
            {
                if (drop != null)
                    Object.DestroyImmediate(drop.gameObject);
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(runObject);
            }
        }

        [Test]
        public void LevelUp_EmitsEventAndPausesRun()
        {
            var runObject = new GameObject("RunController");
            var runController = runObject.AddComponent<RunController>();
            InvokeAwake(runController);
            runController.Model.Start();
            var player = new GameObject("Player");
            var character = player.AddComponent<PlayerCharacterRuntime>();
            character.Initialize(new CharacterBaseStats(100f, 3f), runController);
            var experience = player.AddComponent<PlayerExperienceRuntime>();
            experience.Initialize(character, runController, 5f);
            var emittedLevel = 0;
            experience.LevelUp += level => emittedLevel = level;

            try
            {
                experience.AddPickedUpExperience(5f);

                Assert.AreEqual(2, emittedLevel);
                Assert.AreEqual(RunState.Paused, runController.Model.State);
            }
            finally
            {
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(runObject);
            }
        }

        [Test]
        public void GameplayScene_HasConfiguredExperienceRuntimeAndXpReward()
        {
            EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            var player = GameObject.Find("Player");
            var runtime = player.GetComponent<PlayerExperienceRuntime>();
            Assert.IsNotNull(runtime);

            var serializedRuntime = new SerializedObject(runtime);
            Assert.AreSame(player.GetComponent<PlayerCharacterRuntime>(), serializedRuntime.FindProperty("owner").objectReferenceValue);
            Assert.AreSame(GameObject.Find("RunController").GetComponent<RunController>(), serializedRuntime.FindProperty("runController").objectReferenceValue);
            Assert.Greater(serializedRuntime.FindProperty("baseDropLifetimeSeconds").floatValue, 0f);
            Assert.Greater(serializedRuntime.FindProperty("fixtureLevelThresholds").arraySize, 0);

            var draftRuntime = player.GetComponent<LevelUpDraftRuntime>();
            Assert.IsNotNull(draftRuntime);
            var serializedDraft = new SerializedObject(draftRuntime);
            Assert.AreSame(runtime, serializedDraft.FindProperty("experienceRuntime").objectReferenceValue);
            Assert.AreSame(GameObject.Find("RunController").GetComponent<RunController>(), serializedDraft.FindProperty("runController").objectReferenceValue);
            Assert.IsNotNull(player.GetComponent<PlayerPassiveSetRuntime>());
        }

        private static void InvokeAwake(MonoBehaviour behaviour)
        {
            behaviour.GetType()
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(behaviour, null);
        }
    }
}
