using System;
using System.Collections.Generic;

namespace Game.Content
{
    public sealed class ContentRegistry
    {
        private readonly Dictionary<ContentId, IContentDefinition> _definitions = new Dictionary<ContentId, IContentDefinition>();

        public bool IsBuilt { get; private set; }

        public static ContentRegistry BuildFrom(IEnumerable<IContentDefinition> definitions)
        {
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions));

            var registry = new ContentRegistry();

            foreach (var definition in definitions)
                registry.Register(definition);

            registry.Build();
            return registry;
        }

        public void Register(IContentDefinition definition)
        {
            if (IsBuilt)
                throw new InvalidOperationException("Content registry is already built and cannot be modified.");

            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            if (!definition.Id.IsValid)
                throw new ContentValidationException(new[] { "Content definition has an invalid id." });

            if (_definitions.ContainsKey(definition.Id))
                throw new DuplicateContentIdException(definition.Id);

            _definitions.Add(definition.Id, definition);
        }

        public T Get<T>(ContentId id) where T : class, IContentDefinition
        {
            EnsureBuilt();

            if (TryGet<T>(id, out var definition))
                return definition;

            throw new ContentNotFoundException(id);
        }

        public bool TryGet<T>(ContentId id, out T definition) where T : class, IContentDefinition
        {
            EnsureBuilt();

            if (_definitions.TryGetValue(id, out var raw) && raw is T typed)
            {
                definition = typed;
                return true;
            }

            definition = null;
            return false;
        }

        public void Build()
        {
            if (IsBuilt)
                return;

            var errors = new List<string>();

            foreach (var definition in _definitions.Values)
            {
                if (definition is not IReferencesContent referencing)
                    continue;

                var references = referencing.GetReferencedContent();
                if (references == null)
                {
                    errors.Add($"'{definition.Id}' returned a null content reference collection.");
                    continue;
                }

                foreach (var reference in references)
                {
                    if (!reference.Id.IsValid)
                    {
                        errors.Add($"'{definition.Id}' contains a reference with an invalid content id.");
                        continue;
                    }

                    if (reference.ExpectedType == null)
                    {
                        errors.Add($"'{definition.Id}' references '{reference.Id}' without an expected content type.");
                        continue;
                    }

                    if (!_definitions.TryGetValue(reference.Id, out var target))
                    {
                        errors.Add($"'{definition.Id}' references missing content id '{reference.Id}'.");
                        continue;
                    }

                    if (!reference.ExpectedType.IsInstanceOfType(target))
                    {
                        errors.Add(
                            $"'{definition.Id}' references '{reference.Id}' expecting type '{reference.ExpectedType.Name}' " +
                            $"but found '{target.GetType().Name}'.");
                    }
                }
            }

            if (errors.Count > 0)
                throw new ContentValidationException(errors);

            IsBuilt = true;
        }

        private void EnsureBuilt()
        {
            if (!IsBuilt)
                throw new InvalidOperationException("Content registry must be built before definitions can be resolved.");
        }
    }
}
