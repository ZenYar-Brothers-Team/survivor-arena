namespace Game.Enemy
{
    /// <summary>Per-tick decisions. Expired groups were skipped, not requested.
    /// Cap suppression is discarded, never accumulated for a later tick (DECISION-0029).</summary>
    public readonly struct WaveSpawnDecision
    {
        public int Requested { get; }
        public int Allowed { get; }
        public int Suppressed => Requested - Allowed;
        public int Expired { get; }
        public int Deferred => 0;

        public WaveSpawnDecision(int requested, int allowed, int expired)
        {
            if (requested < 0 || allowed < 0 || allowed > requested || expired < 0)
                throw new System.ArgumentOutOfRangeException(nameof(requested));
            Requested = requested;
            Allowed = allowed;
            Expired = expired;
        }
    }
}
