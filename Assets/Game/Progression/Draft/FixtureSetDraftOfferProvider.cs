using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Progression
{
    /// <summary>GDD Sets: one global probability, independent checks in ordinal ID order.</summary>
    public sealed class FixtureSetDraftOfferProvider : ISetDraftOfferProvider
    {
        public float SetDraftChance { get; }
        public FixtureSetDraftOfferProvider(float setDraftChance)
        {
            NumericValidation.ValidateRange(setDraftChance, 0f, 1f, nameof(setDraftChance));
            SetDraftChance = setDraftChance;
        }
        public IReadOnlyList<DraftOption> SelectPriorityOffers(
            IReadOnlyList<DraftOption> eligibleSets, int slotCount, IDraftRandom random)
        {
            if (eligibleSets == null) throw new ArgumentNullException(nameof(eligibleSets));
            if (random == null) throw new ArgumentNullException(nameof(random));
            var ordered = new List<DraftOption>(eligibleSets);
            ordered.Sort((a, b) => string.CompareOrdinal(a.Definition.Id.ToString(), b.Definition.Id.ToString()));
            var result = new List<DraftOption>();
            foreach (var option in ordered)
                if (SetDraftChance >= 1f || (SetDraftChance > 0f && random.NextFloat01() < SetDraftChance))
                    result.Add(option);
            return result.AsReadOnly(); // All successes, including overflow, belong to the request snapshot.
        }
    }
}
