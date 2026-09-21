using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Content.Json;
using Game.Field.Json;
using Newtonsoft.Json;
namespace Game.Field
{
    public sealed class FixtureFieldCatalog
    {
        public ContentId DefaultFieldId { get; }
        public FieldRoster Roster { get; }
        public IReadOnlyList<FieldEnvironmentDefinition> Environments { get; }
        private FixtureFieldCatalog(FieldCatalogData data)
        {
            if (data.Fields == null || data.Environments == null || data.AvailableFieldIds == null)
                throw new InvalidOperationException("Field catalog requires fields, environments and availableFieldIds.");
            var fields = data.Fields.Select(field => new FieldDefinition(field.Id, field.DisplayName, field.Description,
                field.ThumbnailPlaceholder, field.Difficulty ?? throw new InvalidOperationException($"Field '{field.Id}' requires difficulty."),
                field.UnlockDescription, field.EnvironmentId, field.TimelineId, field.FinalBossId,
                (field.EnemyIds ?? throw new InvalidOperationException("Field requires enemyIds.")).Select(id => new ContentId(id)),
                field.MidBossId == null ? (ContentId?)null : new ContentId(field.MidBossId),
                field.TravelerScheduleId == null ? (ContentId?)null : new ContentId(field.TravelerScheduleId))).ToList();
            Roster = new FieldRoster(fields, new FixtureFieldAccessProvider(fields, data.AvailableFieldIds.Select(id => new ContentId(id))));
            DefaultFieldId = data.DefaultFieldId;
            if (!Roster.TrySelect(DefaultFieldId, out _)) throw new InvalidOperationException("Default field must be available.");
            Environments = data.Environments.Select(environment => new FieldEnvironmentDefinition(environment.Id,
                environment.SceneName, environment.SpawnPointName, environment.ObstacleNames)).ToList().AsReadOnly();
        }
        public static FixtureFieldCatalog Create() => FromJson(JsonContentFile.ReadText("Content/Fields/FixtureFields"));
        public static FixtureFieldCatalog FromJson(string json) => new FixtureFieldCatalog(
            JsonConvert.DeserializeObject<FieldCatalogData>(json, JsonContentFile.Settings)
            ?? throw new InvalidOperationException("Field catalog is required."));
    }
}
