namespace Game.Enemy
{
    /// <summary>Explicit cause; only Killed represents death.</summary>
    public enum EnemyLifeReason
    {
        Spawn,
        PoolReuse,
        Killed,
        Cleanup,
        Escaped,
        Reinitialized,
        Destroyed
    }
}
