using System;
using System.Collections.Generic;
using Game.Content;
namespace Game.Field
{
    public sealed class FixtureFieldAccessProvider : IFieldAccessProvider
    {
        private readonly HashSet<ContentId> _available;
        private readonly Dictionary<ContentId, string> _reasons = new Dictionary<ContentId, string>();
        public FixtureFieldAccessProvider(IEnumerable<FieldDefinition> fields, IEnumerable<ContentId> available)
        {
            _available = new HashSet<ContentId>(available ?? throw new ArgumentNullException(nameof(available)));
            foreach (var field in fields) _reasons.Add(field.Id, field.UnlockDescription);
            foreach (var id in _available)
                if (!_reasons.ContainsKey(id)) throw new ArgumentException($"Unknown available field '{id}'.");
        }
        public string GetLockReason(ContentId id) => _available.Contains(id) ? null : _reasons[id];
    }
}
