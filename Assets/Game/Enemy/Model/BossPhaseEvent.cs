using Game.Combat;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>Producer-owned immutable phase attribution, independent of DEV/telemetry consumers.</summary>
    public readonly struct BossPhaseEvent
    {
        public CombatIdentity Identity { get; }
        public ContentId PreviousPhase { get; }
        public ContentId CurrentPhase { get; }
        public ContentId AttackId { get; }
        public BossPhaseEvent(CombatIdentity identity, ContentId previousPhase, ContentId currentPhase, ContentId attackId)
        { Identity = identity; PreviousPhase = previousPhase; CurrentPhase = currentPhase; AttackId = attackId; }
    }
}
