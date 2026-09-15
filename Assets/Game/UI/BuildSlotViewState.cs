namespace Game.UI
{
    public readonly struct BuildSlotViewState
    {
        public string Title { get; }
        public int Level { get; }
        public bool IsOccupied { get; }

        public BuildSlotViewState(string title, int level, bool isOccupied)
        {
            Title = title ?? string.Empty;
            Level = level;
            IsOccupied = isOccupied;
        }
    }
}
