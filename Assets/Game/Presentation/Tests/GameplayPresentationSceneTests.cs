using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public class GameplayPresentationSceneTests
    {
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [Test]
        public void Player_UsesIsolatedPresentationHierarchy()
        {
            EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            var player = GameObject.Find("Player");
            Assert.IsNotNull(player);
            Assert.IsNull(player.GetComponent<SpriteRenderer>());

            var visualRoot = player.transform.Find("VisualRoot");
            Assert.IsNotNull(visualRoot);
            Assert.IsNotNull(visualRoot.GetComponent<SpritePresentationRig>());
            Assert.IsNotNull(visualRoot.GetComponent<SpritePresentationRuntime>());

            var bodyRoot = visualRoot.Find("BodyRoot");
            var shadow = visualRoot.Find("ShadowRenderer");
            Assert.IsNotNull(bodyRoot);
            Assert.IsNotNull(shadow);
            Assert.IsNotNull(bodyRoot.GetComponent<SpriteRenderer>());
            Assert.IsNotNull(shadow.GetComponent<SpriteRenderer>());
            Assert.AreSame(visualRoot, shadow.parent);
        }
    }
}
