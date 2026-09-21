using System;
using Game.Content;

namespace Game.Field
{
    /// <summary>Optional field-owned extension point. IP-29 supplies a consumer; no encounter/timing semantics are invented here.</summary>
    public sealed class FieldTravelerScheduleDefinition : IContentDefinition
    {
        public ContentId Id { get; }
        public FieldTravelerScheduleDefinition(ContentId id)
        {
            if (!id.IsValid) throw new ArgumentException("Schedule id is required.", nameof(id));
            Id = id;
        }
    }
}
