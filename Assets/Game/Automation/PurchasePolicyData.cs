using System.Collections.Generic;

namespace Game.Automation
{
    /// <summary>Between-run spending policy; empty allow-list means save all currency.</summary>
    public sealed class PurchasePolicyData
    {
        public string Id { get; set; }
        public int? Version { get; set; }
        public List<string> AllowedUpgradeIds { get; set; }
        public int? MaxPurchasesPerIntermission { get; set; }
    }
}
