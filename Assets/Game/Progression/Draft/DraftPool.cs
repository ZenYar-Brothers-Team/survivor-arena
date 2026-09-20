using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Progression
{
    public sealed class DraftPool
    {
        private readonly List<BuildEntryDefinition> _definitions;
        private readonly CharacterDefinition _character;
        private readonly ISetDraftOfferProvider _sets;

        public DraftPool(IEnumerable<BuildEntryDefinition> definitions, CharacterDefinition character = null, ISetDraftOfferProvider setOffers = null)
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

            _character = character;
            _sets = setOffers ?? new FixtureSetDraftOfferProvider();
        }

        public IReadOnlyList<DraftOption> CreateOptions(PlayerBuild build, int offerCount, int offset = 0) =>
            CreateOptions(build, offerCount, new SeededDraftRandom(offset));

        public bool HasEligibleOptions(PlayerBuild build, IReadOnlyCollection<ContentId> banishedIds) =>
            Eligible(build, banishedIds).Count > 0;

        public IReadOnlyList<DraftOption> CreateOptions(PlayerBuild build, int offerCount, IDraftRandom random) =>
            CreateOptions(build, offerCount, random, null);

        public IReadOnlyList<DraftOption> CreateOptions(PlayerBuild build, int offerCount,
            IDraftRandom random, IReadOnlyCollection<ContentId> banishedIds) =>
            CreateOptionsCore(build, offerCount, random, banishedIds, out _, out _).AsReadOnly();

        public IReadOnlyList<DraftOption> CreateRerolledOptions(PlayerBuild build, int offerCount,
            IDraftRandom random, IReadOnlyCollection<ContentId> banishedIds, IReadOnlyList<DraftOption> currentOptions)
        {
            if (currentOptions == null) throw new ArgumentNullException(nameof(currentOptions));
            var options = CreateOptionsCore(build, offerCount, random, banishedIds, out var alternatives, out var priorityCount);
            var currentIds = new HashSet<ContentId>();
            foreach (var option in currentOptions) currentIds.Add(option.Definition.Id);
            var same = currentOptions.Count == options.Count;
            foreach (var option in options) same &= currentIds.Contains(option.Definition.Id);
            if (same && options.Count > priorityCount)
            {
                foreach (var alternative in alternatives)
                {
                    if (currentIds.Contains(alternative.Definition.Id)) continue;
                    options[options.Count - 1] = alternative;
                    break;
                }
            }
            return options.AsReadOnly();
        }

        private List<DraftOption> CreateOptionsCore(PlayerBuild build, int offerCount,
            IDraftRandom random, IReadOnlyCollection<ContentId> banishedIds,
            out List<DraftOption> alternatives, out int priorityCount)
        {
            NumericValidation.ValidateCount(offerCount, nameof(offerCount));
            if (random == null) throw new ArgumentNullException(nameof(random));
            var ordinary = new List<DraftOption>();
            var sets = new List<DraftOption>();
            foreach (var option in Eligible(build, banishedIds))
                (option.Definition.Kind == BuildEntryKind.Set ? sets : ordinary).Add(option);

            var preferred = _sets.SelectPriorityOffers(sets.AsReadOnly(), offerCount, random)
                ?? throw new InvalidOperationException("Set provider returned null.");
            var result = new List<DraftOption>(offerCount);
            var seen = new HashSet<ContentId>();
            foreach (var option in preferred)
            {
                var index = sets.FindIndex(candidate => ReferenceEquals(candidate.Definition, option.Definition));
                if (index < 0 || result.Count == offerCount || !seen.Add(option.Definition.Id))
                    throw new InvalidOperationException("Set provider must return distinct eligible inputs within capacity.");
                // Use the authoritative immutable preview, never a provider-authored level.
                result.Add(sets[index]);
            }
            priorityCount = result.Count;
            var ordinaryCount = Math.Min(offerCount - result.Count, ordinary.Count);
            if (ordinaryCount > 0 && ordinary.Count > ordinaryCount)
                SelectWeightedWithoutReplacement(ordinary, ordinaryCount, random);
            for (var i = 0; i < ordinaryCount; i++) result.Add(ordinary[i]);
            alternatives = ordinary.GetRange(ordinaryCount, ordinary.Count - ordinaryCount);

            if (result.Count < offerCount)
            {
                sets.RemoveAll(option => seen.Contains(option.Definition.Id));
                // DECISION-0019: uniform sampling without replacement, never another chance check.
                var count = Math.Min(offerCount - result.Count, sets.Count);
                for (var i = 0; i < count; i++)
                {
                    var index = i + Math.Min(sets.Count - i - 1, (int)(random.NextFloat01() * (sets.Count - i)));
                    (sets[i], sets[index]) = (sets[index], sets[i]);
                    result.Add(sets[i]);
                }
                alternatives.AddRange(sets.GetRange(count, sets.Count - count));
            }
            return result;
        }

        private List<DraftOption> Eligible(PlayerBuild build, IReadOnlyCollection<ContentId> banishedIds)
        {
            if (build == null) throw new ArgumentNullException(nameof(build));
            var result = new List<DraftOption>();
            foreach (var definition in _definitions)
            {
                if (Contains(banishedIds, definition.Id) || !build.IsEligible(definition) || GetDraftWeight(definition) <= 0f)
                    continue;
                var upgrade = build.TryGetEntry(definition.Id, out var entry);
                result.Add(new DraftOption(definition, upgrade, upgrade ? entry.Level + 1 : 1));
            }
            return result;
        }

        private void SelectWeightedWithoutReplacement(
            List<DraftOption> eligible,
            int selectionCount,
            IDraftRandom random)
        {
            for (var i = 0; i < selectionCount; i++)
            {
                var totalWeight = 0f;
                for (var candidateIndex = i; candidateIndex < eligible.Count; candidateIndex++)
                    totalWeight += GetDraftWeight(eligible[candidateIndex].Definition);

                var roll = random.NextFloat01() * totalWeight;
                var selectedIndex = eligible.Count - 1;
                for (var candidateIndex = i; candidateIndex < eligible.Count; candidateIndex++)
                {
                    roll -= GetDraftWeight(eligible[candidateIndex].Definition);
                    if (roll < 0f)
                    {
                        selectedIndex = candidateIndex;
                        break;
                    }
                }

                (eligible[i], eligible[selectedIndex]) = (eligible[selectedIndex], eligible[i]);
            }
        }

        private float GetDraftWeight(BuildEntryDefinition definition)
        {
            if (_character == null || definition.Kind != BuildEntryKind.ActiveSkill)
                return 1f;
            return _character.GetDraftWeight(definition.Id);
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

    }
}
