using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    public sealed class RingFormationPlannerTests
    {
        private readonly List<Vector2> _offsets = new List<Vector2>();
        private readonly List<bool> _assigned = new List<bool>();

        private int Plan(Vector2 player, List<Vector2> positions, float radius = 9f, float spacing = .9f, int max = 150) =>
            new RingFormationPlanner().Plan(player, positions, radius, spacing, max, _offsets, _assigned);

        [Test]
        public void Plan_ClumpedCrowd_SpreadsEvenlyOverTheCircle()
        {
            var positions = Enumerable.Range(0, 20).Select(i => new Vector2(12f + i * .05f, i * .05f)).ToList();

            var count = Plan(Vector2.zero, positions);

            Assert.AreEqual(20, count);
            Assert.IsTrue(_assigned.All(a => a));
            Assert.IsTrue(_offsets.All(o => Mathf.Abs(o.magnitude - 9f) < .001f));
            var angles = _offsets.Select(o => Mathf.Atan2(o.y, o.x)).OrderBy(a => a).ToList();
            for (var i = 0; i < angles.Count; i++)
            {
                var next = i + 1 < angles.Count ? angles[i + 1] : angles[0] + 2f * Mathf.PI;
                Assert.AreEqual(2f * Mathf.PI / 20f, next - angles[i], .001f);
            }
        }

        [Test]
        public void Plan_SlotsAreOffsetsFromThePlayer()
        {
            var positions = new List<Vector2> { new Vector2(110f, 100f), new Vector2(90f, 100f) };

            Plan(new Vector2(100f, 100f), positions);

            Assert.IsTrue(_offsets.All(o => Mathf.Abs(o.magnitude - 9f) < .001f));
        }

        [Test]
        public void Plan_ParticipantsKeepTheirSideOfTheCircle()
        {
            var positions = new List<Vector2> { new Vector2(15f, 0f), new Vector2(-15f, 0f) };

            Plan(Vector2.zero, positions);

            Assert.Greater(Vector2.Dot(_offsets[0], positions[0]), 0f);
            Assert.Greater(Vector2.Dot(_offsets[1], positions[1]), 0f);
        }

        [Test]
        public void Plan_MoreThanOneRingCapacity_SpillsToOuterRings()
        {
            // Capacity of a 9-unit ring at 0.9 spacing is 62 slots.
            var positions = Enumerable.Range(0, 100).Select(i => new Vector2(10f + i * .1f, i * .3f)).ToList();

            Plan(Vector2.zero, positions);

            var radii = _offsets.Select(o => Mathf.Round(o.magnitude * 10f) / 10f).Distinct().OrderBy(r => r).ToList();
            Assert.AreEqual(2, radii.Count);
            Assert.AreEqual(9f, radii[0], .01f);
            Assert.AreEqual(9.9f, radii[1], .01f);
            Assert.AreEqual(62, _offsets.Count(o => Mathf.Abs(o.magnitude - 9f) < .01f));
        }

        [Test]
        public void Plan_MoreThanMaxParticipants_AssignsOnlyTheClosest()
        {
            var positions = Enumerable.Range(0, 10).Select(i => new Vector2(10f + i, 0f)).ToList();

            var count = Plan(Vector2.zero, positions, max: 4);

            Assert.AreEqual(4, count);
            for (var i = 0; i < 10; i++) Assert.AreEqual(i < 4, _assigned[i]);
        }

        [Test]
        public void Plan_Empty_AssignsNothing()
        {
            Assert.AreEqual(0, Plan(Vector2.zero, new List<Vector2>()));
        }
    }
}
