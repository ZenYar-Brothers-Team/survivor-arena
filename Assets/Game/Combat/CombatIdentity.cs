using System;
using Game.Content;

namespace Game.Combat
{
    /// <summary>Value identity captured before callbacks; missing content/run identity stays explicitly unavailable.</summary>
    public readonly struct CombatIdentity
    {
        public Guid LifeId { get; }
        public Guid? RunId { get; }
        public ContentId? ContentId { get; }
        public CombatEntityCategory Category { get; }
        public CombatIdentity(Guid lifeId, Guid? runId, ContentId? contentId, CombatEntityCategory category)
        {
            LifeId = lifeId;
            RunId = runId;
            ContentId = contentId;
            Category = category;
        }
    }
}
