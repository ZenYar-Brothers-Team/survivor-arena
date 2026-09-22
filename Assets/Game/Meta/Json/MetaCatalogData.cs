namespace Game.Meta
{
    public sealed class MetaCatalogData
    {
        public long? RewardPerLevel { get; set; }
        public long? EmptyBookReward { get; set; }
        public float? FieldClearSeconds { get; set; }
        public MetaUpgradeData[] Upgrades { get; set; }
        public MetaUnlockData[] Unlocks { get; set; }
    }
}
