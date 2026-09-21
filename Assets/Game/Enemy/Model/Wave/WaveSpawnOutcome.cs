using Game.Content;

namespace Game.Enemy
{
    /// <summary>Immutable producer fact for UI/telemetry; actual is recorded after execution.</summary>
    public readonly struct WaveSpawnOutcome
    {
        public ContentId PhaseId { get; }
        public float Elapsed { get; }
        public WaveSpawnMode Mode { get; }
        public WaveSpawnDecision Decision { get; }
        public int Actual { get; }
        public int Unavailable => Decision.Allowed - Actual;
        public int RegularAlive { get; }
        public int RegularCap { get; }

        public WaveSpawnOutcome(ContentId phaseId, float elapsed, WaveSpawnMode mode,
            WaveSpawnDecision decision, int actual, int regularAlive, int regularCap)
        {
            if (actual < 0 || actual > decision.Allowed)
                throw new System.ArgumentOutOfRangeException(nameof(actual));
            PhaseId = phaseId;
            Elapsed = elapsed;
            Mode = mode;
            Decision = decision;
            Actual = actual;
            RegularAlive = regularAlive;
            RegularCap = regularCap;
        }
    }
}
