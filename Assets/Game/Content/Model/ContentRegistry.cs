using System.Collections.Generic;

namespace Game.Content
{
    public sealed class ContentRegistry
    {
        private readonly Dictionary<ContentId, IContentDefinition> _definitions = new Dictionary<ContentId, IContentDefinition>();

        public void Register(IContentDefinition definition)
        {
            if (_definitions.ContainsKey(definition.Id))
                throw new DuplicateContentIdException(definition.Id);

            _definitions.Add(definition.Id, definition);
        }

        public T Get<T>(ContentId id) where T : class, IContentDefinition
        {
            if (TryGet<T>(id, out var definition))
                return definition;

            throw new ContentNotFoundException(id);
        }

        public bool TryGet<T>(ContentId id, out T definition) where T : class, IContentDefinition
        {
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
            var errors = new List<string>();

            foreach (var definition in _definitions.Values)
            {
                if (definition is not IReferencesContent referencing)
                    continue;

                foreach (var reference in referencing.GetReferencedContent())
                {
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
        }
    }
}
