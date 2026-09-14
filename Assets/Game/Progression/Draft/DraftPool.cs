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

        public IReadOnlyList<DraftOption> CreateOptions(PlayerBuild build, int offerCount, IDraftRandom random)
        {
            return CreateOptions(build, offerCount, random, null);
        }

        public IReadOnlyList<DraftOption> CreateOptions(
            PlayerBuild build,
            int offerCount,
            IDraftRandom random,
            IReadOnlyCollection<ContentId> banishedIds)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            var eligible = CreateEligibleOptions(build, offerCount, banishedIds);
            if (eligible.Count <= offerCount)
                return eligible;

            for (var i = 0; i < offerCount; i++)
            {
                var selectedIndex = i + random.NextInt(eligible.Count - i);
                (eligible[i], eligible[selectedIndex]) = (eligible[selectedIndex], eligible[i]);
            }
            return eligible.GetRange(0, offerCount);
        }

        public IReadOnlyList<DraftOption> CreateRerolledOptions(
            PlayerBuild build,
            int offerCount,
            IDraftRandom random,
            IReadOnlyCollection<ContentId> banishedIds,
            IReadOnlyList<DraftOption> currentOptions)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));
            if (currentOptions == null)
                throw new ArgumentNullException(nameof(currentOptions));

            var eligible = CreateEligibleOptions(build, offerCount, banishedIds);
            if (eligible.Count <= offerCount)
                return eligible;

            for (var i = 0; i < offerCount; i++)
            {
                var selectedIndex = i + random.NextInt(eligible.Count - i);
                (eligible[i], eligible[selectedIndex]) = (eligible[selectedIndex], eligible[i]);
            }

            var currentIds = new HashSet<ContentId>();
            for (var i = 0; i < currentOptions.Count; i++)
                currentIds.Add(currentOptions[i].Definition.Id);

            var sameSet = currentOptions.Count == offerCount;
            for (var i = 0; sameSet && i < offerCount; i++)
                sameSet = currentIds.Contains(eligible[i].Definition.Id);

            if (sameSet)
            {
                for (var i = offerCount; i < eligible.Count; i++)
                {
                    if (currentIds.Contains(eligible[i].Definition.Id))
                        continue;
                    eligible[offerCount - 1] = eligible[i];
                    break;
                }
            }

            return eligible.GetRange(0, offerCount);
        }

        private List<DraftOption> CreateEligibleOptions(
            PlayerBuild build,
            int offerCount,
            IReadOnlyCollection<ContentId> banishedIds = null)
        {
            if (build == null)
                throw new ArgumentNullException(nameof(build));
            if (offerCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(offerCount));

            var eligible = new List<DraftOption>();
            foreach (var definition in _definitions)
            {
                if (Contains(banishedIds, definition.Id))
                    continue;
                if (!build.IsEligible(definition))
                    continue;

                var isUpgrade = build.TryGetEntry(definition.Id, out var entry);
                eligible.Add(new DraftOption(
                    definition,
                    isUpgrade,
                    isUpgrade ? entry.Level + 1 : 1));
            }
            return eligible;
        }

        private static bool Contains(IReadOnlyCollection<ContentId> ids, ContentId id)
        {
            if (ids == null)
                return false;
            if (ids is HashSet<ContentId> set)
                return set.Contains(id);
            foreach (var candidate in ids)
            {
                if (candidate == id)
                    return true;
            }
            return false;
        }

        private static int PositiveModulo(int value, int divisor)
        {
            var result = value % divisor;
            return result < 0 ? result + divisor : result;
        }
    }
}
