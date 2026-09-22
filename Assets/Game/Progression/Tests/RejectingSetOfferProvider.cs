using System;
using System.Collections.Generic;

namespace Game.Progression.Tests
{
    public sealed class RejectingSetOfferProvider : ISetDraftOfferProvider
    {
        public IReadOnlyList<DraftOption> SelectPriorityOffers(
            IReadOnlyList<DraftOption> eligibleSets, int slotCount, IDraftRandom random) => Array.Empty<DraftOption>();
    }
}
