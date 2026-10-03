using NUnit.Framework;
using UnityEngine;

namespace Game.ScreenEvents.Tests
{
    public sealed class ScreenHazardTests
    {
        private const float PlayerRadius = .35f;

        [Test]
        public void PhaseAt_FollowsDelayTelegraphStrikeThenDone()
        {
            var hazard = ScreenHazard.Rect(1f, 2f, .5f, .2f, Vector2.zero, Vector2.right, 4f, 1f);
            Assert.AreEqual(ScreenHazardPhase.Waiting, hazard.PhaseAt(.99f));
            Assert.AreEqual(ScreenHazardPhase.Telegraph, hazard.PhaseAt(1f));
            Assert.AreEqual(ScreenHazardPhase.Strike, hazard.PhaseAt(3f));
            Assert.AreEqual(ScreenHazardPhase.Done, hazard.PhaseAt(3.5f));
            Assert.AreEqual(3.5f, hazard.TotalSeconds, 1e-5f);
            Assert.AreEqual(.5f, hazard.TelegraphProgressAt(2f), 1e-5f);
        }

        [Test]
        public void Rect_CoversInsideWithPlayerBody_NotOutside_NotBeforeStrike()
        {
            var hazard = ScreenHazard.Rect(0f, 1f, .5f, .2f, Vector2.zero, Vector2.up, 6f, 2f);
            Assert.IsTrue(hazard.Covers(0f, new Vector2(1.2f, 0f), PlayerRadius), "The body touches the half-width plus radius.");
            Assert.IsFalse(hazard.Covers(0f, new Vector2(1.5f, 0f), PlayerRadius));
            Assert.IsFalse(hazard.Covers(0f, new Vector2(0f, 3.5f), PlayerRadius), "Beyond the half-length.");
            Assert.IsFalse(hazard.Covers(-.1f, Vector2.zero, PlayerRadius), "A warning does not hurt.");
            Assert.IsFalse(hazard.Covers(.6f, Vector2.zero, PlayerRadius), "The strike is over.");
        }

        [Test]
        public void Circle_AndOutsideCircle_AreComplements()
        {
            var circle = ScreenHazard.Circle(0f, 1f, .5f, .2f, Vector2.zero, 2f);
            var outside = ScreenHazard.Circle(0f, 1f, .5f, .2f, Vector2.zero, 2f, outside: true);
            Assert.IsTrue(circle.Covers(0f, new Vector2(2.3f, 0f), PlayerRadius));
            Assert.IsFalse(circle.Covers(0f, new Vector2(2.4f, 0f), PlayerRadius));
            Assert.IsFalse(outside.Covers(0f, new Vector2(1.5f, 0f), PlayerRadius), "A player wholly inside the safe circle is safe.");
            Assert.IsTrue(outside.Covers(0f, new Vector2(1.8f, 0f), PlayerRadius), "A player straddling the rim is hurt.");
            Assert.IsTrue(outside.Covers(0f, new Vector2(50f, 0f), PlayerRadius));
        }

        [Test]
        public void Sweep_HurtsOnlyWhereTheTravellingBarIsNow()
        {
            // Strip from the origin along +x, 10 long, 2 wide; bar 3 long at 5 units per second.
            var sweep = ScreenHazard.Sweep(0f, 1f, .2f, Vector2.zero, Vector2.right, 10f, 2f, 3f, 5f);
            Assert.AreEqual(13f / 5f, sweep.StrikeSeconds, 1e-5f);
            var point = new Vector2(4f, 0f);
            Assert.IsFalse(sweep.Covers(0f, point, PlayerRadius), "The bar has not got there yet.");
            Assert.IsTrue(sweep.Covers(1f, point, PlayerRadius), "Head at 5, tail at 2: the point is inside the bar.");
            Assert.IsFalse(sweep.Covers(2f, point, PlayerRadius), "Head at 10, tail at 7: it has passed.");
            Assert.IsFalse(sweep.Covers(1f, new Vector2(4f, 1.5f), PlayerRadius), "Outside the strip width.");
        }

        [Test]
        public void Ring_HurtsOnTheBandAndSparesTheGapCorridor()
        {
            var ring = ScreenHazard.Ring(0f, 1f, .2f, Vector2.zero, 1f, 9f, 2f, .6f, Vector2.right, 2f);
            Assert.AreEqual(4f, ring.StrikeSeconds, 1e-5f);
            Assert.AreEqual(3f, ring.RingRadiusAt(1f), 1e-5f);
            Assert.IsTrue(ring.Covers(1f, new Vector2(0f, 3f), PlayerRadius), "On the band, away from the gap.");
            Assert.IsFalse(ring.Covers(1f, new Vector2(0f, 6f), PlayerRadius), "Off the band.");
            Assert.IsFalse(ring.Covers(1f, new Vector2(3f, 0f), PlayerRadius), "Inside the gap corridor.");
            Assert.IsTrue(ring.Covers(1f, new Vector2(-3f, 0f), PlayerRadius), "The opposite side has no gap.");
            Assert.IsTrue(ring.Covers(1f, new Vector2(3f, .9f), PlayerRadius), "A player straddling the corridor wall is hurt.");
        }

        [Test]
        public void Factories_RejectInvalidNumbers()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => ScreenHazard.Rect(0f, 1f, .5f, 1.2f, Vector2.zero, Vector2.up, 1f, 1f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => ScreenHazard.Rect(0f, 0f, .5f, .2f, Vector2.zero, Vector2.up, 1f, 1f));
            Assert.Throws<System.ArgumentException>(() => ScreenHazard.Ring(0f, 1f, .2f, Vector2.zero, 3f, 2f, 1f, .5f, Vector2.right, 1f));
        }
    }
}
