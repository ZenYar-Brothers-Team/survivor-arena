using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Game.Pickup.Tests
{
    public sealed class BoxPickupPlacementTests
    {
        [Test]
        public void Contains_AcceptsFreePoint_RejectsObstacleAndOutside()
        {
            var placement = Create();

            Assert.IsTrue(placement.Contains(new Vector2(-5, 0)));
            Assert.IsFalse(placement.Contains(Vector2.zero));
            Assert.IsFalse(placement.Contains(new Vector2(20, 0)));
        }

        [Test]
        public void ClampToBounds_IgnoresObstacle_AndConstrainsOuterBounds()
        {
            var placement = Create();

            Assert.AreEqual(Vector2.zero, placement.ClampToBounds(Vector2.zero));
            var bounded = placement.ClampToBounds(new Vector2(20, -20));
            Assert.That(bounded.x, Is.InRange(9f, 10f));
            Assert.That(bounded.y, Is.InRange(-10f, -9f));
        }

        [Test]
        public void Constructor_RejectsBlockedAnchor()
        {
            Assert.Throws<ArgumentException>(() => new BoxPickupPlacement(new Rect(-10, -10, 20, 20),
                new[] { new Rect(-2, -2, 4, 4) }, Vector2.one * .5f, Vector2.zero, .01f));
        }

        [Test]
        public void Queries_DoNotAllocate()
        {
            var placement = Create();
            placement.Contains(new Vector2(-5, 0));
            placement.ClampToBounds(Vector2.zero);

            Assert.That(() => { placement.Contains(new Vector2(-5, 0)); }, Is.Not.AllocatingGCMemory());
            Assert.That(() => { placement.ClampToBounds(Vector2.zero); }, Is.Not.AllocatingGCMemory());
        }

        private static BoxPickupPlacement Create() => new BoxPickupPlacement(new Rect(-10, -10, 20, 20),
            new[] { new Rect(-2, -2, 4, 4) }, Vector2.one * .5f, new Vector2(-5, 0), .01f);
    }
}
