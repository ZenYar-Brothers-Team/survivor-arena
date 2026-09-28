using System;
using NUnit.Framework;

namespace Game.UI.Tests
{
    public sealed class MenuArtProfileTests
    {
        [Test]
        public void DustMainDrift_IsSidewaysWithGentleRise()
        {
            var profile = MenuArtProfile.Load();
            profile.dustWander = 0;
            var delta = profile.DustPosition(0, 10) - profile.DustPosition(0, 0);
            Assert.Greater(delta.x, 0);
            Assert.Less(delta.y, 0);
            Assert.Greater(delta.x, UnityEngine.Mathf.Abs(delta.y) * 3);
        }

        [Test]
        public void DustMotion_IsSlowerCurvedAndDifferentBetweenParticles()
        {
            var profile = MenuArtProfile.Load();
            Assert.AreEqual(.2f, profile.dustSpeed);
            Assert.AreEqual(2f / 11, profile.DustCycle(0, 10), .00001f);
            var a = profile.DustPosition(0, 0);
            var b = profile.DustPosition(0, 5);
            var c = profile.DustPosition(0, 10);
            Assert.Greater(UnityEngine.Mathf.Abs(b.x - a.x), .001f);
            var ab = b - a;
            var bc = c - b;
            Assert.Greater(UnityEngine.Mathf.Abs(ab.x * bc.y - ab.y * bc.x), .00001f);
            Assert.Less(c.y, a.y);
            Assert.AreNotEqual(ab, profile.DustPosition(1, 5) - profile.DustPosition(1, 0));
            Assert.AreEqual(b, profile.DustPosition(0, 5));
        }

        [Test]
        public void Rays_AreThirtyPercentSlowerWithoutChangingAmplitude()
        {
            var profile = MenuArtProfile.Load();
            Assert.AreEqual(.7f, profile.raySpeed);
            Assert.AreEqual(3f, profile.rayAngle);
            var phase = profile.RayPhase(1, 12);
            profile.raySpeed = 1;
            Assert.AreEqual(profile.RayPhase(1, 12 * .7f), phase, .00001f);
            profile.dustWanderSeconds = 0;
            Assert.Throws<ArgumentOutOfRangeException>(() => profile.Validate());
        }

        [Test]
        public void AuthoredProfile_LoadsAndRejectsInvalidMotion()
        {
            var profile = MenuArtProfile.Load();
            Assert.AreEqual("UI-MENU-FRONT-VISUAL-BACKGROUND", profile.foregroundId);
            profile.driftSeconds = float.NaN;
            Assert.Throws<ArgumentOutOfRangeException>(() => profile.Validate());
        }

        [Test]
        public void Profile_RequiresLayersAndOrderedDustSizes()
        {
            var profile = MenuArtProfile.Load();
            profile.backgroundId = "";
            Assert.Throws<InvalidOperationException>(() => profile.Validate());
            profile = MenuArtProfile.Load();
            profile.dustMaxSize = profile.dustMinSize - 1;
            Assert.Throws<ArgumentOutOfRangeException>(() => profile.Validate());
        }
    }
}
