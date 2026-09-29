using System.Collections.Generic;

namespace Game.Automation
{
    /// <summary>Declarative laboratory starting point, never represented as earned run receipts.</summary>
    public sealed class PresetProfileData
    {
        public long? Currency { get; set; }
        public bool? FirstRun { get; set; }
        public bool? UpgradesDisabled { get; set; }
        public Dictionary<string, int> Upgrades { get; set; }
        public List<string> Unlocked { get; set; }
        public List<string> ClearedFields { get; set; }
    }
}
