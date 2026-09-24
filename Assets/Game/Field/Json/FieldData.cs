namespace Game.Field.Json
{
    public sealed class FieldData
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string ThumbnailPlaceholder { get; set; }
        public string ThumbnailVisualId { get; set; }
        public int? Difficulty { get; set; }
        public string UnlockDescription { get; set; }
        public string EnvironmentId { get; set; }
        public string TimelineId { get; set; }
        public string FinalBossId { get; set; }
        public string MidBossId { get; set; }
        public string TravelerScheduleId { get; set; }
        public string[] EnemyIds { get; set; }
    }
}
