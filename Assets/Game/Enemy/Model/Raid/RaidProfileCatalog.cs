using System;
using Game.Content.Json;
using Game.Enemy.Json;

namespace Game.Enemy
{
    public static class RaidProfileCatalog
    {
        public const string ResourcePath = "Content/Waves/ProductionRaidProfile";

        public static RaidProfile Load() => FromJson(JsonContentFile.ReadText(ResourcePath));

        public static RaidProfile FromJson(string json)
        {
            var d = Newtonsoft.Json.JsonConvert.DeserializeObject<RaidProfileData>(json, JsonContentFile.Settings)
                ?? throw new InvalidOperationException("Raid profile is required.");
            var ring = d.Ring ?? throw Missing("ring");
            var wall = d.Wall ?? throw Missing("wall");
            var contraction = d.Contraction ?? throw Missing("contraction");
            var pincer = d.Pincer ?? throw Missing("pincer");
            return new RaidProfile(
                d.Id,
                d.TriggerRadiusScreenWidths ?? throw Missing("triggerRadiusScreenWidths"),
                d.TriggerEnemyCount ?? throw Missing("triggerEnemyCount"),
                d.CountCheckSeconds ?? throw Missing("countCheckSeconds"),
                d.MinIntervalSeconds ?? throw Missing("minIntervalSeconds"),
                d.MaxIntervalSeconds ?? throw Missing("maxIntervalSeconds"),
                d.InnerExemptRadius ?? throw Missing("innerExemptRadius"),
                d.SlotSpacing ?? throw Missing("slotSpacing"),
                d.RetargetSeconds ?? throw Missing("retargetSeconds"),
                d.MaxParticipants ?? throw Missing("maxParticipants"),
                d.SlotJitter ?? throw Missing("slotJitter"),
                d.FormationSpeedBonus ?? throw Missing("formationSpeedBonus"),
                d.ArrivalSlowDistance ?? throw Missing("arrivalSlowDistance"),
                new RaidRingProfile(
                    ring.DurationSeconds ?? throw Missing("ring.durationSeconds"),
                    ring.RadiusScreenWidths ?? throw Missing("ring.radiusScreenWidths")),
                new RaidWallProfile(
                    wall.DurationSeconds ?? throw Missing("wall.durationSeconds"),
                    wall.SampleSeconds ?? throw Missing("wall.sampleSeconds"),
                    wall.DistanceScreenWidths ?? throw Missing("wall.distanceScreenWidths"),
                    wall.WidthScreenWidths ?? throw Missing("wall.widthScreenWidths")),
                new RaidContractionProfile(
                    contraction.DurationSeconds ?? throw Missing("contraction.durationSeconds"),
                    contraction.StartRadiusScreenWidths ?? throw Missing("contraction.startRadiusScreenWidths"),
                    contraction.EndRadiusScreenWidths ?? throw Missing("contraction.endRadiusScreenWidths")),
                new RaidPincerProfile(
                    pincer.DurationSeconds ?? throw Missing("pincer.durationSeconds"),
                    pincer.LengthScreenWidths ?? throw Missing("pincer.lengthScreenWidths"),
                    pincer.StartGapScreenWidths ?? throw Missing("pincer.startGapScreenWidths"),
                    pincer.EndGapScreenWidths ?? throw Missing("pincer.endGapScreenWidths")));
        }

        private static InvalidOperationException Missing(string field) =>
            new InvalidOperationException($"Raid profile requires {field}.");
    }
}
