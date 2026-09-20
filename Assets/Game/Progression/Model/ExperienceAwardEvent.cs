using System;

namespace Game.Progression
{
    /// <summary>One consumed drop (or explicit intervention); zero-recovery expiry is still observable.</summary>
    public readonly struct ExperienceAwardEvent
    {
        public Guid RunId { get; }
        public ExperienceDropIdentity? Drop { get; }
        public ExperienceEventKind Kind { get; }
        public float BaseAmount { get; }
        public float AwardedAmount { get; }

        public ExperienceAwardEvent(Guid runId, ExperienceDropIdentity? drop, ExperienceEventKind kind,
            float baseAmount, float awardedAmount)
        {
            RunId = runId;
            Drop = drop;
            Kind = kind;
            BaseAmount = baseAmount;
            AwardedAmount = awardedAmount;
        }
    }
}
