namespace Game.Presentation.Json
{
    public sealed class FieldEnvironmentPresentationData
    {
        public string Id { get; set; }
        public string EnvironmentId { get; set; }
        public string GroundVisualId { get; set; }
        public string FenceVisualId { get; set; }
        public string ObstacleVisualId { get; set; }
        public string BushVisualId { get; set; }
        public string GrassVisualId { get; set; }
        public string ColumnVisualId { get; set; }
        public string BarrelVisualId { get; set; }
        public string RockVisualId { get; set; }
        public string ShrineVisualId { get; set; }
        public float? ShrineChance { get; set; }
        public string ObstacleName { get; set; }
        public float? ObstacleScale { get; set; }
        public float? DecorationSpacing { get; set; }
        public float? DecorationJitter { get; set; }
        public float? DecorationChance { get; set; }
        public float? BushChance { get; set; }
        public float? DecorationMargin { get; set; }
        public float? SafeRadius { get; set; }
        public float? GrassScaleMin { get; set; }
        public float? GrassScaleMax { get; set; }
        public float? BushScaleMin { get; set; }
        public float? BushScaleMax { get; set; }
        public int? Seed { get; set; }
        public int? ObstacleSeed { get; set; }
        public int? InteriorObstacleCount { get; set; }
        public int? NearObstacleCount { get; set; }
        public int? ObstaclePlacementAttempts { get; set; }
        public float? NearObstacleRadius { get; set; }
        public float? FenceChance { get; set; }
        public float? ObstacleSeparation { get; set; }
        public float? FenceColliderWidth { get; set; }
        public float? FenceColliderHeight { get; set; }
        public float? StumpColliderRadius { get; set; }
        /// <summary>Optional authored obstacles; when present they replace seeded random placement (FIELD-001).</summary>
        public FieldObstacleData[] Obstacles { get; set; }
        /// <summary>Optional per-run pattern layout (DECISION-0068); excludes authored obstacles.</summary>
        public FieldObstacleLayoutData ObstacleLayout { get; set; }
        /// <summary>Optional per-run blob layout (field geometry study); excludes authored obstacles and the pattern layout.</summary>
        public FieldBlobLayoutData BlobLayout { get; set; }
        /// <summary>Optional arena side in world units; replaces the shared fixture arena for this field.</summary>
        public float? ArenaSideLength { get; set; }
    }
}
