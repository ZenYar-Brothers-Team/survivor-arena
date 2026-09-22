namespace Game.UI
{
    /// <summary>Immutable diagnostic status; no gameplay state owned by the view.</summary>
    public sealed class PlaytestViewState
    {
        public bool Enabled { get; }
        public string Summary { get; }
        public PlaytestViewState(bool enabled, string summary) { Enabled = enabled; Summary = summary; }
    }
}
