using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Progression
{
    public sealed class PlayerSetRuntime : IDisposable
    {
        private readonly Dictionary<ContentId, SetDefinition> _catalog = new Dictionary<ContentId, SetDefinition>();
        private readonly Dictionary<ContentId, ISetExtraAbility> _abilities = new Dictionary<ContentId, ISetExtraAbility>();
        private readonly ISetExtraAbilityFactory _factory;

        public int Count => _abilities.Count;

        public PlayerSetRuntime(IEnumerable<SetDefinition> definitions, ISetExtraAbilityFactory factory)
        {
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions));

            foreach (var definition in definitions)
            {
                if (definition == null)
                    throw new ArgumentException("Set catalog cannot contain null definitions.", nameof(definitions));
                if (!_catalog.TryAdd(definition.Id, definition))
                    throw new ArgumentException($"Duplicate set id '{definition.Id}'.", nameof(definitions));
            }

            if (_catalog.Count > 0)
                _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public bool TryGetAbility(ContentId id, out ISetExtraAbility ability)
        {
            return _abilities.TryGetValue(id, out ability);
        }

        public void Synchronize(PlayerBuild build)
        {
            if (build == null)
                throw new ArgumentNullException(nameof(build));

            foreach (var entry in build.Entries)
            {
                if (entry.Definition.Kind != BuildEntryKind.Set || _abilities.ContainsKey(entry.Definition.Id))
                    continue;
                if (!_catalog.TryGetValue(entry.Definition.Id, out var definition))
                    throw new InvalidOperationException($"Acquired set '{entry.Definition.Id}' is missing from the runtime catalog.");

                var ability = _factory.Create(definition);
                if (ability == null)
                    throw new InvalidOperationException($"Extra ability factory returned null for set '{definition.Id}'.");
                _abilities.Add(definition.Id, ability);
            }
        }

        public void Tick(float deltaTime, bool isRunning)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            foreach (var ability in _abilities.Values)
                ability.Tick(deltaTime, isRunning);
        }

        public void Dispose()
        {
            foreach (var ability in _abilities.Values)
                ability.Dispose();
            _abilities.Clear();
        }
    }
}
