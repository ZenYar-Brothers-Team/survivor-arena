namespace Game.Telemetry
{
    /// <summary>Immutable export payload; safe to pass to a worker. Feedback is created only once.</summary>
    public sealed class PlaytestReport
    {
        public string ReportId { get; }
        public string Json { get; }
        public string Summary { get; }
        public string Feedback { get; }
        public PlaytestReport(string reportId, string json, string summary, string feedback)
        { ReportId = reportId; Json = json; Summary = summary; Feedback = feedback; }
    }
}
