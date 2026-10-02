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
            _sets = setOffers;
            if (_sets == null && _definitions.Exists(definition => definition.Kind == BuildEntryKind.Set))
                throw new ArgumentNullException(nameof(setOffers), "A set catalog requires an explicit global offer policy.");
        }

        /// <summary>
        /// Development command: adds definitions that are not in the pool yet (e.g. profile-locked content for this run).
        /// Returns how many were added; existing ids are skipped.
        /// </summary>
        public int AddDefinitions(IEnumerable<BuildEntryDefinition> definitions)
        {
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions));
            var added = 0;
            foreach (var definition in definitions)
            {
                if (definition == null)
                    throw new ArgumentException("Draft pool cannot contain null definitions.", nameof(definitions));
                if (_definitions.Exists(item => item.Id == definition.Id))
                    continue;
                if (_sets == null && definition.Kind == BuildEntryKind.Set)
                    throw new ArgumentNullException(nameof(definitions), "A set catalog requires an explicit global offer policy.");
                _definitions.Add(definition);
                added++;
            }
            return added;
        }

        /// <summary>Finds a pool definition, including ones not yet in the build (UI component names).</summary>
        public bool TryGetDefinition(ContentId id, out BuildEntryDefinition definition)
        {
            definition = _definitions.Find(item => item.Id == id);
            return definition != null;
        }

        /// <summary>True when the pool can ever offer this id: it is in the pool and has a positive draft weight.</summary>
        public bool CanOffer(ContentId id)
        {
            var definition = _definitions.Find(item => item.Id == id);
            return definition != null && GetDraftWeight(definition) > 0f;
        }

        public IReadOnlyList<DraftOption> CreateOptions(PlayerBuild build, int offerCount, int offset = 0) =>
            CreateOptions(build, offerCount, new SeededDraftRandom(offset));

        public bool HasEligibleOptions(PlayerBuild build, IReadOnlyCollection<ContentId> banishedIds) =>
            Eligible(build, banishedIds).Count > 0;

        public IReadOnlyList<DraftOption> CreateOptions(PlayerBuild build, int offerCount, IDraftRandom random) =>
            CreateOptions(build, offerCount, random, null);

        public IReadOnlyList<DraftOption> CreateOptions(PlayerBuild build, int offerCount,
            IDraftRandom random, IReadOnlyCollection<ContentId> banishedIds, SetDraftCheckState setChecks = null, int minimumActiveSkills = 0) =>
            CreateOptionsCore(build, offerCount, random, banishedIds, setChecks, minimumActiveSkills, out _, out _).AsReadOnly();

        public IReadOnlyList<DraftOption> CreateRerolledOptions(PlayerBuild build, int offerCount,
            IDraftRandom random, IReadOnlyCollection<ContentId> banishedIds, IReadOnlyList<DraftOption> currentOptions, SetDraftCheckState setChecks = null, int minimumActiveSkills = 0)
        {
            if (currentOptions == null) throw new ArgumentNullException(nameof(currentOptions));
            var options = CreateOptionsCore(build, offerCount, random, banishedIds, setChecks, minimumActiveSkills, out var alternatives, out var priorityCount);
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
            SetDraftCheckState setChecks, int minimumActiveSkills, out List<DraftOption> alternatives, out int priorityCount)
        {
            NumericValidation.ValidateCount(offerCount, nameof(offerCount));
            if (random == null) throw new ArgumentNullException(nameof(random));
            var ordinary = new List<DraftOption>();
            var sets = new List<DraftOption>();
            foreach (var option in Eligible(build, banishedIds))
                (option.Definition.Kind == BuildEntryKind.Set ? sets : ordinary).Add(option);

            sets.Sort((a, b) => string.CompareOrdinal(a.Definition.Id.ToString(), b.Definition.Id.ToString()));
            var preferred = sets.Count == 0 ? Array.Empty<DraftOption>() :
                (setChecks ?? new SetDraftCheckState()).GetPriorityOffers(sets.AsReadOnly(), _sets, random);
            var result = new List<DraftOption>(offerCount);
            var seen = new HashSet<ContentId>();
            foreach (var option in preferred)
            {
                if (result.Count == offerCount) break;
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
            EnsureActiveSkills(result, alternatives, minimumActiveSkills, random, priorityCount);
            return result;
        }

        // Early-run guarantee: swap offers that are not active skills for eligible active skills until the minimum is met
        // (or no more active skills are eligible). Priority set offers keep their slots.
        private static void EnsureActiveSkills(List<DraftOption> result, List<DraftOption> alternatives,
            int minimum, IDraftRandom random, int priorityCount)
        {
            if (minimum <= 0) return;
            minimum = Math.Min(minimum, result.Count);
            var have = 0;
            foreach (var option in result)
                if (option.Definition.Kind == BuildEntryKind.ActiveSkill) have++;
            while (have < minimum)
            {
                var candidates = new List<int>();
                for (var i = 0; i < alternatives.Count; i++)
                    if (alternatives[i].Definition.Kind == BuildEntryKind.ActiveSkill) candidates.Add(i);
                if (candidates.Count == 0) return;
                var slot = -1;
                for (var i = result.Count - 1; i >= priorityCount && slot < 0; i--)
                    if (result[i].Definition.Kind == BuildEntryKind.PassiveItem) slot = i;
                for (var i = result.Count - 1; i >= priorityCount && slot < 0; i--)
                    if (result[i].Definition.Kind != BuildEntryKind.ActiveSkill) slot = i;
                if (slot < 0) return;
                var pick = candidates[Math.Min(candidates.Count - 1, (int)(random.NextFloat01() * candidates.Count))];
                var replaced = result[slot];
                result[slot] = alternatives[pick];
                alternatives[pick] = replaced;
                have++;
            }
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
            // DECISION-0089: character weights apply to active skills and passive items; sets keep weight 1.
            if (_character == null ||
                definition.Kind != BuildEntryKind.ActiveSkill && definition.Kind != BuildEntryKind.PassiveItem)
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
