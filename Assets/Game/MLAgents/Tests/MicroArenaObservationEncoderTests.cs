using NUnit.Framework;
using UnityEngine;

namespace Game.MLAgents.Tests
{
    public sealed class MicroArenaObservationEncoderTests
    {
        [Test]
        public void Encode_WritesExpectedRelativeAndNormalizedValues()
        {
            var config = MicroArenaConfig.Default(1);
            var destination = new float[MicroArenaObservationEncoder.ObservationCount];

            MicroArenaObservationEncoder.Encode(
                destination,
                playerPosition: new Vector2(1f, 2f),
                xpPosition: new Vector2(3f, 2f),
                threatPosition: new Vector2(1f, -1f),
                threatVelocity: new Vector2(0f, 2f),
                config);

            Assert.AreEqual(1f / config.ArenaHalfSize, destination[0], 0.0001f);
            Assert.AreEqual(2f / config.ArenaHalfSize, destination[1], 0.0001f);
            Assert.AreEqual(2f / config.ArenaHalfSize, destination[2], 0.0001f);
            Assert.AreEqual(0f, destination[3], 0.0001f);
            Assert.AreEqual(0f, destination[4], 0.0001f);
            Assert.AreEqual(-3f / config.ArenaHalfSize, destination[5], 0.0001f);
            Assert.AreEqual(0f, destination[6], 0.0001f);
            Assert.AreEqual(2f / config.PlayerSpeed, destination[7], 0.0001f);
        }

        [Test]
        public void Encode_WrongDestinationLength_Throws()
        {
            var config = MicroArenaConfig.Default(1);
            var destination = new float[MicroArenaObservationEncoder.ObservationCount - 1];

            Assert.Throws<System.ArgumentException>(() => MicroArenaObservationEncoder.Encode(
                destination, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, config));
        }
    }
}
