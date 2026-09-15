using Game.Character;
using Game.Run;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    public class ActiveSkillSceneIntegrationTests
    {
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [OneTimeSetUp]
        public void OpenGameplayScene()
        {
            EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
        }

        [Test]
        public void Player_HasConfiguredAutomaticFixtureSkillWithoutAttackInput()
        {
            var player = GameObject.Find("Player");
            Assert.IsNotNull(player);
            var skillSet = player.GetComponent<PlayerActiveSkillSetRuntime>();
            Assert.IsNotNull(skillSet);
            Assert.IsTrue(skillSet.enabled);
            var serializedSet = new SerializedObject(skillSet);
            Assert.AreSame(player.GetComponent<PlayerCharacterRuntime>(), serializedSet.FindProperty("owner").objectReferenceValue);
            Assert.AreSame(GameObject.Find("RunController").GetComponent<RunController>(), serializedSet.FindProperty("runController").objectReferenceValue);
            Assert.AreSame(player.GetComponent<Game.Progression.LevelUpDraftRuntime>(), serializedSet.FindProperty("draftRuntime").objectReferenceValue);
        }
    }
}
