using Game.Movement;
using Game.Run;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Character.Tests
{
    public class PlayerCharacterSceneIntegrationTests
    {
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [OneTimeSetUp]
        public void OpenGameplayScene()
        {
            EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
        }

        [Test]
        public void Player_UsesConfiguredCharacterRuntimeAsMovementSource()
        {
            var player = GameObject.Find("Player");
            Assert.IsNotNull(player);

            var runtime = player.GetComponent<PlayerCharacterRuntime>();
            var movementSource = player.GetComponent<IMovementSpeedSource>();
            Assert.IsNotNull(runtime);
            Assert.AreSame(runtime, movementSource);
            Assert.IsNull(player.GetComponent<FixedMovementSpeedSource>());

            var serializedRuntime = new SerializedObject(runtime);
            var configuredRunController = serializedRuntime.FindProperty("runController").objectReferenceValue;
            Assert.AreSame(GameObject.Find("RunController").GetComponent<RunController>(), configuredRunController);
            Assert.Greater(serializedRuntime.FindProperty("baseMaxHealth").floatValue, 0f);
            Assert.Greater(serializedRuntime.FindProperty("baseMovementSpeed").floatValue, 0f);
            Assert.AreEqual(1f, serializedRuntime.FindProperty("baseActiveSkillDamageMultiplier").floatValue);
            Assert.AreEqual(1f, serializedRuntime.FindProperty("baseActiveSkillCooldownMultiplier").floatValue);
            Assert.AreEqual(0f, serializedRuntime.FindProperty("baseDisappearingXpRecovery").floatValue);
        }
    }
}
