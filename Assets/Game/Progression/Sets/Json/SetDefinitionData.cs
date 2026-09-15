namespace Game.Progression.Json
{
    public sealed class SetDefinitionData
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public float DraftChance { get; set; }
        public SetRecipeComponentData[] Recipe { get; set; }
    }
}
