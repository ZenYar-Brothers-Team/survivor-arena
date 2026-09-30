namespace Game.Meta
{
    public sealed class MetaUnlockData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Kind { get; set; }
        public string Condition { get; set; }
        public string RequiredId { get; set; }
        public long? Price { get; set; }
        public string Metric { get; set; }
        public string TargetId { get; set; }
        public long? TargetCount { get; set; }
        public string[] Grants { get; set; }
    }
}
