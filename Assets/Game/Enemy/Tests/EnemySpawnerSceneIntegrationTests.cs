using Game.Run;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Enemy.Tests
{
    public class EnemySpawnerSceneIntegrationTests
    {
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [OneTimeSetUp]
        public void OpenGameplayScene()
        {
            EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
        }

        [Test]
        public void Gameplay_HasContinuousSpawnerWithNonProductionFixture()
        {
            var spawnerObject = GameObject.Find("EnemySpawner");
            Assert.IsNotNull(spawnerObject);
            var spawner = spawnerObject.GetComponent<ContinuousFixtureEnemySpawner>();
            Assert.IsNotNull(spawner);

            var serializedSpawner = new SerializedObject(spawner);
            Assert.AreSame(
                GameObject.Find("RunController").GetComponent<RunController>(),
                serializedSpawner.FindProperty("runController").objectReferenceValue);
            Assert.AreSame(
                GameObject.Find("Player").transform,
                serializedSpawner.FindProperty("target").objectReferenceValue);
            // Spawn cadence, caps and radius live in the wave timeline config, not on the component.
            Assert.IsNull(serializedSpawner.FindProperty("spawnRadius"));
            Assert.IsNull(serializedSpawner.FindProperty("spawnIntervalSeconds"));
            Assert.IsNull(serializedSpawner.FindProperty("maxAliveEnemies"));
        }
    }
}
