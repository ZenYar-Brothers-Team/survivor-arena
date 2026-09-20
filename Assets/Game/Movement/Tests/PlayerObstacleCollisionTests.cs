using NUnit.Framework;
using UnityEngine;

namespace Game.Movement.Tests
{
    public class PlayerObstacleCollisionTests
    {
        private SimulationMode2D _previousSimulationMode;
        private GameObject _mover;
        private GameObject _obstacle;

        [SetUp]
        public void SetUp()
        {
            _previousSimulationMode = Physics2D.simulationMode;
            Physics2D.simulationMode = SimulationMode2D.Script;
        }

        [TearDown]
        public void TearDown()
        {
            if (_mover != null)
                Object.DestroyImmediate(_mover);

            if (_obstacle != null)
                Object.DestroyImmediate(_obstacle);

            Physics2D.simulationMode = _previousSimulationMode;
        }

        [Test]
        public void DynamicBody_IsBlockedByStaticObstacle()
        {
            _obstacle = new GameObject("Obstacle");
            _obstacle.transform.position = new Vector3(2f, 0f, 0f);
            var obstacleCollider = _obstacle.AddComponent<BoxCollider2D>();
            obstacleCollider.size = new Vector2(1f, 1f);

            _mover = new GameObject("Mover");
            _mover.transform.position = new Vector3(0f, 0f, 0f);
            var moverCollider = _mover.AddComponent<BoxCollider2D>();
            moverCollider.size = new Vector2(0.5f, 0.5f);
            var rigidbody = _mover.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f;
            rigidbody.linearVelocity = new Vector2(5f, 0f);

            for (var i = 0; i < 120; i++)
                Physics2D.Simulate(0.02f);

            // Obstacle's left edge is at x=1.5; mover's half-width is 0.25, so it should
            // come to rest with its center no further right than ~1.25.
            Assert.Less(_mover.transform.position.x, 1.3f);
        }

        [TestCase(true, 1.3f, 0f)]
        [TestCase(false, 3f, 0.1f)]
        public void PlayerBoundary_BlocksOnlyPlayer(bool isPlayer, float expectedX, float tolerance)
        {
            var playerLayer = LayerMask.NameToLayer("Player");
            Assert.GreaterOrEqual(playerLayer, 0);

            _obstacle = new GameObject("Player Boundary");
            _obstacle.transform.position = new Vector3(2f, 5f, 0f);
            var boundaryCollider = _obstacle.AddComponent<BoxCollider2D>();
            boundaryCollider.size = new Vector2(1f, 4f);
            boundaryCollider.excludeLayers = ~(1 << playerLayer);

            _mover = new GameObject(isPlayer ? "Player" : "Monster");
            _mover.transform.position = new Vector3(0f, 5f, 0f);
            _mover.layer = isPlayer ? playerLayer : 0;
            _mover.AddComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.5f);
            var rigidbody = _mover.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f;
            rigidbody.linearVelocity = new Vector2(5f, 0f);

            for (var i = 0; i < 30; i++)
                Physics2D.Simulate(0.02f);

            if (isPlayer)
                Assert.Less(_mover.transform.position.x, expectedX);
            else
                Assert.AreEqual(expectedX, _mover.transform.position.x, tolerance);
        }
    }
}
