using System;
using System.Collections.Generic;

namespace Game.Automation
{
    /// <summary>Uniform choice among offered IDs, optionally restricted to an offered preferred subset.</summary>
    public sealed class RandomLegalDraftPolicy
    {
        private readonly Random _random;
        public RandomLegalDraftPolicy(Random random) => _random = random ?? throw new ArgumentNullException(nameof(random));

        public string Choose(IReadOnlyList<string> offeredIds)
        {
            if (offeredIds == null) throw new ArgumentNullException(nameof(offeredIds));
            if (offeredIds.Count == 0) return null;
            return offeredIds[_random.Next(offeredIds.Count)];
        }

        public string ChoosePreferred(IReadOnlyList<string> offeredIds, IReadOnlyList<string> preferredIds)
        {
            if (preferredIds == null) throw new ArgumentNullException(nameof(preferredIds));
            if (preferredIds.Count == 0) return Choose(offeredIds);
            if (offeredIds == null) throw new ArgumentNullException(nameof(offeredIds));
            var eligible = new List<string>(preferredIds.Count);
            foreach (var id in preferredIds)
                if (Contains(offeredIds, id)) eligible.Add(id);
            return eligible.Count > 0 ? eligible[_random.Next(eligible.Count)] : Choose(offeredIds);
        }

        public string ChooseActiveFirst15(IReadOnlyList<string> offeredIds, IReadOnlyList<string> activeIds, int playerLevel) =>
            playerLevel <= 15 ? ChoosePreferred(offeredIds, activeIds) : Choose(offeredIds);

        private static bool Contains(IReadOnlyList<string> ids, string value)
        {
            foreach (var id in ids)
                if (string.Equals(id, value, StringComparison.Ordinal)) return true;
            return false;
        }
    }
}
