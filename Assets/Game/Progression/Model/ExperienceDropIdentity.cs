using System;
using Game.Content;

namespace Game.Progression
{
    public readonly struct ExperienceDropIdentity
    {
        public Guid LifeId { get; }
        public Guid RunId { get; }
        public Guid? SourceLifeId { get; }
        public ContentId? SourceContentId { get; }

        public ExperienceDropIdentity(Guid lifeId, Guid runId, Guid? sourceLifeId, ContentId? sourceContentId)
        {
            LifeId = lifeId;
            RunId = runId;
            SourceLifeId = sourceLifeId;
            SourceContentId = sourceContentId;
        }
    }
}
