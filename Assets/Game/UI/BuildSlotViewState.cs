namespace Game.UI
{
    public readonly struct BuildSlotViewState
    {
        public string Title { get; }
        public string Detail { get; }
        public int Level { get; }
        public bool IsOccupied { get; }

        public BuildSlotViewState(string title, int level, bool isOccupied, string detail = null)
        {
            Title = title ?? string.Empty;
            Detail = detail ?? string.Empty;
            Level = level;
            IsOccupied = isOccupied;
        }
    }
}
