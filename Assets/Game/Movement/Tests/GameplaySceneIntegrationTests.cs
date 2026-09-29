using System.Reflection;
using Game.Character;
using Game.Field;
using Game.Run;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Movement.Tests
{
    public class GameplaySceneIntegrationTests
    {
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";
        private const string PlayerLayerName = "Player";

        [OneTimeSetUp]
        public void OpenGameplayScene()
        {
            EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
        }

        [Test]
        public void Player_HasConfiguredMovementDependencies()
        {
            var player = RequireObject("Player");
            var mover = player.GetComponent<PlayerMover>();
            var speedSource = player.GetComponent<IMovementSpeedSource>();
            var body = player.GetComponent<Rigidbody2D>();
            var collider = player.GetComponent<CircleCollider2D>();

            Assert.IsNotNull(mover);
            Assert.IsNotNull(speedSource);

            // MovementSpeed only resolves from Stats once Initialize() has loaded
            // base config; the scene doesn't run this outside the composition root.
            // The scene's RunController hasn't run Awake() outside Play Mode, so
            // its Model needs invoking by hand before Initialize can bind to it.
            var sceneRunController = RequireObject("RunController").GetComponent<RunController>();
            InvokeAwake(sceneRunController);
            ((PlayerCharacterRuntime)speedSource).Initialize(
                FixtureCharacterCatalog.CreateDefault(),
                sceneRunController);
            Assert.Greater(speedSource.MovementSpeed, 0f);
            Assert.IsNotNull(body);
            Assert.AreEqual(0f, body.gravityScale);
            Assert.IsTrue((body.constraints & RigidbodyConstraints2D.FreezeRotation) != 0);
            Assert.IsNotNull(collider);
            Assert.IsFalse(collider.isTrigger);
            Assert.AreEqual(LayerMask.NameToLayer(PlayerLayerName), player.layer);

            var serializedMover = new SerializedObject(mover);
            var moveAction = serializedMover.FindProperty("moveAction").objectReferenceValue;
            var runController = serializedMover.FindProperty("runController").objectReferenceValue;
            var spawnPoint = serializedMover.FindProperty("spawnPoint").objectReferenceValue;

            Assert.IsNotNull(moveAction);
            Assert.AreSame(RequireObject("RunController").GetComponent<RunController>(), runController);
            Assert.AreSame(RequireObject("SpawnPoint").transform, spawnPoint);
        }

        [Test]
        public void Field_HasClosedBoundsAndFixtureObstacle()
        {
            var geometry = FixtureArenaGeometryCatalog.Create();
            var center = (geometry.SideLength + geometry.WallThickness) * .5f;
            var span = geometry.SideLength + geometry.WallThickness;
            Assert.AreEqual(20, geometry.SideScreenHeights);
            Assert.AreEqual(RequireObject("Main Camera").GetComponent<Camera>().orthographicSize * 2, geometry.ReferenceScreenHeight);
            AssertWall("Wall_Top", new Vector2(0, center), new Vector2(span, geometry.WallThickness));
            AssertWall("Wall_Bottom", new Vector2(0, -center), new Vector2(span, geometry.WallThickness));
            AssertWall("Wall_Left", new Vector2(-center, 0), new Vector2(geometry.WallThickness, span));
            AssertWall("Wall_Right", new Vector2(center, 0), new Vector2(geometry.WallThickness, span));

            var obstacle = RequireObject("Obstacle_Fixture");
            var obstacleCollider = obstacle.GetComponent<BoxCollider2D>();

            Assert.IsNotNull(obstacleCollider);
            Assert.IsFalse(obstacleCollider.isTrigger);
            AssertPlayerOnlyCollision(obstacleCollider);
            Assert.AreEqual(new Vector3(3f, 0f, 0f), obstacle.transform.position);
        }

        [Test]
        public void RunController_UsesCanonicalDefaultDuration()
        {
            var controller = RequireObject("RunController").GetComponent<RunController>();
            var serializedController = new SerializedObject(controller);
            var duration = serializedController.FindProperty("_durationSeconds").floatValue;

            Assert.AreEqual(RunModel.DefaultDurationSeconds, duration);
        }

        [Test]
        public void Camera_FollowsPlayerAtViewportCenterAndPreservesDepth()
        {
            var player = RequireObject("Player");
            var cameraObject = RequireObject("Main Camera");
            var camera = cameraObject.GetComponent<Camera>();
            var follower = cameraObject.GetComponent<CameraFollowTarget>();
            var originalPlayerPosition = player.transform.position;
            var originalCameraPosition = cameraObject.transform.position;

            Assert.IsNotNull(camera);
            Assert.IsNotNull(follower);
            Assert.AreSame(player.transform, follower.Target);

            try
            {
                player.transform.position = new Vector3(4.25f, -3.5f, originalPlayerPosition.z);
                follower.CenterOnTarget();

                var viewportPosition = camera.WorldToViewportPoint(player.transform.position);
                Assert.AreEqual(0.5f, viewportPosition.x, 0.0001f);
                Assert.AreEqual(0.5f, viewportPosition.y, 0.0001f);
                Assert.AreEqual(originalCameraPosition.z, cameraObject.transform.position.z, 0.0001f);
            }
            finally
            {
                player.transform.position = originalPlayerPosition;
                cameraObject.transform.position = originalCameraPosition;
            }
        }

        [Test]
        public void Camera_StopsAtEveryFieldEdge_ThenFollowsPlayerAgain()
        {
            var player = RequireObject("Player");
            var cameraObject = RequireObject("Main Camera");
            var camera = cameraObject.GetComponent<Camera>();
            var follower = cameraObject.GetComponent<CameraFollowTarget>();
            var playerStart = player.transform.position;
            var cameraStart = cameraObject.transform.position;
            var halfField = FixtureArenaGeometryCatalog.Create().SideLength * 0.5f;
            var halfWidth = camera.orthographicSize * camera.aspect;
            var halfHeight = camera.orthographicSize;

            try
            {
                follower.ConfigureBounds(halfField * 2f);
                player.transform.position = new Vector3(halfField, halfField, playerStart.z);
                follower.CenterOnTarget();
                Assert.AreEqual(halfField, cameraObject.transform.position.x + halfWidth, 0.0001f);
                Assert.AreEqual(halfField, cameraObject.transform.position.y + halfHeight, 0.0001f);

                var atCorner = cameraObject.transform.position;
                player.transform.position = new Vector3(halfField - halfWidth * 0.5f,
                    halfField - halfHeight * 0.5f, playerStart.z);
                follower.CenterOnTarget();
                Assert.AreEqual(atCorner.x, cameraObject.transform.position.x, 0.0001f);
                Assert.AreEqual(atCorner.y, cameraObject.transform.position.y, 0.0001f);

                player.transform.position = new Vector3(halfField - halfWidth - 2f,
                    halfField - halfHeight - 2f, playerStart.z);
                follower.CenterOnTarget();
                Assert.AreEqual(player.transform.position.x, cameraObject.transform.position.x, 0.0001f);
                Assert.AreEqual(player.transform.position.y, cameraObject.transform.position.y, 0.0001f);

                player.transform.position = new Vector3(-halfField, -halfField, playerStart.z);
                follower.CenterOnTarget();
                Assert.AreEqual(-halfField, cameraObject.transform.position.x - halfWidth, 0.0001f);
                Assert.AreEqual(-halfField, cameraObject.transform.position.y - halfHeight, 0.0001f);
                var shaken = follower.ClampPosition(cameraObject.transform.position + new Vector3(-1f, -1f, 0f));
                Assert.AreEqual(cameraObject.transform.position.x, shaken.x, 0.0001f);
                Assert.AreEqual(cameraObject.transform.position.y, shaken.y, 0.0001f);
            }
            finally
            {
                follower.ClearBounds();
                player.transform.position = playerStart;
                cameraObject.transform.position = cameraStart;
            }
        }

        private static void InvokeAwake(MonoBehaviour behaviour)
        {
            behaviour.GetType()
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(behaviour, null);
        }

        private static GameObject RequireObject(string name)
        {
            var gameObject = GameObject.Find(name);
            Assert.IsNotNull(gameObject, $"Gameplay scene must contain an active '{name}' object.");
            return gameObject;
        }

        private static void AssertWall(string name, Vector2 expectedPosition, Vector2 expectedSize)
        {
            var wall = RequireObject(name);
            var collider = wall.GetComponent<BoxCollider2D>();

            Assert.IsNotNull(collider);
            Assert.IsFalse(collider.isTrigger);
            AssertPlayerOnlyCollision(collider);
            Assert.AreEqual(expectedPosition.x, wall.transform.position.x, 0.0001f);
            Assert.AreEqual(expectedPosition.y, wall.transform.position.y, 0.0001f);
            Assert.AreEqual(expectedSize.x, collider.size.x, 0.0001f);
            Assert.AreEqual(expectedSize.y, collider.size.y, 0.0001f);
        }

        private static void AssertPlayerOnlyCollision(Collider2D collider)
        {
            var playerLayer = LayerMask.NameToLayer(PlayerLayerName);
            Assert.GreaterOrEqual(playerLayer, 0);
            Assert.AreEqual(~(1 << playerLayer), collider.excludeLayers.value);
        }
    }
}
