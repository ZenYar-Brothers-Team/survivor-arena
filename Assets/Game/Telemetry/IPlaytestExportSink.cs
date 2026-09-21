namespace Game.Telemetry
{
    /// <summary>Worker-only I/O boundary. Throw on failure; the session reports it without affecting gameplay.</summary>
    public interface IPlaytestExportSink
    {
        string Write(PlaytestReport report);
    }
}
