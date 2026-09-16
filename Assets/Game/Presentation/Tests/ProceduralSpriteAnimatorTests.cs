using NUnit.Framework;
using UnityEngine;

namespace Game.Presentation.Tests
{
    public sealed class ProceduralSpriteAnimatorTests
    {
        [Test]
        public void Tick_ComposesLocomotionHitAndSpawnWithinProfileBounds()
        {
            var animator = new ProceduralSpriteAnimator(SpriteMotionProfileTests.CreateProfile());
            animator.PlayHit(2f);

            var pose = animator.Tick(0.025f, isRunning: true, new Vector2(3f, 0f));

            Assert.AreNotEqual(Vector3.zero, pose.PositionOffset);
            Assert.AreNotEqual(Vector2.one, pose.ScaleMultiplier);
            Assert.That(pose.RotationDegrees, Is.InRange(-8f, 8f));
            Assert.Greater(pose.FlashAmount, 0f);
            Assert.IsFalse(pose.FlipX);
        }

        [Test]
        public void Tick_WithZeroVelocity_ProducesVisibleIdlePose()
        {
            var animator = new ProceduralSpriteAnimator(SpriteMotionProfileTests.CreateProfile());

            var pose = animator.Tick(0.12f, isRunning: true, Vector2.zero);

            Assert.AreNotEqual(Vector3.zero, pose.PositionOffset);
            Assert.AreNotEqual(Vector2.one, pose.ScaleMultiplier);
            Assert.AreNotEqual(0f, pose.RotationDegrees);
            Assert.IsFalse(pose.FlipX);
        }

        [Test]
        public void Tick_DiagonalVelocity_UsesSpeedMagnitudeButHorizontalTiltAndFacing()
        {
            var profile = SpriteMotionProfileTests.CreateProfile();
            var diagonalAnimator = new ProceduralSpriteAnimator(profile);
            var horizontalAnimator = new ProceduralSpriteAnimator(profile);
            var diagonalVelocity = new Vector2(-1f, 1f).normalized * profile.ReferenceSpeed;

            var diagonalPose = diagonalAnimator.Tick(0.04f, isRunning: true, diagonalVelocity);
            var horizontalPose = horizontalAnimator.Tick(
                0.04f,
                isRunning: true,
                Vector2.left * profile.ReferenceSpeed);

            Assert.AreEqual(horizontalPose.PositionOffset.y, diagonalPose.PositionOffset.y, 0.0001f);
            Assert.AreEqual(horizontalPose.ScaleMultiplier.x, diagonalPose.ScaleMultiplier.x, 0.0001f);
            Assert.AreEqual(horizontalPose.ScaleMultiplier.y, diagonalPose.ScaleMultiplier.y, 0.0001f);
            Assert.Less(Mathf.Abs(diagonalPose.RotationDegrees), Mathf.Abs(horizontalPose.RotationDegrees));
            Assert.IsTrue(diagonalPose.FlipX);
        }

        [Test]
        public void Tick_EquivalentElapsedTime_IsFrameRateIndependent()
        {
            var thirtyFps = new ProceduralSpriteAnimator(SpriteMotionProfileTests.CreateProfile());
            var oneHundredTwentyFps = new ProceduralSpriteAnimator(SpriteMotionProfileTests.CreateProfile());
            var velocity = new Vector2(1f, 1f).normalized * 2.25f;

            SpritePose thirtyFpsPose = default;
            for (var i = 0; i < 30; i++)
                thirtyFpsPose = thirtyFps.Tick(1f / 30f, isRunning: true, velocity);

            SpritePose oneHundredTwentyFpsPose = default;
            for (var i = 0; i < 120; i++)
                oneHundredTwentyFpsPose = oneHundredTwentyFps.Tick(1f / 120f, isRunning: true, velocity);

            AssertPoseApproximatelyEqual(thirtyFpsPose, oneHundredTwentyFpsPose, 0.0001f);
        }

