using System.Collections.Generic;

namespace Game.Progression
{
    /// <summary>
    /// Select successful set checks in policy order from already eligible, unbanished sets.
    /// Return up to slotCount distinct inputs. DraftPool owns ordinary fill and uniform
    /// backfill from remaining eligible sets (DECISION-0019); chance/order policy belongs to IP-11.
    /// </summary>
    public interface ISetDraftOfferProvider
    {
        IReadOnlyList<DraftOption> SelectPriorityOffers(
            IReadOnlyList<DraftOption> eligibleSets, int slotCount, IDraftRandom random);
    }
}
