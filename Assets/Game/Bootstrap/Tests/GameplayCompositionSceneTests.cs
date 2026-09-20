using Game.ActiveSkill;
using Game.Enemy;
using Game.Progression;
using Game.Presentation;
using Game.UI;
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
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            Assert.IsNotNull(root);

            var serialized = new SerializedObject(root);
            Assert.IsNotNull(serialized.FindProperty("runController").objectReferenceValue);
            Assert.IsNotNull(serialized.FindProperty("player").objectReferenceValue);
            Assert.IsInstanceOf<SpritePresentationRuntime>(serialized.FindProperty("playerPresentation").objectReferenceValue);
            Assert.IsNotNull(serialized.FindProperty("experienceRuntime").objectReferenceValue);
            Assert.IsInstanceOf<LevelUpDraftRuntime>(serialized.FindProperty("draftRuntime").objectReferenceValue);
            Assert.IsInstanceOf<PlayerActiveSkillSetRuntime>(serialized.FindProperty("activeSkillRuntime").objectReferenceValue);
            Assert.IsInstanceOf<PlayerPassiveSetRuntime>(serialized.FindProperty("passiveRuntime").objectReferenceValue);
            Assert.IsInstanceOf<ContinuousFixtureEnemySpawner>(serialized.FindProperty("enemySpawner").objectReferenceValue);
            Assert.IsInstanceOf<GameplayUiRoot>(serialized.FindProperty("gameplayUiRoot").objectReferenceValue);

            // Run parameters are content (Resources/Content/Run), not scene-serialized fields.
            Assert.IsNull(serialized.FindProperty("startingCharacterId"));
            Assert.IsNull(serialized.FindProperty("draftOfferCount"));
            Assert.IsNull(serialized.FindProperty("draftSeed"));
            Assert.IsNull(serialized.FindProperty("fixtureInitialRerolls"));
            Assert.IsNull(serialized.FindProperty("fixtureInitialBanishes"));
        }
    }
}
