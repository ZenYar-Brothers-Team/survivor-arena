using System;
using Game.Content;

namespace Game.MLAgents
{
    /// <summary>
    /// Resolved parameters for one MicroArena episode. Research-tool config, not gameplay content: values are
    /// read from ML-Agents EnvironmentParameters per training stage (see docs/research/2026-10-01-ml-agents-microarena-setup.md)
    /// rather than Assets/Resources/Content JSON, so curriculum stages can be driven entirely from the Python side
    /// without touching the Unity project between stages.
    /// </summary>
    public sealed class MicroArenaConfig
    {
        public float ArenaHalfSize { get; }
        public float PlayerRadius { get; }
        public float ThreatRadius { get; }
        public float XpRadius { get; }
        public float PlayerSpeed { get; }
        public float StartXOffset { get; }
        public float StartJitter { get; }
        public float ThreatAmplitude { get; }
        public float ThreatFrequencyHz { get; }
        public float TimeLimitSeconds { get; }
        public int Seed { get; }

        public MicroArenaConfig(
            float arenaHalfSize,
            float playerRadius,
            float threatRadius,
            float xpRadius,
            float playerSpeed,
            float startXOffset,
            float startJitter,
            float threatAmplitude,
            float threatFrequencyHz,
            float timeLimitSeconds,
            int seed)
        {
            NumericValidation.ValidatePositive(arenaHalfSize, nameof(arenaHalfSize));
            NumericValidation.ValidatePositive(playerRadius, nameof(playerRadius));
            NumericValidation.ValidatePositive(threatRadius, nameof(threatRadius));
            NumericValidation.ValidatePositive(xpRadius, nameof(xpRadius));
            NumericValidation.ValidatePositive(playerSpeed, nameof(playerSpeed));
            NumericValidation.ValidateNonNegative(startXOffset, nameof(startXOffset));
            NumericValidation.ValidateNonNegative(startJitter, nameof(startJitter));
            NumericValidation.ValidateNonNegative(threatAmplitude, nameof(threatAmplitude));
            if (threatFrequencyHz < 0f)
                throw new ArgumentOutOfRangeException(nameof(threatFrequencyHz));
            NumericValidation.ValidatePositive(timeLimitSeconds, nameof(timeLimitSeconds));
            if (startXOffset <= playerRadius + xpRadius)
                throw new ArgumentOutOfRangeException(nameof(startXOffset), "Start offset must clear the player/XP contact radii.");

            ArenaHalfSize = arenaHalfSize;
            PlayerRadius = playerRadius;
            ThreatRadius = threatRadius;
            XpRadius = xpRadius;
            PlayerSpeed = playerSpeed;
            StartXOffset = startXOffset;
            StartJitter = startJitter;
            ThreatAmplitude = threatAmplitude;
            ThreatFrequencyHz = threatFrequencyHz;
            TimeLimitSeconds = timeLimitSeconds;
            Seed = seed;
        }

        /// <summary>Stationary-threat short-distance default; matches the first successful curriculum stage in the research snapshot.</summary>
        public static MicroArenaConfig Default(int seed) => new MicroArenaConfig(
            arenaHalfSize: 6f,
            playerRadius: 0.30f,
            threatRadius: 0.55f,
            xpRadius: 0.45f,
            playerSpeed: 4f,
            startXOffset: 1.5f,
            startJitter: 0f,
            threatAmplitude: 0f,
            threatFrequencyHz: 0f,
            timeLimitSeconds: 2.5f,
            seed: seed);
    }
}
