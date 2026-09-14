using Game.ActiveSkill;
using Game.Enemy;
using Game.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Bootstrap.Tests
{
    public class GameplayCompositionSceneTests
    {
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [Test]
        public void GameplayScene_HasSingleCompositionRootWithAllRuntimeReferences()
        {
            EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            var root = Object.FindFirstObjectByType<GameplayCompositionRoot>();
            Assert.IsNotNull(root);

            var serialized = new SerializedObject(root);
            Assert.IsNotNull(serialized.FindProperty("runController").objectReferenceValue);
            Assert.IsNotNull(serialized.FindProperty("player").objectReferenceValue);
            Assert.IsNotNull(serialized.FindProperty("experienceRuntime").objectReferenceValue);
            Assert.IsInstanceOf<LevelUpDraftRuntime>(serialized.FindProperty("draftRuntime").objectReferenceValue);
            Assert.IsInstanceOf<PlayerActiveSkillSetRuntime>(serialized.FindProperty("activeSkillRuntime").objectReferenceValue);
            Assert.IsInstanceOf<PlayerPassiveSetRuntime>(serialized.FindProperty("passiveRuntime").objectReferenceValue);
            Assert.IsInstanceOf<ContinuousFixtureEnemySpawner>(serialized.FindProperty("enemySpawner").objectReferenceValue);
            StringAssert.StartsWith("FIXTURE-", serialized.FindProperty("startingActiveId").stringValue);
            Assert.Greater(serialized.FindProperty("draftOfferCount").intValue, 0);
        }
    }
}
