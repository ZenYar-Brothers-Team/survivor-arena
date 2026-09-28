namespace Game.UI
{
    public sealed class RecipeComponentViewState
    {
        public string Name { get; }
        public int CurrentLevel { get; }
        public int ProjectedLevel { get; }
        public int RequiredLevel { get; }
        public bool IsSelected { get; }
        public bool IsOwned => CurrentLevel > 0;
        public bool IsLevelMet => CurrentLevel >= RequiredLevel;
        public string Text => $"{(IsOwned ? "✓" : "○")} {Name}  {CurrentLevel}" +
            (IsSelected ? $"→{ProjectedLevel}" : "") + $"/{RequiredLevel}";

        public RecipeComponentViewState(string name, int currentLevel, int projectedLevel, int requiredLevel, bool isSelected)
        {
            Name = name;
            CurrentLevel = currentLevel;
            ProjectedLevel = projectedLevel;
            RequiredLevel = requiredLevel;
            IsSelected = isSelected;
        }
    }
}
