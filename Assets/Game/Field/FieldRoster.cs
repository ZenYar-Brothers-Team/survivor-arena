using System;
using System.Collections.Generic;
using Game.Content;
namespace Game.Field
{
    public sealed class FieldRoster
    {
        private readonly Dictionary<ContentId, FieldDefinition> _byId = new Dictionary<ContentId, FieldDefinition>();
        private readonly IFieldAccessProvider _access;
        public IReadOnlyList<FieldDefinition> AllFields { get; }
        public FieldRoster(IEnumerable<FieldDefinition> fields, IFieldAccessProvider access)
        {
            _access = access ?? throw new ArgumentNullException(nameof(access));
            var copy = new List<FieldDefinition>(fields ?? throw new ArgumentNullException(nameof(fields)));
            if (copy.Count == 0) throw new ArgumentException("Field roster cannot be empty.");
            foreach (var field in copy)
            {
                if (field == null || !_byId.TryAdd(field.Id, field)) throw new ArgumentException("Invalid or duplicate field.");
                GetLockReason(field.Id);
            }
            AllFields = copy.AsReadOnly();
        }
        public string GetLockReason(ContentId id)
        {
            if (!_byId.ContainsKey(id)) throw new ArgumentException($"Unknown field '{id}'.");
            var reason = _access.GetLockReason(id);
            if (reason != null && string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("Locked field requires a reason.");
            return reason;
        }
        public bool TrySelect(ContentId id, out FieldDefinition field)
        {
            if (_byId.TryGetValue(id, out field) && GetLockReason(id) == null) return true;
            field = null;
            return false;
        }
    }
}
