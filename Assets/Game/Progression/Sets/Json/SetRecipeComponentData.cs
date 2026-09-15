namespace Game.Progression.Json
{
    public sealed class SetRecipeComponentData
    {
        public string Id { get; set; }
        public BuildEntryKind Kind { get; set; }
        public int MinimumLevel { get; set; }
    }
}
