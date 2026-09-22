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
    }
}
