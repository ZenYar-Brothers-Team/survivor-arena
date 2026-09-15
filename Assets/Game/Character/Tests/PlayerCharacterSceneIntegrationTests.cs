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

            // Base stats are no longer scene-configured fields — the composition
            // root supplies them via Initialize() from FixtureCharacterCatalog
            // (see FixtureCharacterCatalogTests for direct coverage of that catalog).
            runtime.Initialize(FixtureCharacterCatalog.CreateDefault());
            Assert.Greater(runtime.Stats.MaxHealth, 0f);
            Assert.Greater(runtime.Stats.MovementSpeed, 0f);
        }
    }
}
