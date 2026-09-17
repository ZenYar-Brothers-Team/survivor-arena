namespace Game.UI
{
    public readonly struct RunOverlayViewState
    {
        public bool IsVisible { get; }
        public string Title { get; }
        public bool CanResume { get; }

        public RunOverlayViewState(bool isVisible, string title, bool canResume)
        {
            IsVisible = isVisible;
            Title = title ?? string.Empty;
            CanResume = canResume;
        }
    }
}
