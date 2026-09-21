using System.Collections.Generic;
namespace Game.Meta
{
    /// <summary>Persistence DTO only. Never expose mutable instances to views.</summary>
    public sealed class ProfileData
    {
        public int SchemaVersion { get; set; }
        public long Currency { get; set; }
        public bool FirstRun { get; set; }
        public Dictionary<string, int> Upgrades { get; set; }
        public HashSet<string> Unlocked { get; set; }
        public HashSet<string> ClearedFields { get; set; }
        public Dictionary<string, MetaRunReceipt> Runs { get; set; }
    }
}
