namespace Game.MLAgents
{
    /// <summary>Terminal result of one MicroArena episode; checked in this order: collect, then collision, then timeout.</summary>
    public enum MicroArenaOutcome
    {
        None,
        Collected,
        Collision,
        Timeout
    }
}
