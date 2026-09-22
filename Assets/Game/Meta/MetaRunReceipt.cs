using System.Collections.Generic;
namespace Game.Meta
{
    public sealed class MetaRunReceipt
    {
        public string RunId { get; set; }
        public long LevelReward { get; set; }
        public long BookReward { get; set; }
        public List<string> NewUnlocks { get; set; }
        public long Total => checked(LevelReward + BookReward);
    }
}
