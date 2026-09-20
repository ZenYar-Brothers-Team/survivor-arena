namespace Game.Run
{
    /// <summary>Session completion cause; administrative stops do not grant gameplay victory.</summary>
    public enum RunCompletionReason
    {
        Victory,
        Defeat,
        Aborted,
        Retry,
        Error
    }
}