        [Test]
        public void Tick_CalibratedMotionStaysInsidePresentationSafetyEnvelope()
        {
            var profile = SpriteMotionProfileTests.CreateProfile();
            var animator = new ProceduralSpriteAnimator(profile);
            animator.PlayHit(1f);

            for (var i = 0; i < 240; i++)
            {
                var angle = i * Mathf.Deg2Rad * 7f;
                var velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * profile.ReferenceSpeed;
                var pose = animator.Tick(1f / 120f, isRunning: true, velocity);

                Assert.LessOrEqual(Mathf.Abs(pose.PositionOffset.y), 0.05f + 0.0001f);
                Assert.That(pose.ScaleMultiplier.x, Is.InRange(0.65f, 1.15f));
                Assert.That(pose.ScaleMultiplier.y, Is.InRange(0.65f, 1.15f));
                Assert.That(pose.RotationDegrees, Is.InRange(-8f, 8f));
                Assert.That(pose.FlashAmount, Is.InRange(0f, 1f));
            }
        }

        [Test]
        public void Pause_FreezesPoseAndFacingUntilRunningResumes()
        {
            var animator = new ProceduralSpriteAnimator(SpriteMotionProfileTests.CreateProfile());
            var movingLeft = animator.Tick(0.04f, isRunning: true, Vector2.left * 3f);

            var paused = animator.Tick(1f, isRunning: false, Vector2.right * 3f);

            AssertPoseEqual(movingLeft, paused);
            Assert.IsTrue(paused.FlipX);

            var resumed = animator.Tick(0.04f, isRunning: true, Vector2.right * 3f);
            Assert.IsFalse(resumed.FlipX);
        }

        [Test]
        public void Reset_ClearsHitAndFacingAndRestartsSpawn()
        {
            var profile = SpriteMotionProfileTests.CreateProfile();
            var animator = new ProceduralSpriteAnimator(profile);
            animator.Tick(0.2f, isRunning: true, Vector2.left * 3f);
            animator.PlayHit(1f);
            animator.Tick(0.01f, isRunning: true, Vector2.left * 3f);

            animator.Reset();
            var pose = animator.CurrentPose;

            Assert.IsFalse(pose.FlipX);
            Assert.AreEqual(0f, pose.FlashAmount);
            Assert.AreEqual(Vector3.zero, pose.PositionOffset);
            Assert.AreEqual(profile.SpawnScaleFrom, pose.ScaleMultiplier.x, 0.0001f);
            Assert.AreEqual(profile.SpawnScaleFrom, pose.ScaleMultiplier.y, 0.0001f);
        }

        private static void AssertPoseEqual(SpritePose expected, SpritePose actual)
        {
            Assert.AreEqual(expected.PositionOffset, actual.PositionOffset);
            Assert.AreEqual(expected.RotationDegrees, actual.RotationDegrees);
            Assert.AreEqual(expected.ScaleMultiplier, actual.ScaleMultiplier);
            Assert.AreEqual(expected.FlipX, actual.FlipX);
            Assert.AreEqual(expected.FlashAmount, actual.FlashAmount);
        }

        private static void AssertPoseApproximatelyEqual(
            SpritePose expected,
            SpritePose actual,
            float tolerance)
        {
            Assert.AreEqual(expected.PositionOffset.x, actual.PositionOffset.x, tolerance);
            Assert.AreEqual(expected.PositionOffset.y, actual.PositionOffset.y, tolerance);
            Assert.AreEqual(expected.RotationDegrees, actual.RotationDegrees, tolerance);
            Assert.AreEqual(expected.ScaleMultiplier.x, actual.ScaleMultiplier.x, tolerance);
            Assert.AreEqual(expected.ScaleMultiplier.y, actual.ScaleMultiplier.y, tolerance);
            Assert.AreEqual(expected.FlipX, actual.FlipX);
            Assert.AreEqual(expected.FlashAmount, actual.FlashAmount, tolerance);
        }
    }
}
