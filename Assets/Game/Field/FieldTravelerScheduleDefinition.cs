using System;
using Game.Content;

namespace Game.Field
{
    /// <summary>Field-owned extension point; Game.Traveler supplies the validated payload without a reverse assembly dependency.</summary>
    public class FieldTravelerScheduleDefinition : IContentDefinition
    {
        public ContentId Id { get; }
        public FieldTravelerScheduleDefinition(ContentId id)
        {
            if (!id.IsValid) throw new ArgumentException("Schedule id is required.", nameof(id));
            Id = id;
        }
    }
}
