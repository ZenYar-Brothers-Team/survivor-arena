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
        public int Cap { get; }
        public long Price { get; }
        public string Bonus { get; }
        public string NextBonus { get; }
        public string Group { get; }
        public UnityEngine.Sprite Icon { get; }
        public bool HiddenCharacter { get; }
        public string Kind { get; }
        public bool Owned { get; }
        public bool HiddenField { get; }
        public MetaCardViewState(string id, string character, int level, string text, string detail, bool canBuy,
            int cap = 0, long price = 0, string bonus = null, string nextBonus = null, string group = null,
            UnityEngine.Sprite icon = null, bool hiddenCharacter = false, string kind = null, bool owned = false,
            bool hiddenField = false)
        { Id = id; Character = character; Level = level; Text = text; Detail = detail; CanBuy = canBuy;
            Cap = cap; Price = price; Bonus = bonus; NextBonus = nextBonus; Group = group; Icon = icon; HiddenCharacter = hiddenCharacter;
            Kind = kind; Owned = owned; HiddenField = hiddenField; }
    }
}
