namespace Game.Enemy.Json
{
    public sealed class RaidProfileData
    {
        public string Id { get; set; }
        public float? TriggerRadiusScreenWidths { get; set; }
        public int? TriggerEnemyCount { get; set; }
        public float? CountCheckSeconds { get; set; }
        public float? MinIntervalSeconds { get; set; }
        public float? MaxIntervalSeconds { get; set; }
        public float? InnerExemptRadius { get; set; }
        public float? SlotSpacing { get; set; }
        public float? RetargetSeconds { get; set; }
        public int? MaxParticipants { get; set; }
        public float? SlotJitter { get; set; }
        public float? FormationSpeedBonus { get; set; }
        public float? ArrivalSlowDistance { get; set; }
        public RaidRingData Ring { get; set; }
        public RaidWallData Wall { get; set; }
        public RaidContractionData Contraction { get; set; }
        public RaidPincerData Pincer { get; set; }
    }
}
