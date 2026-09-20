using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Progression
{
    public sealed class CharacterRoster
    {
        private readonly Dictionary<ContentId, CharacterDefinition> _definitions;
        private readonly List<CharacterDefinition> _unlocked;
        private readonly HashSet<ContentId> _unlockedIds;

        public IReadOnlyList<CharacterDefinition> AllCharacters { get; }
        public IReadOnlyList<CharacterDefinition> UnlockedCharacters => _unlocked;

        public CharacterRoster(
            IReadOnlyList<CharacterDefinition> definitions,
            IEnumerable<ContentId> unlockedIds)
        {
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions));
            if (unlockedIds == null)
                throw new ArgumentNullException(nameof(unlockedIds));
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

            _unlockedIds = new HashSet<ContentId>();
            _unlocked = new List<CharacterDefinition>();
            foreach (var id in unlockedIds)
            {
                if (!_definitions.TryGetValue(id, out var definition))
                    throw new ArgumentException($"Unlocked character id '{id}' is not in the roster.", nameof(unlockedIds));
                if (_unlockedIds.Add(id))
                    _unlocked.Add(definition);
            }
            if (_unlocked.Count == 0)
                throw new ArgumentException("Character roster requires at least one unlocked character.", nameof(unlockedIds));

            AllCharacters = all;
        }

        public bool TrySelect(ContentId id, out CharacterDefinition character)
        {
            if (_unlockedIds.Contains(id) && _definitions.TryGetValue(id, out character))
                return true;

            character = null;
            return false;
        }
    }
}
