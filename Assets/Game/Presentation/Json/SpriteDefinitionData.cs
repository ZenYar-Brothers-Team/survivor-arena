namespace Game.Presentation.Json
{
    public sealed class SpriteDefinitionData
    {
        public string Id { get; set; }
        public string ResourcePath { get; set; }
        public SpriteRole? Role { get; set; }
        public float? ContactRadius { get; set; }
        public float? ContactCenterY { get; set; }
        public ProjectilePresentationProfileData Projectile { get; set; }
    }
}
