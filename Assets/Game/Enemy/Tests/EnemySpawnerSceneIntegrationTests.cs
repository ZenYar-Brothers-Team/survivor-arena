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
            Assert.IsTrue(serializedSpawner.FindProperty("fixtureContentId").stringValue.StartsWith("FIXTURE-"));
            Assert.Greater(serializedSpawner.FindProperty("fixtureContactDamageInterval").floatValue, 0f);
            Assert.Greater(serializedSpawner.FindProperty("fixtureExperienceReward").floatValue, 0f);
            Assert.Greater(serializedSpawner.FindProperty("spawnIntervalSeconds").floatValue, 0f);
            Assert.Greater(serializedSpawner.FindProperty("maxAliveEnemies").intValue, 0);
        }
    }
}
