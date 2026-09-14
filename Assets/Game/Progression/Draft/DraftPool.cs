using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Progression
{
    public sealed class DraftPool
    {
        private readonly List<BuildEntryDefinition> _definitions;

        public DraftPool(IEnumerable<BuildEntryDefinition> definitions)
        {
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions));

            _definitions = new List<BuildEntryDefinition>();
            var ids = new HashSet<ContentId>();
            foreach (var definition in definitions)
            {
                if (definition == null)
                    throw new ArgumentException("Draft pool cannot contain null definitions.", nameof(definitions));
                if (!ids.Add(definition.Id))
                    throw new ArgumentException($"Duplicate draft content id '{definition.Id}'.", nameof(definitions));
                _definitions.Add(definition);
            }

            if (_definitions.Count == 0)
                throw new ArgumentException("Draft pool cannot be empty.", nameof(definitions));
        }

        public IReadOnlyList<DraftOption> CreateOptions(PlayerBuild build, int offerCount, int offset = 0)
        {
            if (build == null)
                throw new ArgumentNullException(nameof(build));
            if (offerCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(offerCount));

            var eligible = new List<DraftOption>();
            foreach (var definition in _definitions)
            {
                if (!build.IsEligible(definition))
                    continue;

                var isUpgrade = build.TryGetEntry(definition.Id, out var entry);
                eligible.Add(new DraftOption(
                    definition,
                    isUpgrade,
                    isUpgrade ? entry.Level + 1 : 1));
            }

            if (eligible.Count <= offerCount)
                return eligible;

            var options = new List<DraftOption>(offerCount);
            var start = PositiveModulo(offset, eligible.Count);
            for (var i = 0; i < offerCount; i++)
                options.Add(eligible[(start + i) % eligible.Count]);
            return options;
        }

        private static int PositiveModulo(int value, int divisor)
        {
            var result = value % divisor;
            return result < 0 ? result + divisor : result;
        }
    }
}
