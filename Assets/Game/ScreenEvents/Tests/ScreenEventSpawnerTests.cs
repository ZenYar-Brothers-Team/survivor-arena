using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Game.ScreenEvents.Tests
{
    public sealed class ScreenEventSpawnerTests
    {
        private const float PlayerRadius = .35f;
        private static readonly Rect View = ScreenEventTestData.View;

        private static ScreenEventInstance Spawn(Game.ScreenEvents.Json.ScreenEventData data, int seed, Vector2? player = null)
            => ScreenEventSpawner.Create(new ScreenEventDefinition(data), View, player ?? Vector2.zero, PlayerRadius, new System.Random(seed));

        [Test]
        public void Create_IsDeterministicForASeed_AndVariesBetweenSeeds()
        {
            var first = Spawn(ScreenEventTestData.Circles(), 3);
            var again = Spawn(ScreenEventTestData.Circles(), 3);
            var other = Spawn(ScreenEventTestData.Circles(), 4);
            CollectionAssert.AreEqual(first.Hazards.Select(h => h.Origin).ToArray(), again.Hazards.Select(h => h.Origin).ToArray());
            CollectionAssert.AreNotEqual(first.Hazards.Select(h => h.Origin).ToArray(), other.Hazards.Select(h => h.Origin).ToArray());
        }

        [Test]
        public void Strips_StayParallelInsideTheScreenAndKeepTheirClearGap()
        {
            var data = ScreenEventTestData.Spear();
            var minimum = (data.WidthH.Value + data.MinGapH.Value) * View.height;
            for (var seed = 0; seed < 60; seed++)
            {
                var instance = Spawn(data, seed);
                Assert.That(instance.Hazards.Count, Is.InRange(1, 3));
                var across = instance.Hazards.Select(h => h.Origin.x).OrderBy(x => x).ToArray();
                for (var i = 1; i < across.Length; i++) Assert.GreaterOrEqual(across[i] - across[i - 1], minimum - 1e-3f);
                foreach (var hazard in instance.Hazards)
                {
                    Assert.Less(Vector2.Distance(Vector2.down, hazard.Direction), 1e-4f, "270 degrees is top to bottom.");
                    Assert.GreaterOrEqual(hazard.Origin.x - hazard.Width * .5f, View.xMin - 1e-3f);
                    Assert.LessOrEqual(hazard.Origin.x + hazard.Width * .5f, View.xMax + 1e-3f);
                    Assert.GreaterOrEqual(hazard.Length, View.height, "A strip crosses the whole screen.");
                }
            }
        }

        [Test]
        public void OrderedStrips_LightUpAcrossTheScreenInTurn()
        {
            var data = ScreenEventTestData.Pillars();
            var instance = Spawn(data, 11);
            Assert.AreEqual(5, instance.Hazards.Count);
            for (var i = 1; i < 5; i++)
            {
                Assert.Greater(instance.Hazards[i].Origin.x, instance.Hazards[i - 1].Origin.x);
                Assert.AreEqual(data.StaggerSeconds.Value, instance.Hazards[i].StartDelay - instance.Hazards[i - 1].StartDelay, 1e-4f);
            }
        }

        [Test]
        public void Cross_HasOneVerticalAndOneHorizontalStripThatReachTheEdges()
        {
            var instance = Spawn(ScreenEventTestData.Cross(), 5);
            Assert.AreEqual(2, instance.Hazards.Count);
            var vertical = instance.Hazards.Single(h => h.Direction == Vector2.up);
            var horizontal = instance.Hazards.Single(h => h.Direction == Vector2.right);
            Assert.GreaterOrEqual(vertical.Length, View.height);
            Assert.GreaterOrEqual(horizontal.Length, View.width);
        }

        [Test]
        public void Circles_NeverStartUnderThePlayer_OverlapOrLeaveTheScreen()
        {
            var data = ScreenEventTestData.Circles();
            for (var seed = 0; seed < 80; seed++)
            {
                var player = new Vector2(Mathf.Lerp(View.xMin + 2f, View.xMax - 2f, (seed * 37 % 100) / 100f), Mathf.Lerp(View.yMin + 2f, View.yMax - 2f, (seed * 53 % 100) / 100f));
                var circles = Spawn(data, seed, player).Hazards;
                Assert.That(circles.Count, Is.InRange(1, 5));
                for (var i = 0; i < circles.Count; i++)
                {
                    Assert.GreaterOrEqual((circles[i].Origin - player).magnitude, circles[i].Radius + PlayerRadius, "A circle is never right under the player.");
                    Assert.GreaterOrEqual(circles[i].Origin.x - circles[i].Radius, View.xMin - 1e-3f);
                    Assert.LessOrEqual(circles[i].Origin.y + circles[i].Radius, View.yMax + 1e-3f);
                    for (var j = 0; j < i; j++)
                        Assert.GreaterOrEqual((circles[i].Origin - circles[j].Origin).magnitude, circles[i].Radius + circles[j].Radius - 1e-3f);
                }
            }
        }

        [Test]
        public void Ring_StartsAtTheScreenCentreAndGrowsPastEveryCorner()
        {
            var ring = Spawn(ScreenEventTestData.Ring(), 2).Hazards.Single();
            Assert.AreEqual(View.center, ring.Origin);
            var corner = (new Vector2(View.xMax, View.yMax) - View.center).magnitude;
            Assert.Greater(ring.Radius + ring.Speed * ring.StrikeSeconds, corner);
            Assert.AreEqual(ScreenEventTestData.Ring().GapWidthH.Value * View.height, ring.GapWidth, 1e-4f);
        }

        [Test]
        public void HalfScreen_CoversExactlyOneSideAndReachesTheEdge()
        {
            for (var seed = 0; seed < 40; seed++)
            {
                var half = Spawn(ScreenEventTestData.Half(), seed).Hazards.Single();
                Assert.IsTrue(half.Covers(0f, half.Origin, 0f));
                Assert.IsFalse(half.Covers(0f, View.center - (half.Origin - View.center).normalized * View.height * .6f, 0f), "The opposite side stays clear.");
            }
        }

        [Test]
        public void Corners_StrikeAroundTheScreenOneByOne_AndLeaveTheMiddleClear()
        {
            var instance = Spawn(ScreenEventTestData.Corners(), 9);
            Assert.AreEqual(4, instance.Hazards.Count);
            for (var i = 0; i < 4; i++) Assert.AreEqual(i * .8f, instance.Hazards[i].StartDelay, 1e-4f);
            foreach (var hazard in instance.Hazards) Assert.IsFalse(hazard.Covers(0f, View.center, PlayerRadius), "The middle is never struck.");
            var corners = instance.Hazards.Select(h => (h.Origin.x > View.center.x ? 1 : 0) + (h.Origin.y > View.center.y ? 2 : 0)).Distinct().Count();
            Assert.AreEqual(4, corners, "One hazard per corner.");
        }

        [Test]
        public void Fan_StartsAtAnEdgeAndSpreadsSweepsInward()
        {
            var instance = Spawn(ScreenEventTestData.Fan(), 6);
            Assert.AreEqual(3, instance.Hazards.Count);
            Assert.AreEqual(1, instance.Hazards.Select(h => h.Origin).Distinct().Count(), "One apex.");
            Assert.AreEqual(3, instance.Hazards.Select(h => Mathf.Round(Vector2.SignedAngle(Vector2.right, h.Direction))).Distinct().Count());
        }

        [Test]
        public void Judgment_LeavesOnlyASafeCircleNearThePlayerInsideTheScreen()
        {
            var data = ScreenEventTestData.Judgment();
            for (var seed = 0; seed < 40; seed++)
            {
                var player = Vector2.zero;
                var judgment = Spawn(data, seed, player).Hazards.Single();
                Assert.AreEqual(ScreenBurstShape.OutsideCircle, judgment.Shape);
                Assert.AreEqual(data.SafeRadiusH.Value * View.height, judgment.Radius, 1e-4f);
                Assert.LessOrEqual((judgment.Origin - player).magnitude, data.PlayerDistanceMaxH.Value * View.height + 1e-3f);
                Assert.GreaterOrEqual(judgment.Origin.x - judgment.Radius, View.xMin - 1e-3f);
                Assert.IsFalse(judgment.Covers(0f, judgment.Origin, PlayerRadius));
            }
        }

        [Test]
        public void Create_RejectsAnEmptyView()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => ScreenEventSpawner.Create(new ScreenEventDefinition(ScreenEventTestData.Half()),
                new Rect(0, 0, 0, 5), Vector2.zero, PlayerRadius, new System.Random(1)));
        }
    }
}
