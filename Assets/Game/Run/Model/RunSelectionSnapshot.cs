using System;
using Game.Content;
namespace Game.Run
{
    /// <summary>Release-safe run identity, independent of catalogs and optional telemetry.</summary>
    public sealed class RunSelectionSnapshot
    {
        public ContentId CharacterId { get; }
        public ContentId FieldId { get; }
        public ContentId EnvironmentId { get; }
        public ContentId TimelineId { get; }
        public RunSelectionSnapshot(ContentId characterId, ContentId fieldId, ContentId environmentId, ContentId timelineId)
        {
            if (!characterId.IsValid || !fieldId.IsValid || !environmentId.IsValid || !timelineId.IsValid)
                throw new ArgumentException("Run selection requires valid character, field, environment and timeline ids.");
            CharacterId = characterId; FieldId = fieldId; EnvironmentId = environmentId; TimelineId = timelineId;
        }
    }
}
