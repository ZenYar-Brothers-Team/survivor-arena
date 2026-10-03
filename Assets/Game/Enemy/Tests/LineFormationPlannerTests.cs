using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    public sealed class LineFormationPlannerTests
    {
        private readonly List<Vector2> _offsets = new List<Vector2>();
        private readonly List<bool> _assigned = new List<bool>();
        private readonly LineFormationPlanner _planner = new LineFormationPlanner();

        private static List<Vector2> Crowd(int count) =>
            Enumerable.Range(0, count).Select(i => new Vector2(14f + i % 10 * .3f, (i / 10 - 3) * .3f)).ToList();

        [Test]
        public void PlanWall_PutsTheFrontRowAheadAndAcrossTheHeading()
        {
            var forward = Vector2.up;

            var count = _planner.PlanWall(Vector2.zero, Crowd(15), forward, 4.5f, 18f, .9f, 150, _offsets, _assigned);

            Assert.AreEqual(15, count);
            Assert.IsTrue(_offsets.All(o => Mathf.Abs(o.y - 4.5f) < .001f), "One row ahead of the player.");
            Assert.Greater(_offsets.Max(o => o.x) - _offsets.Min(o => o.x), 12f, "The row spans most of the wall width.");
        }

        [Test]
        public void PlanWall_MoreThanOneRow_StacksExtraRowsBehindTheFront()
        {
            // 18 / 0.9 + 1 = 21 slots per row.
            var count = _planner.PlanWall(Vector2.zero, Crowd(30), Vector2.up, 4.5f, 18f, .9f, 150, _offsets, _assigned);

            Assert.AreEqual(30, count);
            Assert.AreEqual(21, _offsets.Count(o => Mathf.Abs(o.y - 4.5f) < .001f));
            Assert.AreEqual(9, _offsets.Count(o => Mathf.Abs(o.y - 5.4f) < .001f));
        }

        [Test]
        public void PlanPincer_BuildsTwoColumnsOnBothSidesOfThePlayer()
        {
            var crowd = new List<Vector2>();
            for (var i = 0; i < 10; i++) { crowd.Add(new Vector2(i, 12f)); crowd.Add(new Vector2(i, -12f)); }

            var count = _planner.PlanPincer(Vector2.zero, crowd, Vector2.right, 14f, 8f, .9f, 150, _offsets, _assigned);

            Assert.AreEqual(20, count);
            Assert.AreEqual(10, _offsets.Count(o => Mathf.Abs(o.y - 4f) < .001f));
            Assert.AreEqual(10, _offsets.Count(o => Mathf.Abs(o.y + 4f) < .001f));
        }

        [Test]
        public void Plan_AnyFormation_RespectsTheParticipantCap()
        {
            var count = _planner.PlanWall(Vector2.zero, Crowd(40), Vector2.up, 4.5f, 18f, .9f, 10, _offsets, _assigned);

            Assert.AreEqual(10, count);
            Assert.AreEqual(10, _assigned.Count(a => a));
        }
    }
}
