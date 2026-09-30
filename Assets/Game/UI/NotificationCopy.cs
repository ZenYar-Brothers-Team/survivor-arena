namespace Game.UI
{
    /// <summary>
    /// The only source of notification text (DECISION-0107). Future localization replaces this table;
    /// producers never build player-facing notification strings themselves.
    /// </summary>
    public static class NotificationCopy
    {
        public static string Title(NotificationMessage message)
        {
            switch (message.Kind)
            {
                case NotificationKind.LevelUp: return "Новый уровень";
                case NotificationKind.SetAcquired: return "Сет получен";
                case NotificationKind.SetRecipeReady: return "Рецепт сета готов";
                case NotificationKind.BossIncoming: return "Приближается босс";
                case NotificationKind.TravelerAppeared: return "Появился Путник";
                case NotificationKind.TravelerEscaped: return "Путник ушёл";
                case NotificationKind.CharacterUnlocked: return "Открыт персонаж";
                case NotificationKind.FieldUnlocked: return "Открыта карта";
                case NotificationKind.ContentUnlocked: return "Новое открытие";
                case NotificationKind.UnlocksSummary: return "Новые открытия";
                case NotificationKind.Development: return "DEV";
                default: return string.Empty;
            }
        }

        public static string Detail(NotificationMessage message)
        {
            switch (message.Kind)
            {
                case NotificationKind.LevelUp: return "Уровень " + message.Value;
                case NotificationKind.UnlocksSummary: return "Открыто: " + message.Value + " · подробнее в «Развитии»";
                default: return message.Subject;
            }
        }
    }
}
