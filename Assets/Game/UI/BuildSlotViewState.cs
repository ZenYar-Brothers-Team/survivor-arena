namespace Game.UI
{
    public readonly struct BuildSlotViewState
    {
        public string Title { get; }
        public string Detail { get; }
        public int Level { get; }
        public bool IsOccupied { get; }
        public UnityEngine.Sprite Icon { get; }

        public BuildSlotViewState(string title, int level, bool isOccupied, string detail = null,
            UnityEngine.Sprite icon = null)
        {
            Title = title ?? string.Empty;
            Detail = detail ?? string.Empty;
            Level = level;
            IsOccupied = isOccupied;
            Icon = icon;
        }
    }
}
