using NUnit.Framework;
using UnityEngine;

namespace Game.Movement.Tests
{
    public class MovementVelocityCalculatorTests
    {
        [Test]
        public void NoInput_ReturnsZeroVelocity()
        {
            var velocity = MovementVelocityCalculator.Calculate(Vector2.zero, 5f, isRunning: true);

            Assert.AreEqual(Vector2.zero, velocity);
        }

        [Test]
        public void InputRight_ReturnsSpeedAlongX()
        {
            var velocity = MovementVelocityCalculator.Calculate(Vector2.right, 5f, isRunning: true);

            Assert.AreEqual(new Vector2(5f, 0f), velocity);
        }

        [Test]
        public void DiagonalInput_DoesNotExceedSpeedMagnitude()
        {
            var velocity = MovementVelocityCalculator.Calculate(new Vector2(1f, 1f), 5f, isRunning: true);

            Assert.LessOrEqual(velocity.magnitude, 5f + 0.0001f);
        }

        [Test]
        public void NotRunning_AlwaysReturnsZeroVelocity()
        {
            var velocity = MovementVelocityCalculator.Calculate(Vector2.right, 5f, isRunning: false);

            Assert.AreEqual(Vector2.zero, velocity);
        }

        [Test]
        public void ChangedSpeed_ChangesVelocityMagnitudeWithoutChangingDirection()
        {
            var normal = MovementVelocityCalculator.Calculate(Vector2.right, 3f, isRunning: true);
            var modified = MovementVelocityCalculator.Calculate(Vector2.right, 4.5f, isRunning: true);

            Assert.AreEqual(Vector2.right, normal.normalized);
            Assert.AreEqual(Vector2.right, modified.normalized);
            Assert.AreEqual(3f, normal.magnitude, 0.0001f);
            Assert.AreEqual(4.5f, modified.magnitude, 0.0001f);
        }
    }
}
