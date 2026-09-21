namespace Game.Progression.Json
{
    public sealed class SetDefinitionData
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public SetEffectData[] Effects { get; set; }
        public SetRecipeComponentData[] Recipe { get; set; }
    }
}
