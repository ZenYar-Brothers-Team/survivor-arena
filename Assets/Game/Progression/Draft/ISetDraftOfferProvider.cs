using System.Collections.Generic;

namespace Game.Progression
{
    /// <summary>
    /// Select successful set checks in policy order from already eligible, unbanished sets.
    /// Return all successful distinct inputs; the check snapshot passes eligibleSets.Count as slotCount. DraftPool owns ordinary fill and uniform
    /// backfill from remaining eligible sets (DECISION-0019); DECISION-0022 orders inputs by ordinal ID and preserves all results across banish. Global chance belongs to IP-11.
    /// </summary>
    public interface ISetDraftOfferProvider
    {
        IReadOnlyList<DraftOption> SelectPriorityOffers(
            IReadOnlyList<DraftOption> eligibleSets, int slotCount, IDraftRandom random);
    }
}
