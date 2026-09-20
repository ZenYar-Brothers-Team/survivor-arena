namespace Game.UI
{
    public readonly struct SetBuildViewState
    {
        public string Title { get; }

        public SetBuildViewState(string title)
        {
            Title = title ?? string.Empty;
        }
    }
}
