namespace Game.Meta
{
    public sealed class MetaUpgradeData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool? Personal { get; set; }
        public string Stat { get; set; }
        public int? Cap { get; set; }
        public int? PriceCoefficient { get; set; }
        public float? Bonus { get; set; }
    }
}
