using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
namespace Game.Pickup.Tests
{
    public sealed class BoxPickupPlacementTests
    {
        [TestCase(20, 0, 9.49f, 0)]
        [TestCase(2.7f, 0, 3.51f, 0)]
        [TestCase(-2, 2, -2, 2)]
        public void Placement_OutsideObstacleOrFree_UsesNearestReachablePoint(float x, float y, float expectedX, float expectedY)
        {
            var placement = new BoxPickupPlacement(new Rect(-10, -10, 20, 20),
                new[] { new Rect(1, -1, 2, 2) }, Vector2.one * .5f, Vector2.zero, .01f);
            Assert.IsTrue(placement.TryPlace(new Vector2(x, y), out var point));
            Assert.AreEqual(expectedX, point.x, .001f); Assert.AreEqual(expectedY, point.y, .001f);
        }
        [Test]
        public void Placement_DisconnectedRegion_StaysInSpawnComponent()
        {
            var placement = new BoxPickupPlacement(new Rect(-10, -10, 20, 20),
                new[] { new Rect(1, -10, 1, 20) }, Vector2.one * .5f, Vector2.zero, .01f);
            Assert.IsTrue(placement.TryPlace(new Vector2(8, 0), out var point));
            Assert.AreEqual(.49f, point.x, .001f); Assert.AreEqual(0, point.y, .001f);
        }
        [Test]
        public void PlaceFrom_ClearStep_ReturnsRequestedPoint()
        {
            var placement = new BoxPickupPlacement(new Rect(-10, -10, 20, 20),
                new[] { new Rect(1, -1, 2, 2) }, Vector2.one * .5f, Vector2.zero, .01f);
            var requested = new Vector2(-2, 2);
            Assert.IsTrue(placement.TryPlaceFrom(Vector2.zero, requested, out var point));
            Assert.AreEqual(requested, point);
        }
        [Test]
        public void PlaceFrom_BlockedOrDisconnectedStep_MatchesFullProjection()
        {
            var placement = new BoxPickupPlacement(new Rect(-10, -10, 20, 20),
                new[] { new Rect(1, -10, 1, 20) }, Vector2.one * .5f, Vector2.zero, .01f);
            var requested = new Vector2(8, 0);
            Assert.IsTrue(placement.TryPlace(requested, out var expected));
            Assert.IsTrue(placement.TryPlaceFrom(Vector2.zero, requested, out var actual));
            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void PlaceFrom_SixtyFourObstacles_StaysWithinMovementBudget()
        {
            var obstacles = new Rect[64];
            for (var i = 0; i < obstacles.Length; i++)
                obstacles[i] = new Rect(10 + i * 1.1f, 10 + i * 1.3f, .5f, .5f);
            var placement = new BoxPickupPlacement(new Rect(-100, -100, 200, 200),
                obstacles, Vector2.one * .5f, Vector2.zero, .01f);
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < 100; i++)
            {
                Assert.IsTrue(placement.TryPlaceFrom(Vector2.zero, new Vector2(-1, i * .001f), out var point));
                Assert.AreEqual(-1, point.x);
            }
            stopwatch.Stop();
            Assert.Less(stopwatch.ElapsedMilliseconds, 200, "Clear movement steps must not rebuild the arena grid.");
        }
    }
}
