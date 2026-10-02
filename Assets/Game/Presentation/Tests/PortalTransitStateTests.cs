using NUnit.Framework;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public sealed class PortalTransitStateTests
    {
        [Test]
        public void Transit_ShrinksThenTravelsHidden_ArrivesAndRestoresWithoutJumpingCamera()
        {
            var state = new PortalTransitState(Vector2.zero, Vector2.right * 60f, .2f, 1f, .25f);
            Assert.AreEqual(1f, state.Scale); Assert.IsFalse(state.HasArrived);
            state.Tick(.1f, true);
            Assert.AreEqual(PortalTransitPhase.Collapsing, state.Phase);
            Assert.AreEqual(.5f, state.Scale, .001f); Assert.AreEqual(.5f, state.Flash, .001f);
            var time = state.Elapsed; var camera = state.CameraPosition;
            state.Tick(1f, false); Assert.AreEqual(time, state.Elapsed); Assert.AreEqual(camera, state.CameraPosition);
            state.Tick(.6f, true);
            Assert.AreEqual(PortalTransitPhase.Traveling, state.Phase);
            Assert.AreEqual(0f, state.Scale); Assert.IsFalse(state.HasArrived);
            Assert.AreEqual(30f, state.CameraPosition.x, .001f);
            state.Tick(.55f, true);
            Assert.IsTrue(state.HasArrived); Assert.AreEqual(PortalTransitPhase.Appearing, state.Phase);
            Assert.AreEqual(60f, state.CameraPosition.x); Assert.Greater(state.Scale, 0f);
            state.Tick(10f, true);
            Assert.AreEqual(PortalTransitPhase.Complete, state.Phase); Assert.AreEqual(1f, state.Scale); Assert.AreEqual(0f, state.Flash);
        }
        [Test]
        public void Transit_RejectsNonFinitePositionsAndMissingDurations()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new PortalTransitState(new Vector2(float.NaN, 0f), Vector2.one, .2f, 1f, .25f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new PortalTransitState(Vector2.zero, Vector2.one, .2f, 0f, .25f));
        }
    }
}
