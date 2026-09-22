using System.Collections.Generic;

namespace Game.Progression.Tests
{
    public sealed class RecordingSetOfferProvider : ISetDraftOfferProvider
    {
        public int Calls { get; private set; }
        public List<string> LastOrder { get; } = new List<string>();
        public bool RejectAfterFirstCall { get; set; }

        public IReadOnlyList<DraftOption> SelectPriorityOffers(
            IReadOnlyList<DraftOption> eligibleSets, int slotCount, IDraftRandom random)
        {
            Calls++;
            LastOrder.Clear();
            var result = new List<DraftOption>();
            foreach (var option in eligibleSets)
            {
                LastOrder.Add(option.Definition.Id.ToString());
                if ((!RejectAfterFirstCall || Calls == 1) && result.Count < slotCount)
                    result.Add(option);
            }
            return result;
        }
    }
}
