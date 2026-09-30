namespace Game.Traveler
{
    /// <summary>A Traveler the development panel can launch on demand: content id, display name and role.</summary>
    public readonly struct TravelerChoice
    {
        public string Id { get; }
        public string Name { get; }
        public string Role { get; }

        public TravelerChoice(string id, string name, string role)
        {
            Id = id; Name = name; Role = role;
        }
    }
}
