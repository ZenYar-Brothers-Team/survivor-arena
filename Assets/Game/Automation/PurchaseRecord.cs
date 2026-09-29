namespace Game.Automation
{
    public sealed class PurchaseRecord
    {
        public string UpgradeId { get; }
        public int PreviousLevel { get; }
        public int NewLevel { get; }
        public long Price { get; }
        public long CurrencyAfter { get; }
        public PurchaseRecord(string upgradeId, int previousLevel, long price, long currencyAfter)
        { UpgradeId = upgradeId; PreviousLevel = previousLevel; NewLevel = previousLevel + 1;
            Price = price; CurrencyAfter = currencyAfter; }
    }
}
