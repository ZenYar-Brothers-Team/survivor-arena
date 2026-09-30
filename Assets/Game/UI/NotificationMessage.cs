namespace Game.UI
{
    /// <summary>
    /// Language-neutral notification: producers pass the event kind and its subject (content display name,
    /// level number or development text); the view renders it through <see cref="NotificationCopy"/>.
    /// </summary>
    public readonly struct NotificationMessage
    {
        public NotificationKind Kind { get; }
        public string Subject { get; }
        public int Value { get; }
        public bool IsEmpty => Kind == NotificationKind.None;

        public NotificationMessage(NotificationKind kind, string subject = "", int value = 0)
        {
            Kind = kind;
            Subject = subject ?? string.Empty;
            Value = value;
        }

        public static NotificationMessage LevelUp(int level) => new NotificationMessage(NotificationKind.LevelUp, value: level);
    }
}
