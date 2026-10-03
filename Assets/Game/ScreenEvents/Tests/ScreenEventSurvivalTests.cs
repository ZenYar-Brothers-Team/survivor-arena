using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Game.ScreenEvents.Tests
{
    public sealed class ScreenEventSurvivalTests
    {
        private const float PlayerRadius = .35f;
        private const float Speed = 1.9f;
        private const float Cell = .4f;
        private static readonly Rect View = ScreenEventTestData.View;

        private static ScreenEventInstance Instance(params ScreenHazard[] hazards)
            => new ScreenEventInstance(new ScreenEventDefinition(ScreenEventTestData.Half()), new List<ScreenHazard>(hazards));

        [Test]
        public void CanSurvive_ANegativeControl_WholeScreenStrike_CannotBeSurvived()
        {
            var everything = ScreenHazard.Rect(0f, 1f, .5f, .2f, View.center, Vector2.right, 100f, 100f);
            Assert.IsFalse(ScreenEventSurvival.CanSurvive(Instance(everything), View, Vector2.zero, PlayerRadius, Speed, Cell));
        }

        [Test]
        public void CanSurvive_ANegativeControl_AHalfScreenStrikeWithTooShortAWarningCannotBeEscaped()
        {
            var leftHalf = ScreenHazard.Rect(0f, .5f, .4f, .2f, new Vector2(View.xMin - 5f + View.width * .25f, 0f), Vector2.right,
                View.width * .5f + 10f, 40f);
            Assert.IsFalse(ScreenEventSurvival.CanSurvive(Instance(leftHalf), View, new Vector2(View.xMin + 1f, 0f), PlayerRadius, Speed, Cell),
                "0.5 s at 1.9 units per second is under one unit of travel.");
        }

        [Test]
        public void CanSurvive_TheSameStrikeWithALongWarningCanBeEscaped_AndAWaitingPlayerIsSafeInTheOtherHalf()
        {
            var leftHalf = ScreenHazard.Rect(0f, 5f, .4f, .2f, new Vector2(View.xMin - 5f + View.width * .25f, 0f), Vector2.right,
                View.width * .5f + 10f, 40f);
            Assert.IsTrue(ScreenEventSurvival.CanSurvive(Instance(leftHalf), View, new Vector2(View.xMin + 1f, 0f), PlayerRadius, Speed, Cell));
            Assert.IsTrue(ScreenEventSurvival.CanSurvive(Instance(leftHalf), View, new Vector2(View.xMax - 1f, 0f), PlayerRadius, Speed, Cell));
        }

        [Test]
        public void CanSurvive_RingWithAGap_NeedsTheGapToBeReachable()
        {
            // The ring starts at the screen centre; its gap corridor points right. A player close to the corridor reaches it, a
            // slow one far on the opposite side with a short warning does not.
            var ring = ScreenHazard.Ring(0f, 2f, .2f, View.center, 1f, 12f, 3f, .6f, Vector2.right, 2.6f);
            Assert.IsTrue(ScreenEventSurvival.CanSurvive(Instance(ring), View, new Vector2(3f, .5f), PlayerRadius, Speed, Cell));
            var shortWarning = ScreenHazard.Ring(0f, .4f, .2f, View.center, 1f, 12f, 8f, .6f, Vector2.right, 2.6f);
            Assert.IsFalse(ScreenEventSurvival.CanSurvive(Instance(shortWarning), View, new Vector2(-7f, 3f), PlayerRadius, Speed, Cell));
        }

        [Test]
        public void CanSurvive_StartCellOutsideTheView_IsClampedInside()
        {
            var circle = ScreenHazard.Circle(0f, 1f, .5f, .2f, new Vector2(50f, 50f), 1f);
            Assert.IsTrue(ScreenEventSurvival.CanSurvive(Instance(circle), View, new Vector2(500f, -500f), PlayerRadius, Speed, Cell));
        }
    }
}
