using Game.Telemetry;

namespace Game.UI.Tests
{
    internal sealed class FakePlaytestSession : IPlaytestSession
    {
        public bool Enabled { get; set; }
        public string Summary { get; set; } = "fixture session";
        public int Exports;
        public string Marker;
        public void AddMarker(string text) { Marker = text; }
        public void Export() { Exports++; }
    }
}
