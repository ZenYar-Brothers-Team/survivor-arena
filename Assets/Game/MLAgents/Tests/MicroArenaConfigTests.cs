using System;
using NUnit.Framework;

namespace Game.MLAgents.Tests
{
    public sealed class MicroArenaConfigTests
    {
        [Test]
        public void Default_ProducesValidatedStationaryShortStageConfig()
        {
            var config = MicroArenaConfig.Default(42);

            Assert.AreEqual(6f, config.ArenaHalfSize);
            Assert.AreEqual(4f, config.PlayerSpeed);
            Assert.AreEqual(0f, config.ThreatAmplitude);
            Assert.AreEqual(42, config.Seed);
        }

        [Test]
        public void Constructor_StartOffsetInsideContactRadii_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MicroArenaConfig(
                arenaHalfSize: 6f,
                playerRadius: 0.3f,
                threatRadius: 0.55f,
                xpRadius: 0.45f,
                playerSpeed: 4f,
                startXOffset: 0.5f,
                startJitter: 0f,
                threatAmplitude: 0f,
                threatFrequencyHz: 0f,
                timeLimitSeconds: 2.5f,
                seed: 1));
        }

        [Test]
        public void Constructor_NonPositiveArenaHalfSize_Throws()
        {
            var d = MicroArenaConfig.Default(1);
            Assert.Throws<ArgumentOutOfRangeException>(() => new MicroArenaConfig(
                arenaHalfSize: 0f,
                playerRadius: d.PlayerRadius,
                threatRadius: d.ThreatRadius,
                xpRadius: d.XpRadius,
                playerSpeed: d.PlayerSpeed,
                startXOffset: d.StartXOffset,
                startJitter: d.StartJitter,
                threatAmplitude: d.ThreatAmplitude,
                threatFrequencyHz: d.ThreatFrequencyHz,
                timeLimitSeconds: d.TimeLimitSeconds,
                seed: d.Seed));
        }
    }
}
