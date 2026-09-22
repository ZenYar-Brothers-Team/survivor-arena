using System;
using Game.Content;
using Game.Content.Json;
using Game.Field.Json;
using Newtonsoft.Json;

namespace Game.Field
{
    /// <summary>Editor-baked fixture geometry. Screen height and wall thickness are world units; screen count is dimensionless.</summary>
    public sealed class FixtureArenaGeometryCatalog
    {
        public float SideScreenHeights { get; }
        public float ReferenceScreenHeight { get; }
        public float WallThickness { get; }
        public float SideLength => SideScreenHeights * ReferenceScreenHeight;
        private FixtureArenaGeometryCatalog(ArenaGeometryData data)
        {
            SideScreenHeights = data.SideScreenHeights ?? throw new ArgumentException("sideScreenHeights is required.");
            ReferenceScreenHeight = data.ReferenceScreenHeight ?? throw new ArgumentException("referenceScreenHeight is required.");
            WallThickness = data.WallThickness ?? throw new ArgumentException("wallThickness is required.");
            NumericValidation.ValidatePositive(SideScreenHeights, nameof(SideScreenHeights));
            NumericValidation.ValidatePositive(ReferenceScreenHeight, nameof(ReferenceScreenHeight));
            NumericValidation.ValidatePositive(WallThickness, nameof(WallThickness));
            NumericValidation.ValidatePositive(SideLength, nameof(SideLength));
        }
        public static FixtureArenaGeometryCatalog Create() => FromJson(JsonContentFile.ReadText("Content/Fields/FixtureArenaGeometry"));
        public static FixtureArenaGeometryCatalog FromJson(string json) => new FixtureArenaGeometryCatalog(
            JsonConvert.DeserializeObject<ArenaGeometryData>(json, JsonContentFile.Settings)
            ?? throw new ArgumentException("Arena geometry is required."));
    }
}
