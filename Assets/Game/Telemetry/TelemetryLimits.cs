using Game.Content;

namespace Game.Telemetry
{
    /// <summary>Validated diagnostic budgets, not gameplay tuning. IP-31 delivery contract.</summary>
    public sealed class TelemetryLimits
    {
        public int Timeline { get; }
        public int Aggregates { get; }
        public int Identities { get; }
        public int Text { get; }
        public int ExportBytes { get; }
        public const int ConfigBytes = 2 * 1024 * 1024;
        public TelemetryLimits(int timeline = 2048, int aggregates = 256, int identities = 4096,
            int text = 512, int exportBytes = 8 * 1024 * 1024)
        {
            NumericValidation.ValidateRange(timeline, 1, 2048, nameof(timeline));
            NumericValidation.ValidateRange(aggregates, 1, 256, nameof(aggregates));
            NumericValidation.ValidateRange(identities, 1, 4096, nameof(identities));
            NumericValidation.ValidateRange(text, 1, 512, nameof(text));
            NumericValidation.ValidateRange(exportBytes, 1, 8 * 1024 * 1024, nameof(exportBytes));
            Timeline = timeline; Aggregates = aggregates; Identities = identities;
            Text = text; ExportBytes = exportBytes;
        }
    }
}
