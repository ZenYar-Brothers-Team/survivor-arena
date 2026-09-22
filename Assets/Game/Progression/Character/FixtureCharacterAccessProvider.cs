using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Progression
{
    /// <summary>Explicit immutable profile fixture; no persistence or production unlock policy.</summary>
    public sealed class FixtureCharacterAccessProvider : ICharacterAccessProvider
    {
        private readonly HashSet<ContentId> _unlocked;
        private readonly IReadOnlyDictionary<ContentId, string> _reasons;
        public FixtureCharacterAccessProvider(IEnumerable<ContentId> unlocked,
            IReadOnlyDictionary<ContentId, string> reasons = null)
        {
            _unlocked = new HashSet<ContentId>(unlocked ?? throw new ArgumentNullException(nameof(unlocked)));
            var copy = new Dictionary<ContentId, string>();
            if (reasons != null)
                foreach (var pair in reasons)
                {
                    if (string.IsNullOrWhiteSpace(pair.Value)) throw new ArgumentException("A lock reason cannot be blank.");
                    copy.Add(pair.Key, pair.Value);
                }
            _reasons = copy;
        }
        public string GetLockReason(ContentId id) => _unlocked.Contains(id) ? null :
            _reasons.TryGetValue(id, out var reason) ? reason : "Unavailable in this fixture profile.";
    }
}
