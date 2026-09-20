using System.Collections.Generic;

namespace Game.Progression
{
    /// <summary>Legacy fixture adapter; IP-11 replaces per-set chances with global policy.</summary>
    public sealed class FixtureSetDraftOfferProvider : ISetDraftOfferProvider
    {
        public IReadOnlyList<DraftOption> SelectPriorityOffers(
            IReadOnlyList<DraftOption> eligibleSets, int slotCount, IDraftRandom random)
        {
            var result = new List<DraftOption>();
            foreach (var option in eligibleSets)
            {
                var chance = option.Definition.DraftChance;
                var passed = chance >= 1f || (chance > 0f && random.NextFloat01() < chance);
                if (passed && result.Count < slotCount) result.Add(option);
            }
            return result;
        }
    }
}
