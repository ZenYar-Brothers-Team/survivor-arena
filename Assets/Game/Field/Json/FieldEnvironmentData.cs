namespace Game.Field.Json
{
    public sealed class FieldEnvironmentData
    {
        public string Id { get; set; }
        public string SceneName { get; set; }
        public string SpawnPointName { get; set; }
        public string[] ObstacleNames { get; set; }
    }
}
