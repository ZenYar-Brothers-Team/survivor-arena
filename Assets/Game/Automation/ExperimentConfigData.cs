using System.Collections.Generic;

namespace Game.Automation
{
    /// <summary>External tool configuration. Required values remain nullable until validation.</summary>
    public sealed class ExperimentConfigData
    {
        public int? SchemaVersion { get; set; }
        public string ExperimentId { get; set; }
        public string Template { get; set; }
        public string InitialProfilePath { get; set; }
        public int? Chains { get; set; }
        public int? MaxRunsPerChain { get; set; }
        public int? RunSpeed { get; set; }
        public string CharacterId { get; set; }
        public List<string> FieldRoute { get; set; }
        public MovementPolicyData MovementPolicy { get; set; }
        public DraftPolicyData DraftPolicy { get; set; }
        public PurchasePolicyData PurchasePolicy { get; set; }
        public bool? StopAfterRouteClear { get; set; }
        public float? MaxExperimentWallSeconds { get; set; }
        public float? RunWallTimeoutSeconds { get; set; }
        public float? TransitionTimeoutSeconds { get; set; }
        public string OutputDirectory { get; set; }
    }
}
