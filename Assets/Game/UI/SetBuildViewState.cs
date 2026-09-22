namespace Game.UI
{
    public readonly struct SetBuildViewState
    {
        public string Title { get; }
        public string Detail { get; }
        public UnityEngine.Sprite Icon { get; }

        public SetBuildViewState(string title, string detail = "", UnityEngine.Sprite icon = null)
        {
            Detail = detail ?? string.Empty;
            Icon = icon;
            Title = title ?? string.Empty;
        }
    }
}
