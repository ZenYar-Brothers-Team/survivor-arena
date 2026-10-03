namespace Game.UI
{
    /// <summary>One screen event offered by the development panel (DECISION-0157).</summary>
    public sealed class ScreenEventDevelopmentEntry
    {
        public string Id { get; }
        public string Name { get; }
        public bool Rare { get; }

        public ScreenEventDevelopmentEntry(string id, string name, bool rare)
        {
            Id = id;
            Name = name;
            Rare = rare;
        }
    }
}
