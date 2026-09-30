namespace Game.UI
{
    /// <summary>Player-facing notification events (UI/UX §14). Text comes from <see cref="NotificationCopy"/>.</summary>
    public enum NotificationKind
    {
        None,
        LevelUp,
        SetAcquired,
        SetRecipeReady,
        BossIncoming,
        TravelerAppeared,
        TravelerEscaped,
        CharacterUnlocked,
        FieldUnlocked,
        ContentUnlocked,
        UnlocksSummary,
        Development
    }
}
