namespace Game.Telemetry
{
    /// <summary>Rare diagnostic transition with simulation timestamp and bounded text.</summary>
    public sealed class TelemetryEvent
    {
        public long Sequence { get; }
        public float Seconds { get; }
        public string Kind { get; }
        public string Detail { get; }
        public TelemetryEvent(long sequence, float seconds, string kind, string detail)
        { Sequence = sequence; Seconds = seconds; Kind = kind; Detail = detail; }
    }
}
