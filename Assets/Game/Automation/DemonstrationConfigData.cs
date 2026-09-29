using System;
using Game.Content;

namespace Game.Automation
{
    /// <summary>IP-34 AB-14: explicit recording budgets; these do not change gameplay.</summary>
    public sealed class DemonstrationConfigData
    {
        public int? SchemaVersion { get; set; }
        public float? SampleIntervalSeconds { get; set; }
        public int? MaxSamples { get; set; }
        public int? MaxFileMegabytes { get; set; }
        public int? QueueCapacity { get; set; }
        public int? MaxEntitiesPerCollection { get; set; }

        public void Validate()
        {
            if (SchemaVersion != 1 || !SampleIntervalSeconds.HasValue || !MaxSamples.HasValue ||
                !MaxFileMegabytes.HasValue || !QueueCapacity.HasValue || !MaxEntitiesPerCollection.HasValue)
                throw new ArgumentException("All version-1 demonstration settings are required.");
            NumericValidation.ValidateRange(SampleIntervalSeconds.Value, 0.02f, 0.5f, nameof(SampleIntervalSeconds));
            NumericValidation.ValidateRange(MaxSamples.Value, 1, 1000000, nameof(MaxSamples));
            NumericValidation.ValidateRange(MaxFileMegabytes.Value, 1, 2048, nameof(MaxFileMegabytes));
            NumericValidation.ValidateRange(QueueCapacity.Value, 1, 1024, nameof(QueueCapacity));
            NumericValidation.ValidateRange(MaxEntitiesPerCollection.Value, 1, 2048, nameof(MaxEntitiesPerCollection));
        }
    }
}
