namespace Game.Telemetry
{
    /// <summary>UI-facing diagnostic commands. None changes game state.</summary>
    public interface IPlaytestSession
    {
        bool Enabled { get; }
        string Summary { get; }
        void AddMarker(string text);
        void Export();
    }
}
