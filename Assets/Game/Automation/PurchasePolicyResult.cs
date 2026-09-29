using System.Collections.Generic;

namespace Game.Automation
{
    public sealed class PurchasePolicyResult
    {
        public List<PurchaseRecord> Purchases { get; } = new List<PurchaseRecord>();
        public SortedDictionary<string, string> Refusals { get; } = new SortedDictionary<string, string>();
        public string Error { get; internal set; }
    }
}
