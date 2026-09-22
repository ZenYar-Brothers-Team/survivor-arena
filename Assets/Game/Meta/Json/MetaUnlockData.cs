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
    }
}
