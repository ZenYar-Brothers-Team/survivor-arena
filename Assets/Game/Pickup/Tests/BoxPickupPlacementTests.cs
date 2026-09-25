using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;
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

        // Production-like field: 64 authored-size obstacles in a 200×200 arena (FIELD-001 scale).
        private static Rect[] FieldObstacles(int seed)
        {
            var random = new System.Random(seed);
            var obstacles = new Rect[64];
            for (var i = 0; i < obstacles.Length; i++)
            {
                var fence = random.Next(2) == 0;
                var size = fence ? new Vector2(2.4f, .5f) : new Vector2(1.2f, 1f);
                var center = new Vector2((float)(random.NextDouble() * 180 - 90), (float)(random.NextDouble() * 180 - 90));
                if (center.magnitude < 4f) center += center.normalized * 4f + Vector2.right * .1f;
                obstacles[i] = new Rect(center - size / 2f, size);
            }
            return obstacles;
        }

        [Test]
        public void TryPlace_MatchesTheOriginalGridSearchExactly_OnRandomFields()
        {
            var bounds = new Rect(-100, -100, 200, 200);
            var half = Vector2.one * .45f;
            for (var seed = 1; seed <= 4; seed++)
            {
                var obstacles = FieldObstacles(seed);
                var placement = new BoxPickupPlacement(bounds, obstacles, half, Vector2.zero, .05f);
                var reference = new BoxPickupPlacementReference(bounds, obstacles, half, Vector2.zero, .05f);
                var random = new System.Random(seed * 17);
                var requests = new List<Vector2> { new Vector2(150, 0), new Vector2(-150, -150), Vector2.zero };
                foreach (var rect in obstacles) requests.Add(rect.center); // inside obstacles: nearest point is on an edge
                for (var i = 0; i < 40; i++)
                    requests.Add(new Vector2((float)(random.NextDouble() * 220 - 110), (float)(random.NextDouble() * 220 - 110)));
                foreach (var requested in requests)
                {
                    Assert.AreEqual(reference.TryPlace(requested, out var expected), placement.TryPlace(requested, out var actual));
                    Assert.AreEqual(expected, actual, $"seed {seed}, request ({requested.x}, {requested.y})");
                }
            }
        }

        [Test]
        public void TryPlace_ExactEqualDistanceTie_PicksTheSamePointAsTheOriginalSearch()
        {
            // Exactly representable geometry: the inflated obstacle is (-2,-3)-(2,3) and the spawn (0,-5) is below it,
            // so (-2,0) and (2,0) are both exactly 2 away from the request and reached at the same search depth from
            // the left and the right. Only the neighbour visit order decides the winner.
            var bounds = new Rect(-10, -10, 20, 20);
            var obstacles = new[] { new Rect(-1, -2, 2, 4) };
            var spawn = new Vector2(0, -5);
            var placement = new BoxPickupPlacement(bounds, obstacles, Vector2.one * .5f, spawn, .5f);
            var reference = new BoxPickupPlacementReference(bounds, obstacles, Vector2.one * .5f, spawn, .5f);
            Assert.IsTrue(reference.TryPlace(Vector2.zero, out var expected));
            Assert.IsTrue(placement.TryPlace(Vector2.zero, out var actual));
            Assert.AreEqual(new Vector2(-2, 0), expected, "Original search keeps the first of the tied points.");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TryPlace_ReusedBuffers_AcrossQueriesThatAddOrSkipExtraGridLines()
        {
            var obstacles = new[] { new Rect(1, -1, 2, 2), new Rect(-4, 2, 1, 5) };
            var bounds = new Rect(-10, -10, 20, 20);
            var placement = new BoxPickupPlacement(bounds, obstacles, Vector2.one * .5f, Vector2.zero, .01f);
            // Grid-line coordinates (no extra column/row), off-grid ones (both extra), mixed, and outside the bounds.
            var requests = new[] { new Vector2(2, 0), new Vector2(0.49f, 0.51f), new Vector2(7.3f, -2.2f), new Vector2(0, 5.5f),
                new Vector2(-3.5f, 4), new Vector2(40, -40), new Vector2(2, 0) };
            foreach (var requested in requests)
            {
                var fresh = new BoxPickupPlacement(bounds, obstacles, Vector2.one * .5f, Vector2.zero, .01f);
                Assert.IsTrue(fresh.TryPlace(requested, out var expected));
                Assert.IsTrue(placement.TryPlace(requested, out var actual));
                Assert.AreEqual(expected, actual, $"State from a previous query leaked into ({requested.x}, {requested.y}).");
            }
        }

        [Test]
        public void TryPlace_AfterWarmUp_DoesNotAllocate()
        {
            var placement = new BoxPickupPlacement(new Rect(-10, -10, 20, 20),
                new[] { new Rect(1, -1, 2, 2), new Rect(-4, 2, 1, 5) }, Vector2.one * .5f, Vector2.zero, .01f);
            placement.TryPlace(new Vector2(2.2f, .3f), out _);
            Assert.That(() => { placement.TryPlace(new Vector2(-3.7f, 3.1f), out _); }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void TryPlace_SixtyFourFieldObstacles_FullProjectionStaysWithinPerfGuardBudget()
        {
            var obstacles = FieldObstacles(7);
            var placement = new BoxPickupPlacement(new Rect(-100, -100, 200, 200), obstacles, Vector2.one * .45f, Vector2.zero, .05f);
            placement.TryPlace(obstacles[0].center, out _);
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < 20; i++)
                Assert.IsTrue(placement.TryPlace(obstacles[i + 1].center, out _));
            stopwatch.Stop();
            // "Pickup.ReachablePlacement" warns above 5 ms per call; the original search took 20–30 ms per call in play.
            Assert.Less(stopwatch.ElapsedMilliseconds, 20 * 5, "Full projection must stay under the PerfGuard threshold on average.");
        }
    }
}
