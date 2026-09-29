namespace Game.Automation
{
    /// <summary>The versioned choice policy; version changes must be reflected in comparison conditions.</summary>
    public sealed class DraftPolicyData
    {
        public string Id { get; set; }
        public int? Version { get; set; }
    }
}
