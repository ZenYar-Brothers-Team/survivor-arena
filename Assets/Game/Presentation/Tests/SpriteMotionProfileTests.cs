using System;
using NUnit.Framework;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public sealed class SpriteMotionProfileTests
    {
        [Test]
        public void Constructor_AcceptsFixtureScaleValues()
        {
            var profile = CreateProfile();

            Assert.AreEqual("FIXTURE-MOTION", profile.Id.ToString());
            Assert.AreEqual(3f, profile.ReferenceSpeed);
            Assert.AreEqual(0.035f, profile.IdleBreathStretch);
            Assert.AreEqual(0.06f, profile.LocomotionStretch);
        }

        [Test]
        public void Constructor_RejectsValuesOutsideArtDirectionSafetyEnvelope()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateProfile(locomotionStretch: 0.13f));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateProfile(maxTiltDegrees: 8.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateProfile(spawnScaleFrom: 0.49f));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateProfile(idleSwayDegrees: 3.1f));
        }

        internal static SpriteMotionProfile CreateProfile(
            float locomotionStretch = 0.06f,
            float maxTiltDegrees = 6f,
            float spawnScaleFrom = 0.75f,
            float idleSwayDegrees = 1.4f)
        {
            return new SpriteMotionProfile(
                "FIXTURE-MOTION",
                3f,
                0.025f,
                0.675f,
                0.035f,
                idleSwayDegrees,
                0.05f,
                3f,
                locomotionStretch,
                maxTiltDegrees,
                0.14f,
                0.1f,
                7f,
                0.08f,
                new Color(1f, 0.45f, 0.45f, 1f),
                0.2f,
                spawnScaleFrom);
        }
    }
}
