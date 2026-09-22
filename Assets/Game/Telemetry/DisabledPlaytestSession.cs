namespace Game.Telemetry
{
    /// <summary>No subscriptions, files, counters, or gameplay dependency in release.</summary>
    public sealed class DisabledPlaytestSession : IPlaytestSession
    {
        public bool Enabled => false;
        public string Summary => "Recording disabled";
        public void AddMarker(string text) { }
        public void Export() { }
    }
}
