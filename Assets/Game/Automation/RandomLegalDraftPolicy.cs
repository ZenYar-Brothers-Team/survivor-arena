using System;
using System.Collections.Generic;

namespace Game.Automation
{
    /// <summary>Uniform choice among only the currently offered IDs; does not own gameplay draft RNG.</summary>
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
    }
}
