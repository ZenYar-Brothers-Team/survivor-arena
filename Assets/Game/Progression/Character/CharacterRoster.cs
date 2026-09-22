using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Progression
{
    public sealed class CharacterRoster
    {
        private readonly Dictionary<ContentId, CharacterDefinition> _definitions;
        private readonly ICharacterAccessProvider _access;

        public IReadOnlyList<CharacterDefinition> AllCharacters { get; }
        public IReadOnlyList<CharacterDefinition> UnlockedCharacters
        {
            get
            {
                var unlocked = new List<CharacterDefinition>();
                foreach (var character in AllCharacters)
                    if (GetLockReason(character.Id) == null) unlocked.Add(character);
                return unlocked.AsReadOnly();
            }
        }

        public CharacterRoster(IReadOnlyList<CharacterDefinition> definitions, IEnumerable<ContentId> unlockedIds)
            : this(definitions, new FixtureCharacterAccessProvider(unlockedIds))
        {
            foreach (var id in unlockedIds)
                if (!_definitions.ContainsKey(id)) throw new ArgumentException($"Unknown unlocked character '{id}'.", nameof(unlockedIds));
        }

        public CharacterRoster(IReadOnlyList<CharacterDefinition> definitions, ICharacterAccessProvider access)
        {
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            _access = access ?? throw new ArgumentNullException(nameof(access));
            if (definitions.Count == 0)
                throw new ArgumentException("Character roster cannot be empty.", nameof(definitions));

            _definitions = new Dictionary<ContentId, CharacterDefinition>();
            var all = new CharacterDefinition[definitions.Count];
            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i] ??
                    throw new ArgumentException("Character roster cannot contain null definitions.", nameof(definitions));
                if (!_definitions.TryAdd(definition.Id, definition))
                    throw new ArgumentException($"Duplicate character id '{definition.Id}'.", nameof(definitions));
                all[i] = definition;
            }

            AllCharacters = Array.AsReadOnly(all);
            foreach (var character in AllCharacters) GetLockReason(character.Id);
        }

        public string GetLockReason(ContentId id)
        {
            if (!_definitions.ContainsKey(id)) throw new ArgumentException($"Unknown character '{id}'.", nameof(id));
            var reason = _access.GetLockReason(id);
            if (reason != null && string.IsNullOrWhiteSpace(reason))
                throw new InvalidOperationException($"Locked character '{id}' requires a reason.");
            return reason;
        }

        public bool TrySelect(ContentId id, out CharacterDefinition character)
        {
            if (_definitions.TryGetValue(id, out character) && GetLockReason(id) == null)
                return true;

            character = null;
            return false;
        }
    }
}
