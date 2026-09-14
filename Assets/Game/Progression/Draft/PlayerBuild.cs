using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Progression
{
    public sealed class PlayerBuild
    {
        public const int ActiveSlotCapacity = 6;
        public const int PassiveSlotCapacity = 6;

        private readonly Dictionary<ContentId, BuildEntry> _entries =
            new Dictionary<ContentId, BuildEntry>();

        public IEnumerable<BuildEntry> Entries => _entries.Values;
        public int ActiveCount { get; private set; }
        public int PassiveCount { get; private set; }

        public PlayerBuild(BuildEntryDefinition startingActiveSkill)
        {
            if (startingActiveSkill == null)
                throw new ArgumentNullException(nameof(startingActiveSkill));
            if (startingActiveSkill.Kind != BuildEntryKind.ActiveSkill)
                throw new ArgumentException("The starting build entry must be an active skill.", nameof(startingActiveSkill));

            Acquire(startingActiveSkill);
        }

        public bool TryGetEntry(ContentId id, out BuildEntry entry)
        {
            return _entries.TryGetValue(id, out entry);
        }

        public bool IsEligible(BuildEntryDefinition definition)
        {
            if (definition == null)
                return false;

            if (_entries.TryGetValue(definition.Id, out var existing))
                return existing.Definition.Kind == definition.Kind && !existing.IsMaxLevel;

            return definition.Kind == BuildEntryKind.ActiveSkill
                ? ActiveCount < ActiveSlotCapacity
                : PassiveCount < PassiveSlotCapacity;
        }

        public BuildSelectionResult Apply(BuildEntryDefinition definition)
        {
            if (!IsEligible(definition))
                throw new InvalidOperationException("Build entry is not eligible for acquisition or upgrade.");

            if (_entries.TryGetValue(definition.Id, out var existing))
            {
                existing.Upgrade();
                return new BuildSelectionResult(existing, wasNewEntry: false);
            }

            return new BuildSelectionResult(Acquire(definition), wasNewEntry: true);
        }

        private BuildEntry Acquire(BuildEntryDefinition definition)
        {
            var entry = new BuildEntry(definition);
            _entries.Add(definition.Id, entry);
            if (definition.Kind == BuildEntryKind.ActiveSkill)
                ActiveCount++;
            else
                PassiveCount++;
            return entry;
        }
    }
}
