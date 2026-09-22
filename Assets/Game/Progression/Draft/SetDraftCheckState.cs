using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Progression
{
    /// <summary>DECISION-0022: one check snapshot per request/reroll, reused by banish rebuilds.</summary>
    public sealed class SetDraftCheckState
    {
        private HashSet<ContentId> _passed;

        public IReadOnlyList<DraftOption> GetPriorityOffers(IReadOnlyList<DraftOption> eligible,
            ISetDraftOfferProvider provider, IDraftRandom random)
        {
            var ordered = new List<DraftOption>(eligible);
            ordered.Sort((a, b) => string.CompareOrdinal(a.Definition.Id.ToString(), b.Definition.Id.ToString()));
            if (_passed == null)
            {
                // Check all eligible sets, including successes beyond the visible slots.
                var checks = provider.SelectPriorityOffers(ordered.AsReadOnly(), ordered.Count, random)
                    ?? throw new InvalidOperationException("Set provider returned null.");
                var passed = new HashSet<ContentId>();
                foreach (var option in checks)
                {
                    if (!ordered.Exists(input => ReferenceEquals(input.Definition, option.Definition)) ||
                        !passed.Add(option.Definition.Id))
                        throw new InvalidOperationException("Set provider must return distinct eligible inputs.");
                }
                _passed = passed;
            }
            ordered.RemoveAll(option => !_passed.Contains(option.Definition.Id));
            return ordered.AsReadOnly();
        }
    }
}
