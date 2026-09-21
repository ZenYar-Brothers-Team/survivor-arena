namespace Game.UI
{
    public sealed class MetaCardViewState
    {
        public string Id { get; }
        public string Character { get; }
        public int Level { get; }
        public string Text { get; }
        public string Detail { get; }
        public bool CanBuy { get; }
        public MetaCardViewState(string id, string character, int level, string text, string detail, bool canBuy)
        { Id = id; Character = character; Level = level; Text = text; Detail = detail; CanBuy = canBuy; }
    }
}
